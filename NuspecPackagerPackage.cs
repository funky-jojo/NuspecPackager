using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE80;
using EnvDTE;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace LandOfJoe.NuspecPackager
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.3", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    //this is needed to cause Visual Studio to initialize the package immediately
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
    [Guid(GuidList.guidNuspecPackagerPkgString)]
    [ProvideOptionPage(typeof(NuspecPackagerOptionPageGrid),
         "Nuspec Packager", "General", 0, 0, true)]
    public sealed class NuspecPackagerPackage 
        : Package
    {

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public NuspecPackagerPackage()
        {
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            try
            {
                base.Initialize();

                // save instance
                Instance = this;

                // Add our command handlers for menu (commands must exist in the .vsct file)
                OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
                if (null != mcs)
                {
                    // Create the command for the menu item.
                    CommandID menuCommandID = new CommandID(GuidList.guidNuspecPackagerCmdSet, (int)PkgCmdIDList.cmdidMyCommand);
                    OleMenuCommand menuItem = new OleMenuCommand(PackageMenuItemCallback, menuCommandID);
                    menuItem.BeforeQueryStatus += new EventHandler(OnBeforeQueryStatus);
                    mcs.AddCommand(menuItem);

                    // Create the command for the menu item.
                    menuCommandID = new CommandID(GuidList.guidNuspecPackagerCmdSet, (int)PkgCmdIDList.cmdidPackageFromProject);
                    menuItem = new OleMenuCommand(PackageProjectMenuCallback, menuCommandID);
                    menuItem.BeforeQueryStatus += new EventHandler(OnBeforeQueryStatus);
                    mcs.AddCommand(menuItem);

                    // Create the command for the menu item.
                    menuCommandID = new CommandID(GuidList.guidNuspecPackagerCmdSet, (int)PkgCmdIDList.cmdidPackageSymbols);
                    menuItem = new OleMenuCommand(PackageSymbolsMenuCallback, menuCommandID);
                    menuItem.BeforeQueryStatus += new EventHandler(OnBeforeQueryStatus);
                    mcs.AddCommand(menuItem);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Exception during Initialize() of {0}: {1}", this.ToString(), ex.Message));
            }
        }
        void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            try
            {
                OleMenuCommand menuCommand = sender as OleMenuCommand;
                if (menuCommand != null)
                {
                    var items = GetSelectedItems();
                    menuCommand.Visible = items.All(m => m.Extension == ".nuspec");

                    if (menuCommand.CommandID.Equals(new CommandID(GuidList.guidNuspecPackagerCmdSet, (int)PkgCmdIDList.cmdidPackageSymbols)))
                    {
                        var optionPage = GetOptionsPage();
                        menuCommand.Visible = menuCommand.Visible && optionPage.ShowPackageSymbolsMenu;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Exception during OnBeforeQueryStatus() of {0}: {1}", this.ToString(), ex.Message));
            }
        }
        #endregion



        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void PackageMenuItemCallback(object sender, EventArgs e)
        {
            var items = GetSelectedItems();
            PackageNuspecFiles(items);
        }

        private void PackageProjectMenuCallback(object sender, EventArgs e)
        {
            var items = GetSelectedItems();
            PackageNuspecFiles(items, "", true);
        }

        /// <summary>
        /// This function is called when the Package .nuspec (Symbols) menu is calicked
        /// </summary>
        private void PackageSymbolsMenuCallback(object sender, EventArgs e)
        {
            var items = GetSelectedItems();
            PackageNuspecFiles(items, "-Symbols");
        }

        #region package nuspec files
        private void PackageNuspecFiles(List<NuspecItemInfo> nuspecItems, string additionalOptions = "", bool buildFromProject = false)
        {
            Logger.Clear();
            WriteOutput("Nuspec Packager starting...", true);
            var hasErrors = false;
            try
            {
                foreach (var item in nuspecItems)
                {
                    WriteOutput("Processing nuspec file: " + item.FileName);

                    //get configuration for this nuspec file and make sure it is valid
                    NuspecItemConfig itemConfig = GetItemConfig(item);
                    if (!ValidateOptions(item, itemConfig))
                    {
                        hasErrors = true;
                        WriteOutput("Skipping nuspec file: " + item.Name);
                        continue;
                    }

                    //build the project
                    var buildSuccess = BuildProject(item);
                    if (!buildSuccess)
                    {
                        hasErrors = true;
                        WriteOutput("Skipping nuspec file: " + item.Name);
                        continue;
                    }

                    var outputPkgPath = "";
                    if (buildFromProject || (itemConfig.PackFromProject ?? false))
                    {
                        var actualFileToProcess = item.ProjectPath;
                        WriteOutput("Handling file: " + actualFileToProcess);
                        hasErrors = !Pack(additionalOptions, new NuspecItemInfo()
                        {
                            FileName = actualFileToProcess, // item.ProjectItem.Properties.Item("FullPath").Value,
                            ProjectPath = item.ProjectPath,
                            ProjectUniqueName = item.ProjectUniqueName,
                            ProjectName = item.ProjectName
                        }, itemConfig, ref outputPkgPath) || hasErrors;
                    }
                    else
                    {
                        //process the nuspec file and keep track if any errors occur
                        var actualFileToProcess = item.FileName;
                        WriteOutput("Handling file: " + actualFileToProcess);
                        hasErrors = !Pack(additionalOptions, item, itemConfig, ref outputPkgPath) || hasErrors;
                    }

                    WriteOutput($"Trying to upload {outputPkgPath}...{itemConfig.UploadToFeed}");
                    if ((itemConfig.UploadToFeed ?? false) && !hasErrors && !string.IsNullOrEmpty(outputPkgPath))
                    {
                        hasErrors = !this.PublishPack(additionalOptions, item, outputPkgPath, itemConfig) || hasErrors;
                    }
                }
            }

            catch (Exception ex)
            {
                var message = string.Format(CultureInfo.CurrentCulture, "Exception during NuspecPackagerFiles() of {0}: {1}", this.ToString(), ex.Message);
                WriteOutput(message);
                MessageBoxHelper.ShowMessageBox(message, OLEMSGICON.OLEMSGICON_CRITICAL);
                hasErrors = true;
            }

            //display final result
            var msg = "Nuspec Packager finished " + (hasErrors ? "with errors." : "successfully.");
            WriteOutput(msg, showInStatus: true);
        }
        #endregion

        #region Build the project that the nuspec file is in


        bool _myBuildInProgress = false;
        bool _overallBuildSuccess = false;

        /// <summary>
        /// build the current project before packaging
        /// </summary>
        /// <returns>true, if successful</returns>
        private bool BuildProject(NuspecItemInfo item)
        {
            var optionPage = GetOptionsPage();

            if (!optionPage.BuildBeforePackaging)
            {
                return true;
            }

            WriteOutput("Building project: " + item.ProjectName, true);

            var dte = (DTE2)GetService(typeof(SDTE));
            dte.Events.BuildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjConfigDone;
            dte.Events.BuildEvents.OnBuildDone += BuildEvents_OnBuildDone;
            var solutionFile = dte.Solution.FullName;
            _myBuildInProgress = true;
            _overallBuildSuccess = true;

            var activeConfigurationName = dte.Solution.SolutionBuild.ActiveConfiguration.Name;
            dte.Solution.SolutionBuild.BuildProject(activeConfigurationName, item.ProjectUniqueName, true);

            dte.Events.BuildEvents.OnBuildProjConfigDone -= BuildEvents_OnBuildProjConfigDone;
            dte.Events.BuildEvents.OnBuildDone -= BuildEvents_OnBuildDone;

            return _overallBuildSuccess;
        }

        void BuildEvents_OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
        {
            if (!_myBuildInProgress)
            {
                return;
            }
            _myBuildInProgress = false;
        }

        void BuildEvents_OnBuildProjConfigDone(string Project, string ProjectConfig, string Platform, string SolutionConfig, bool Success)
        {
            if (!_myBuildInProgress)
            {
                return;
            }
            _overallBuildSuccess = _overallBuildSuccess && Success;
            WriteOutput("Build " + Project + (Success ? " succeeded" : " failed"), true);
        }


        #endregion

        #region validation of config options
        private bool ValidateOptions(NuspecItemInfo item, NuspecItemConfig itemConfig)
        {
            if (!File.Exists(itemConfig.NuGetExe))
            {
                WriteOutput("NuspecPackager configuration error: The path to NuGet.exe is not valid: " + itemConfig.NuGetExe);
                return false;
            }

            //clean up path a little
            itemConfig.OutputPath = itemConfig.OutputPath.TrimEnd(new[] { '/', '\\' });

            if (!Directory.Exists(itemConfig.OutputPath))
            {
                //try to create it
                WriteOutput("NuspecPackager configuration error: The output directory does not exist, and could not be created.");
                if (!String.IsNullOrWhiteSpace(itemConfig.OutputPath))
                {
                    try
                    {
                        WriteOutput("NuspecPackager configuration warning: Trying to create the output directory: " + itemConfig.OutputPath);
                        Directory.CreateDirectory(itemConfig.OutputPath);
                    }
                    catch { }
                }
                if (!Directory.Exists(itemConfig.OutputPath))
                {
                    WriteOutput("NuspecPackager configuration error: The output directory does not exist, and could not be created.");
                    return false;
                }
            }
            return true;
        }
        #endregion

        private NuspecItemConfig GetItemConfig(NuspecItemInfo item)
        {
            var dte = (DTE2)GetService(typeof(SDTE));
            var optionPage = GetOptionsPage();
            //get the config options from VS Options Dialog
            var defaultConfig = new NuspecItemConfig
            {
                NuGetExe = optionPage.NuGetExeDir,
                OutputPath = optionPage.DefaultOutputPath,
                PackFromProject = optionPage.PackFromProject,
                AppendV2ApiTrait = optionPage.AppendV2ApiTrait,
                RemoteFeedApiKey = optionPage.RemoteFeedApiKey,
                PublishUrl = optionPage.PublishUrl,
                UploadToFeed = optionPage.UploadToFeed
            };

            if (!String.IsNullOrEmpty(optionPage.NuGetExeDir))
            {
                //use global config as default nuget exe dir
                defaultConfig.NuGetExe = Path.Combine(optionPage.NuGetExeDir, "NuGet.exe");
            }
            else
            {
                //default path is at same level as item
                defaultConfig.NuGetExe = Path.Combine(item.Directory, "NuGet.exe");

                //if exe not there, then let default path be at .nuget folder at solution level
                if (!File.Exists(defaultConfig.NuGetExe))
                {
                    WriteOutput("Could not find nuget.exe at: " + defaultConfig.NuGetExe);

                    defaultConfig.NuGetExe = Path.Combine(Path.GetDirectoryName(dte.Solution.FullName), ".nuget\\NuGet.exe");

                    if (!File.Exists(defaultConfig.NuGetExe))
                    {
                        WriteOutput("Could not find nuget.exe at: " + defaultConfig.NuGetExe);
                        WriteOutput("Create a NuspecPackager.config file or set the NugetExeDir property in the Visual Studio options page to specify a directory where nuget.exe is located.");
                    }
                }
            }


            //get config otions from folder's default config file
            var folderConfig = Util.GetDirectoryConfig(item);

            //get config options for this item's config file
            var itemConfig = Util.GetNuspecItemConfig(item);

            //merge properties from folder and default into item config's empty properties
            itemConfig.MergeFrom(folderConfig);
            itemConfig.MergeFrom(defaultConfig);
            itemConfig.EnsureAbsolutePaths(item);

            return itemConfig;
        }

        /// <summary>
        /// package the nuspec file
        /// </summary>
        /// <returns>true, if successful</returns>
        private bool Pack(string additionalOptions, NuspecItemInfo item, NuspecItemConfig itemConfig, ref string outputFile)
        {
            WriteOutput("Packing nuspec file: " + item.FileName);

            var startInfo = new ProcessStartInfo(itemConfig.NuGetExe);
            startInfo.Arguments = $"pack \"{item.FileName}\" -NoDefaultExcludes -OutputDirectory \"{itemConfig.OutputPath}\" {additionalOptions}";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            var process = System.Diagnostics.Process.Start(startInfo);

            process.WaitForExit();

            var output = process.StandardOutput.ReadToEnd();
            WriteOutput(output);

            if (process.ExitCode == 0)
            {
                var regx = new Regex(@"(')([^']+)\1");
                outputFile = regx.Matches(output).Cast<Match>().Select(m => m.Groups[2].Value).Last();
                WriteOutput("Successfully packed nuspec file: " + item.FileName);
                return true;
            }
            else
            {
                outputFile = null;
                var error = process.StandardError.ReadToEnd();
                WriteOutput("Error packing nuspec file: " + item.FileName + Environment.NewLine + "ERROR: " + error);
                return false;
            }
        }

        /// <summary>
        /// package the nuspec file
        /// </summary>
        /// <returns>true, if successful</returns>
        private bool PublishPack(string additionalOptions, NuspecItemInfo item, string pkgFullPath, NuspecItemConfig itemConfig)
        {
            WriteOutput($"Uploading nuspec file: {pkgFullPath}");

            var startInfo = new ProcessStartInfo(itemConfig.NuGetExe);
            var publishUrlAppend = (itemConfig.AppendV2ApiTrait ?? false) ? "api/v2/package" : "";
            startInfo.Arguments = $"push {pkgFullPath} {itemConfig.RemoteFeedApiKey} -Source {itemConfig.PublishUrl}{publishUrlAppend} {additionalOptions}";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            var process = System.Diagnostics.Process.Start(startInfo);

            process.WaitForExit();

            var output = process.StandardOutput.ReadToEnd();
            WriteOutput(output);

            if (process.ExitCode == 0)
            {
                WriteOutput("Successfully published nupkg file: " + pkgFullPath);
                return true;
            }
            else
            {
                var error = process.StandardError.ReadToEnd();
                WriteOutput("Error publish nupkg file: " + pkgFullPath + Environment.NewLine + "ERROR: " + error);
                return false;
            }
        }

        private void WriteOutput(string message, bool showInStatus = false)
        {
            // write using alternative method
            WriteToGeneralOutputWindow(message);
            if (showInStatus)
            {
                ShowStatus(message);
            }
        }

        /// <summary>
        /// Write to VS output pane - 
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="message"></param>
        private void WriteToGeneralOutputWindow(string message)
        {
            try
            {
                Logger.Log(message);
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("NuSpecPackager", message);
                EventLog.WriteEntry("NuSpecPackager", e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        /// <summary>
        /// Return single instance of NuspecPackager
        /// </summary>
        public static NuspecPackagerPackage Instance { get; private set; }

        private void ShowStatus(string message)
        {
            var dte = (DTE2)GetService(typeof(SDTE));
            dte.StatusBar.Text = message;
        }

        private List<NuspecItemInfo> GetSelectedItems()
        {
            var list = new List<NuspecItemInfo>();
            try
            {
                var dte = (DTE2)GetService(typeof(SDTE));

                foreach (var comItem in dte.SelectedItems)
                {
                    dynamic item = comItem;
                    var itemName = item.Name as string;
                    list.Add(new NuspecItemInfo
                    {
                        FileName = item.ProjectItem.Properties.Item("FullPath").Value,
                        ProjectPath = item.ProjectItem.ContainingProject.FullName,
                        ProjectUniqueName = item.ProjectItem.ContainingProject.UniqueName,
                        ProjectName = item.ProjectItem.ContainingProject.Name
                    });
                }
            }
            catch
            {
                //ignore
            }
            return list;
        }

        private NuspecPackagerOptionPageGrid GetOptionsPage()
        {
            var optionPage = (NuspecPackagerOptionPageGrid)GetDialogPage(typeof(NuspecPackagerOptionPageGrid));
            return optionPage;
        }

    }
}

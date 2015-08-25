using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;

namespace LandOfJoe.NuspecPackager
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false), ComVisible(true)]
    public class NuspecPackagerOptionPageGrid : DialogPage
    {

        private bool _buildBeforePackaging = true;
        [Category("Nuspec Packager")]
        [DisplayName("Build Solution Before Packaging")]
        [Description("If True, the solution will be automatically built before the package is created.")]
        public bool BuildBeforePackaging
        {
            get { return _buildBeforePackaging; }
            set { _buildBeforePackaging = value; }
        }

        private string _defaultOutputPath = "./";
        [Category("Nuspec Packager")]
        [DisplayName("Default Output Path")]
        [Description("Specifies the output folder where the nuspec file will be built.  This can be an absolute or relative path.\n\nIf a relative path is used, it will be resolved relative to the .nuspec file that is being packaged.")]
        public string DefaultOutputPath
        {
            get { return _defaultOutputPath; }
            set { _defaultOutputPath = value; }
        }

        private string _customNuGetExe = "";
        [Category("Nuspec Packager")]
        [DisplayName("Custom NuGet.exe Path")]
        [Description("Specifies a custom NuGet.exe path.\n\nIf the Use default NuGet.exe path option is checked, this custom path will be ignored.")]
        public string CustomNuGetExePath
        {
            get { return _customNuGetExe; }
            set { _customNuGetExe = value; }
        }


        private bool _useDefaultNugetPath = true;
        [Category("Nuspec Packager")]
        [DisplayName("Use default NuGet.exe path")]
        [Description("If checked, we'll look for NuGet.exe in the solution's .nuget folder.  This should be installed when NuGet Package Restore is enabled for the solution.")]
        public bool UseDefaultNuGetExePath
        {
            get { return _useDefaultNugetPath; }
            set { _useDefaultNugetPath = value; }
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            if (ValidateOptions())
            {
                base.OnApply(e);
            }
            else
            {
                e.ApplyBehavior = ApplyKind.CancelNoNavigate;
            }
        }


        /// <summary>
        /// validate the options and show message for the first invalid option.
        /// </summary>
        public bool ValidateOptions()
        {

            //can't validate relative, because it is dynamic depending on the .nuspec file being packaged
            if (Path.IsPathRooted(this.DefaultOutputPath) && !Directory.Exists(this.DefaultOutputPath))
            {
                MessageBoxHelper.ShowMessageBox("Default Output Path must specify an existing directory.  This setting can be changed in the Visual Studio Options dialog.", OLEMSGICON.OLEMSGICON_CRITICAL);
                return false;
            }

            if (!this.UseDefaultNuGetExePath && !String.IsNullOrWhiteSpace(this.CustomNuGetExePath) && !Directory.Exists(this.CustomNuGetExePath))
            {
                MessageBoxHelper.ShowMessageBox("Custom NuGet.exe Path must either be empty or specify an existing directory.  This setting can be changed in the Visual Studio Options dialog.", OLEMSGICON.OLEMSGICON_CRITICAL);
                return false;
            }

            return true;

        }

        private bool _showPackageSymbolsMenu = true;
        [Category("Nuspec Packager")]
        [DisplayName("Show Pack Symbols menu")]
        [Description("If True, adds an option to the context menu to package symbols")]
        public bool ShowPackageSymbolsMenu
        {
            get { return _showPackageSymbolsMenu; }
            set { _showPackageSymbolsMenu = value; }
        }

    }
}

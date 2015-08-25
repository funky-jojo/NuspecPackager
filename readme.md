#Nuspec Packager - Version 1.4

##Overview

The Nuspec Packager is a very simple VSIX extension designed for Visual Studio 2012 and up. It is designed specifically to provide to the ability to “pack” .nuspec files that are located in your Visual Studio solution by adding a menu item to the Context Menu in the Solution Explorer. This menu item is only visible for .nuspec files.

Note: This extension uses NuGet.exe command line utility to process the .nuspec file. This utility may be obtained as a separate download, or it is added to your solution’s .nuget folder when NuGet Package Restore is enabled for the solution.

**Note:** This version does not allow nuspec files to be contained at a solution-level; they must be part of a project. When the Package Nuspec is executed, it will build the current project before packing the .nuspec file.

##Nuspec Packager Settings

Default settings for the Nuspec Packager are specified in the Visual Studio Options dialog.

| Setting Name                            | Description   |
|-----------------------------------------|---------------|
| **Default Output Path**                    | Specifies the output folder where the nuspec file will be built. This can be an absolute or relative .  If a relative path is used, it will be resolved relative to the .nuspec file that is being packaged. |
| **Custom NuGet.exe Path**                   | Specifies a custom NuGet.exe path.  If the Use default NuGet.exe path option is checked, this custom path will be ignored. |
| **Use default NuGet.exe path** (true/false) | If checked, we'll look for NuGet.exe in the solution's .nuget folder. This should be installed when NuGet Package Restore is enabled for the solution. |

##Nuspec Packager Config File

Using a Nuspec Packager config file is optional, but can be used to override the default settings that are specified in the Visual Studio Options dialog.

Nuspec Packager config files are progressively applied at three levels:

-   Visual Studio Options &gt; Nuspec Packager settings

-   Config file for all .nuspec files in a folder

-   Config file for a particular .nuspec file within the same folder.

The following settings are available in the .config file:

| Setting Name | Description                                                                                                       |
|--------------|-------------------------------------------------------------------------------------------------------------------|
| **NuGetExe**     | Specifies the path to the NuGet.exe utility. This path can be absolute or relative.  If a relative path is used, the full path is resolved relative to the .nuspec file that is being packaged.         |
| **OutputPath**   | Specifies the directory where the resulting NuGet package will be created. This path can be absolute or relative.  If a relative path is used, the full path is resolved relative to the .nuspec file that is being packaged.         |

Any setting in the config file can be omitted. In that case the setting from the next higher precendence will be used. In other words, if a setting is omitted from a nuspec-file-specific config, it will be obtained from the folder-specific config, if one exists. Otherwise, it will be obtained from the default setting specified in the Visual Studio Options dialog.

**Config File Naming Conventions**

The config file should be named using the following naming conventions depending on the level of override needed:

To apply specific settings for all .nuspec files in a folder:

> **nuspec-packager.config**

To apply specific settings for a particular .nuspec file in the same folder:

> ***filename*.nuspec-packager.config**

**Sample Nuspec Packager config file:**

    <?xml version="1.0"?>
    <NuspecItemConfig xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
      <NuGetExe>c:\nuget.exe</NuGetExe>
      <OutputPath>..\nuget packages</OutputPath>
    </NuspecItemConfig>

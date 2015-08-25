using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandOfJoe.NuspecPackager
{
    public static class MessageBoxHelper
    {
        public static void ShowMessageBox(string message, OLEMSGICON icon)
        {
            IVsUIShell uiShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "Nuspec Packager",
                       message,
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       icon,
                       0,        // false
                       out result));
        }
    }
}

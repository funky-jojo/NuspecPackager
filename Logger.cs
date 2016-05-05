using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace LandOfJoe.NuspecPackager
{
    /// <summary>
    /// Helper class to log to output windows
    /// </summary>
    /// <remarks>
    /// Copied from VS WebEssentials2015: https://github.com/madskristensen/WebEssentials2015/blob/master/EditorExtensions/Shared/Helpers/Logger.cs
    /// </remarks>
    public static class Logger
    {
        private static IVsOutputWindowPane pane;
        private static object _syncRoot = new object();

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsOutputWindowPane.OutputString(System.String)")]
        public static void Log(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            try
            {
                if (EnsurePane())
                {
                    pane.OutputString(message + Environment.NewLine);
                }
            }
            catch
            {
                // Do nothing
            }
        }

        public static void Log(Exception ex)
        {
            if (ex != null)
            {
                Log(ex.ToString());
                //Telemetry.TrackException(ex);
            }
        }

        public static void ShowMessage(string message, string title = "NuspecPackager",
            MessageBoxButtons messageBoxButtons = MessageBoxButtons.OK,
            MessageBoxIcon messageBoxIcon = MessageBoxIcon.Warning,
            MessageBoxDefaultButton messageBoxDefaultButton = MessageBoxDefaultButton.Button1)
        {
            Log(String.Format(CultureInfo.CurrentCulture, "{0}: {1}", title, message));
        }

        private static bool EnsurePane()
        {
            if (pane == null)
            {
                lock (_syncRoot)
                {
                    if (pane == null)
                    {
                        pane = NuspecPackagerPackage.Instance.GetOutputPane(VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "NuspecPackager");
                    }
                }
            }

            pane.Activate();

            return pane != null;
        }
    }
}
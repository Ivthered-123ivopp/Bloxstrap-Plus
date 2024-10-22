using System.ComponentModel;
using System.Security.AccessControl;
using System.Windows;

namespace Bloxstrap
{
    static class Utilities
    {
        public static void ShellExecute(string website)
        {
            try
            {
                Process.Start(new ProcessStartInfo 
                { 
                    FileName = website, 
                    UseShellExecute = true 
                });
            }
            catch (Win32Exception ex)
            {
                // lmfao

                if (ex.NativeErrorCode != (int)ErrorCode.CO_E_APPNOTFOUND)
                    throw;

                Process.Start(new ProcessStartInfo
                {
                    FileName = "rundll32.exe",
                    Arguments = $"shell32,OpenAs_RunDLL {website}"
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="versionStr1"></param>
        /// <param name="versionStr2"></param>
        /// <returns>
        /// Result of System.Version.CompareTo <br />
        /// -1: version1 &lt; version2 <br />
        ///  0: version1 == version2 <br />
        ///  1: version1 &gt; version2
        /// </returns>
        public static VersionComparison CompareVersions(string versionStr1, string versionStr2)
        {
            var version1 = new Version(versionStr1.Replace("v", ""));
            var version2 = new Version(versionStr2.Replace("v", ""));

            return (VersionComparison)version1.CompareTo(version2);
        }

        public static string GetRobloxVersion(bool studio)
        {
            string fileName = studio ? "Studio/RobloxStudioBeta.exe" : "Player/RobloxPlayerBeta.exe";

            string playerLocation = Path.Combine(Paths.Roblox, fileName);

            if (!File.Exists(playerLocation))
                return "";

            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(playerLocation);

            if (versionInfo.ProductVersion is null)
                return "";

            return versionInfo.ProductVersion.Replace(", ", ".");
        }

        public static Process[] GetProcessesSafe()
        {
            const string LOG_IDENT = "Utilities::GetProcessesSafe";

            try
            {
                return Process.GetProcesses();
            }
            catch (ArithmeticException ex) // thanks microsoft
            {
                App.Logger.WriteLine(LOG_IDENT, $"Unable to fetch processes!");
                App.Logger.WriteException(LOG_IDENT, ex);
                return Array.Empty<Process>(); // can we retry?
            }
        }

        public static void RemoveTeleportFix()
        {
            const string LOG_IDENT = "Utilities::RemoveTeleportFix";

            string user = Environment.UserDomainName + "\\" + Environment.UserName;

            try
            {
                FileInfo fileInfo = new FileInfo(App.RobloxCookiesFilePath);
                FileSecurity fileSecurity = fileInfo.GetAccessControl();

                fileSecurity.RemoveAccessRule(new FileSystemAccessRule(user, FileSystemRights.Read, AccessControlType.Deny));
                fileSecurity.RemoveAccessRule(new FileSystemAccessRule(user, FileSystemRights.Write, AccessControlType.Allow));

                fileInfo.SetAccessControl(fileSecurity);

                App.Logger.WriteLine(LOG_IDENT, "Successfully removed teleport fix.");
            }
            catch (Exception ex)
            {
                Frontend.ShowExceptionDialog(ex);
            }
        }

        public static void ApplyTeleportFix()
        {
            const string LOG_IDENT = "Utilities::ApplyTeleportFix";

            string user = Environment.UserDomainName + "\\" + Environment.UserName;

            if (File.Exists(App.RobloxCookiesFilePath))
            {
                if (App.Settings.Prop.FixTeleports)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Attempting to apply teleport fix...");

                    try
                    {
                        FileInfo fileInfo = new FileInfo(App.RobloxCookiesFilePath);
                        FileSecurity fileSecurity = fileInfo.GetAccessControl();

                        fileSecurity.AddAccessRule(new FileSystemAccessRule(user, FileSystemRights.Read, AccessControlType.Deny));
                        fileSecurity.AddAccessRule(new FileSystemAccessRule(user, FileSystemRights.Write, AccessControlType.Allow));

                        fileInfo.SetAccessControl(fileSecurity);

                        App.Logger.WriteLine(LOG_IDENT, "Successfully made RobloxCookies.dat write-only.");
                    }
                    catch (Exception ex)
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Failed to make RobloxCookies.dat write-only.");
                        App.Logger.WriteException(LOG_IDENT, ex);
                        Frontend.ShowMessageBox(Strings.Utilities_ApplyTeleportFixFail, MessageBoxImage.Error);
                        Frontend.ShowExceptionDialog(ex);
                    }
                }
                else
                {
                    App.Logger.WriteLine(LOG_IDENT, "Removing teleport fix...");
                    RemoveTeleportFix();
                }
            }
            else
            {
                App.Logger.WriteLine(LOG_IDENT, $"Failed to find RobloxCookies.dat");
                Frontend.ShowMessageBox($"Failed to find RobloxCookies.dat | Path: {App.RobloxCookiesFilePath}", MessageBoxImage.Error);
            }
        }
    }
}

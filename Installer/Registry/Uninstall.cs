using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Installer.Registry
{
    public class Uninstall
    {
        public static void CreateUninstallerKeys(string installPath, string ffPath)
        {
            RegistryKey registryKey1 = !Environment.Is64BitOperatingSystem ? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32) : RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey registryKey2 = registryKey1.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\7thHeaven") ?? registryKey1.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\7thHeaven");
            try
            {
                registryKey2.SetValue("DisplayIcon", (object)(installPath + "7th Heaven.exe"));
                registryKey2.SetValue("DisplayName", (object)"7th Heaven");
                registryKey2.SetValue("InstallLocation", (object)installPath);
                registryKey2.SetValue("NoModify", (object)0);
                registryKey2.SetValue("NoRepair", (object)1);
                registryKey2.SetValue("URLInfoAbout", (object)"https://7thheaven.rocks/");
                registryKey2.SetValue("UninstallString", (object)("\"" + installPath + "setup.exe\" uninstall"));
                registryKey2.SetValue("ModifyPath", (object)("\"" + installPath + "setup.exe\" modify"));
                registryKey2.SetValue("GamePath", (object)(ffPath));
            }
            catch (Exception)
            {
            }
        }

        public static void RemoveUninstallerKeys() => (!Environment.Is64BitOperatingSystem ? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32) : RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)).DeleteSubKeyTree("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\7thHeaven");

        public static string GetInstallLocation()
        {
            RegistryKey registryKey = (!Environment.Is64BitOperatingSystem ? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32) : RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)).OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\7thHeaven");
            return registryKey != null ? (string)registryKey.GetValue("InstallLocation") : (string)null;
        }

        public static string GetGameLocation()
        {
            RegistryKey registryKey = (!Environment.Is64BitOperatingSystem ? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32) : RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)).OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\7thHeaven");
            return registryKey != null ? (string)registryKey.GetValue("GamePath") : (string)null;
        }

        public static void removeShellIntegration()
        {
            RegistryKey classesRoot = Microsoft.Win32.Registry.ClassesRoot;
            try
            {
                List<string> list = ((IEnumerable<string>)classesRoot.GetSubKeyNames()).Where<string>((Func<string, bool>)(k => k == "Directory" || k == "7thHeaven")).ToList<string>();
                if (list.Contains("Directory"))
                {
                    RegistryKey registryKey1 = classesRoot.OpenSubKey("Directory", true);
                    if (((IEnumerable<string>)registryKey1.GetSubKeyNames()).Any<string>((Func<string, bool>)(k => k == "shell")))
                    {
                        RegistryKey registryKey2 = registryKey1.OpenSubKey("shell", true);
                        if (((IEnumerable<string>)registryKey2.GetSubKeyNames()).Any<string>((Func<string, bool>)(k => k == "Pack into IRO")))
                            registryKey2.DeleteSubKeyTree("Pack into IRO");
                    }
                }
                if (list.Contains("7thHeaven"))
                {
                    RegistryKey registryKey3 = classesRoot.OpenSubKey("7thHeaven", true);
                    if (((IEnumerable<string>)registryKey3.GetSubKeyNames()).Any<string>((Func<string, bool>)(k => k == "shell")))
                    {
                        RegistryKey registryKey4 = registryKey3.OpenSubKey("shell", true);
                        if (((IEnumerable<string>)registryKey4.GetSubKeyNames()).Any<string>((Func<string, bool>)(k => k == "Unpack IRO")))
                            registryKey4.DeleteSubKeyTree("Unpack IRO");
                    }
                }
            }
            catch (Exception)
            {
            }
            try
            {
                RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes");
                if (((IEnumerable<string>)registryKey.GetSubKeyNames()).Where<string>((Func<string, bool>)(k => k == "iros")).ToList<string>().Contains("iros"))
                    registryKey.DeleteSubKeyTree("iros");
            }
            catch (Exception)
            {
            }
            try
            {
                RegistryKey registryKey5 = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes");
                List<string> list = ((IEnumerable<string>)registryKey5.GetSubKeyNames()).Where<string>((Func<string, bool>)(k => k == "7thHeaven" || k == ".iro" || k == ".irop")).ToList<string>();
                if (list.Contains("7thHeaven"))
                {
                    RegistryKey registryKey6 = registryKey5.OpenSubKey("7thHeaven", true);
                    string[] subKeyNames = registryKey6.GetSubKeyNames();
                    if (((IEnumerable<string>)subKeyNames).Any<string>((Func<string, bool>)(k => k == "shell")))
                    {
                        RegistryKey registryKey7 = registryKey6.OpenSubKey("shell", true);
                        if (((IEnumerable<string>)registryKey7.GetSubKeyNames()).Any<string>((Func<string, bool>)(k => k == "open")))
                            registryKey7.DeleteSubKeyTree("open");
                    }
                    if (((IEnumerable<string>)subKeyNames).Any<string>((Func<string, bool>)(k => k == ".iro")))
                        registryKey6.DeleteSubKeyTree(".iro");
                    if (((IEnumerable<string>)subKeyNames).Any<string>((Func<string, bool>)(k => k == "DefaultIcon")))
                        registryKey6.DeleteSubKeyTree("DefaultIcon");
                }
                if (list.Contains(".iro"))
                    registryKey5.DeleteSubKeyTree(".iro");
                if (!list.Contains(".irop"))
                    return;
                registryKey5.DeleteSubKeyTree(".irop");
            }
            catch (Exception)
            {
            }
        }
    }
}

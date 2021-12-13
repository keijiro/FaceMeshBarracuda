using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UMP;
#if UNITY_IPHONE
using UnityEditor.iOS.Xcode;
#endif

public class UMPPostBuilds : MonoBehaviour
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        var settings = UMPSettings.Instance;

        switch (buildTarget)
        {
            case BuildTarget.StandaloneWindows:
                BuildWindowsPlayer32(path, settings);
                break;

            case BuildTarget.StandaloneWindows64:
                BuildWindowsPlayer64(path, settings);
                break;

            case BuildTarget.StandaloneLinux:
                BuildLinuxPlayer32(path, settings);
                break;

            case BuildTarget.StandaloneLinux64:
                BuildLinuxPlayer64(path, settings);
                break;

            case BuildTarget.StandaloneLinuxUniversal:
                BuildLinuxPlayerUniversal(path, settings);
                break;

            case BuildTarget.iOS:
                BuildForiOS(path, settings);
                break;
        }

        if (buildTarget == (BuildTarget)2 || buildTarget == (BuildTarget)27)
            BuildMacPlayer64(path, buildTarget, settings);
    }

    private static void BuildForiOS(string path, UMPSettings settings)
    {
#if UNITY_IPHONE
        var projPath = Path.Combine(path, "Unity-iPhone.xcodeproj/project.pbxproj");
        var pbxProject = new PBXProject();
        pbxProject.ReadFromString(File.ReadAllText(projPath));

        var target = pbxProject.TargetGuidByName("Unity-iPhone");

        // Activate Background Mode for Audio
        string plistPath = path + "/Info.plist";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        PlistElementDict rootDict = plist.root;
        var buildKey = "UIBackgroundModes";
        rootDict.CreateArray(buildKey).AddString("audio");
        File.WriteAllText(plistPath, plist.WriteToString());

        var fileGuid = pbxProject.AddFile("usr/lib/" + "libc++.dylib", "Frameworks/" + "libc++.dylib", PBXSourceTree.Sdk);
        pbxProject.AddFileToBuild(target, fileGuid);
        fileGuid = pbxProject.AddFile("usr/lib/" + "libz.dylib", "Frameworks/" + "libz.dylib", PBXSourceTree.Sdk);
        pbxProject.AddFileToBuild(target, fileGuid);
        fileGuid = pbxProject.AddFile("usr/lib/" + "libz.tbd", "Frameworks/" + "libz.tbd", PBXSourceTree.Sdk);
        pbxProject.AddFileToBuild(target, fileGuid);

        fileGuid = pbxProject.AddFile("usr/lib/" + "libbz2.dylib", "Frameworks/" + "libbz2.dylib", PBXSourceTree.Sdk);
        pbxProject.AddFileToBuild(target, fileGuid);
        fileGuid = pbxProject.AddFile("usr/lib/" + "libbz2.tbd", "Frameworks/" + "libbz2.tbd", PBXSourceTree.Sdk);
        pbxProject.AddFileToBuild(target, fileGuid);

        if ((settings.PlayersIPhone & PlayerOptionsIPhone.PlayerTypes.Native) == PlayerOptionsIPhone.PlayerTypes.Native)
            pbxProject.AddFrameworkToProject(target, "CoreImage.framework", true);

        File.WriteAllText(projPath, pbxProject.WriteToString());
#endif
    }

    public static void BuildWindowsPlayer32(string path, UMPSettings settings)
    {
        string buildPath = Path.GetDirectoryName(path);
        string dataPath = buildPath + "/" + Path.GetFileNameWithoutExtension(path) + "_Data";

        if (!string.IsNullOrEmpty(buildPath))
        {
            if (!settings.UseExternalLibraries)
            {
                CopyPlugins(settings.AssetPath + "/Plugins/Win/x86/plugins/", dataPath + "/Plugins/plugins/");
            }
            else
            {
                if (File.Exists(dataPath + "/Plugins/" + UMPSettings.LIB_VLC_NAME + ".dll"))
                    File.Delete(dataPath + "/Plugins/" + UMPSettings.LIB_VLC_NAME + ".dll");

                if (File.Exists(dataPath + "/Plugins/" + UMPSettings.LIB_VLC_CORE_NAME + ".dll"))
                    File.Delete(dataPath + "/Plugins/" + UMPSettings.LIB_VLC_CORE_NAME + ".dll");
            }
        }
        Debug.Log("Standalone Windows (x86) build is completed: " + path);
    }

    public static void BuildWindowsPlayer64(string path, UMPSettings settings)
    {
        string buildPath = Path.GetDirectoryName(path);
        string dataPath = buildPath + "/" + Path.GetFileNameWithoutExtension(path) + "_Data";

        if (!string.IsNullOrEmpty(buildPath))
        {
            if (!settings.UseExternalLibraries)
            {
                CopyPlugins(settings.AssetPath + "/Plugins/Win/x86_64/plugins/", dataPath + "/Plugins/plugins/");
            }
            else
            {
                if (File.Exists(dataPath + "/Plugins/" + UMPSettings.LIB_VLC_NAME + ".dll"))
                    File.Delete(dataPath + "/Plugins/" + UMPSettings.LIB_VLC_NAME + ".dll");

                if (File.Exists(dataPath + "/Plugins/" + UMPSettings.LIB_VLC_CORE_NAME + ".dll"))
                    File.Delete(dataPath + "/Plugins/" + UMPSettings.LIB_VLC_CORE_NAME + ".dll");
            }
        }
        Debug.Log("Standalone Windows (x86_x64) build is completed: " + path);
    }

    public static void BuildMacPlayer64(string path, BuildTarget target, UMPSettings settings)
    {
        string buildPath = Path.GetDirectoryName(path);
        string dataPath = buildPath + "/" + Path.GetFileName(path) + "/Contents";

        if (target == (BuildTarget)2)
        {
            if (Directory.Exists(dataPath + "/Plugins/x86_64/" + UMPSettings.LIB_VLC_NAME + ".bundle"))
                Directory.Move(dataPath + "/Plugins/x86_64/" + UMPSettings.LIB_VLC_NAME + ".bundle", dataPath + "/Plugins/" + UMPSettings.LIB_VLC_NAME + ".bundle");

            if (Directory.Exists(dataPath + "/Plugins/x86_64/" + UMPSettings.ASSET_NAME + ".bundle"))
                Directory.Move(dataPath + "/Plugins/x86_64/" + UMPSettings.ASSET_NAME + ".bundle", dataPath + "/Plugins/" + UMPSettings.ASSET_NAME + ".bundle");
        }

        if (!string.IsNullOrEmpty(buildPath) && settings.UseExternalLibraries)
        {
            if (Directory.Exists(dataPath + "/Plugins/" + UMPSettings.LIB_VLC_NAME + ".bundle"))
                Directory.Delete(dataPath + "/Plugins/" + UMPSettings.LIB_VLC_NAME + ".bundle", true);
        }

        Debug.Log("Standalone Mac (x86_x64) build is completed: " + dataPath);
    }

    public static void BuildLinuxPlayer32(string path, UMPSettings settings)
    {
        string buildPath = Path.GetDirectoryName(path);
        string dataPath = buildPath + "/" + Path.GetFileNameWithoutExtension(path) + "_Data";
        string umpLauncherPath = settings.AssetPath + "/Plugins/Linux/UMPInstaller.sh";
        string umpRemoverPath = settings.AssetPath + "/Plugins/Linux/UMPRemover.sh";

        if (!string.IsNullOrEmpty(buildPath))
        {
            if (!settings.UseExternalLibraries)
            {
                string vlcFolderPath32 = dataPath + "/Plugins/x86/vlc";
                if (!Directory.Exists(vlcFolderPath32))
                    Directory.CreateDirectory(vlcFolderPath32);

                CopyPlugins(settings.AssetPath + "/Plugins/Linux/x86/plugins/", vlcFolderPath32 + "/plugins/");
            }
            else
            {
                if (File.Exists(dataPath + "/Plugins/x86/" + UMPSettings.LIB_VLC_NAME + ".so"))
                    File.Delete(dataPath + "/Plugins/x86/" + UMPSettings.LIB_VLC_NAME + ".so");

                if (File.Exists(dataPath + "/Plugins/x86/" + UMPSettings.LIB_VLC_CORE_NAME + ".so"))
                    File.Delete(dataPath + "/Plugins/x86/" + UMPSettings.LIB_VLC_CORE_NAME + ".so");
            }

            CopyShellScript(umpLauncherPath, buildPath);
            CopyShellScript(umpRemoverPath, buildPath);
        }
        Debug.Log("Standalone Linux (x86) build is completed: " + path);
    }

    public static void BuildLinuxPlayer64(string path, UMPSettings settings)
    {
        string buildPath = Path.GetDirectoryName(path);
        string dataPath = buildPath + "/" + Path.GetFileNameWithoutExtension(path) + "_Data";
        string umpLauncherPath = settings.AssetPath + "/Plugins/Linux/UMPInstaller.sh";
        string umpRemoverPath = settings.AssetPath + "/Plugins/Linux/UMPRemover.sh";

        if (!string.IsNullOrEmpty(buildPath))
        {
            if (!settings.UseExternalLibraries)
            {
                string vlcFolderPath64 = dataPath + "/Plugins/x86_64/vlc";
                if (!Directory.Exists(vlcFolderPath64))
                    Directory.CreateDirectory(vlcFolderPath64);

                CopyPlugins(settings.AssetPath + "/Plugins/Linux/x86_64/plugins/", vlcFolderPath64 + "/plugins/");
            }
            else
            {
                if (File.Exists(dataPath + "/Plugins/x86_64/" + UMPSettings.LIB_VLC_NAME + ".so"))
                    File.Delete(dataPath + "/Plugins/x86_64/" + UMPSettings.LIB_VLC_NAME + ".so");

                if (File.Exists(dataPath + "/Plugins/x86_64/" + UMPSettings.LIB_VLC_CORE_NAME + ".so"))
                    File.Delete(dataPath + "/Plugins/x86_64/" + UMPSettings.LIB_VLC_CORE_NAME + ".so");
            }

            CopyShellScript(umpLauncherPath, buildPath);
            CopyShellScript(umpRemoverPath, buildPath);
        }
        Debug.Log("Standalone Linux (x86_x64) build is completed: " + path);
    }

    public static void BuildLinuxPlayerUniversal(string path, UMPSettings settings)
    {
        string buildPath = Path.GetDirectoryName(path);
        string dataPath = buildPath + "/" + Path.GetFileNameWithoutExtension(path) + "_Data";
        string umpLauncherPath = settings.AssetPath + "/Plugins/Linux/UMPInstaller.sh";
        string umpRemoverPath = settings.AssetPath + "/Plugins/Linux/UMPRemover.sh";

        if (!string.IsNullOrEmpty(buildPath))
        {
            if (!settings.UseExternalLibraries)
            {
                string vlcFolderPath32 = dataPath + "/Plugins/x86/vlc";
                if (!Directory.Exists(vlcFolderPath32))
                    Directory.CreateDirectory(vlcFolderPath32);

                string vlcFolderPath64 = dataPath + "/Plugins/x86_64/vlc";
                if (!Directory.Exists(vlcFolderPath64))
                    Directory.CreateDirectory(vlcFolderPath64);

                CopyPlugins(settings.AssetPath + "/Plugins/Linux/x86/plugins/", vlcFolderPath32 + "/plugins/");
                CopyPlugins(settings.AssetPath + "/Plugins/Linux/x86_64/plugins/", vlcFolderPath64 + "/plugins/");
            }
            else
            {
                if (File.Exists(dataPath + "/Plugins/x86/" + UMPSettings.LIB_VLC_NAME + ".so"))
                    File.Delete(dataPath + "/Plugins/x86/" + UMPSettings.LIB_VLC_NAME + ".so");

                if (File.Exists(dataPath + "/Plugins/x86/" + UMPSettings.LIB_VLC_CORE_NAME + ".so"))
                    File.Delete(dataPath + "/Plugins/x86/" + UMPSettings.LIB_VLC_CORE_NAME + ".so");

                if (File.Exists(dataPath + "/Plugins/x86_64/" + UMPSettings.LIB_VLC_NAME + ".so"))
                    File.Delete(dataPath + "/Plugins/x86_64/" + UMPSettings.LIB_VLC_NAME + ".so");

                if (File.Exists(dataPath + "/Plugins/x86_64/" + UMPSettings.LIB_VLC_CORE_NAME + ".so"))
                    File.Delete(dataPath + "/Plugins/x86_64/" + UMPSettings.LIB_VLC_CORE_NAME + ".so");
            }

            CopyShellScript(umpLauncherPath, buildPath);
            CopyShellScript(umpRemoverPath, buildPath);
        }
        Debug.Log("Standalone Linux (Universal) build is completed: " + path);
    }

    private static void CopyPlugins(string sourcePath, string targetPath)
    {
        string fileName = string.Empty;
        string destFile = targetPath;

        if (!Directory.Exists(targetPath))
            Directory.CreateDirectory(targetPath);

        string[] directories = Directory.GetDirectories(sourcePath);

        foreach (var d in directories)
        {
            string[] files = Directory.GetFiles(d);

            if (files.Length > 0)
            {
                destFile = Path.Combine(targetPath, Path.GetFileName(d));
                Directory.CreateDirectory(destFile);
            }

            foreach (var s in files)
            {
                if (Path.GetExtension(s).Equals(".meta"))
                    continue;

                fileName = Path.GetFileName(s);
                File.Copy(s, Path.Combine(destFile, fileName), false);
            }
        }
    }

    private static void CopyShellScript(string scriptPath, string targetPath)
    {
        string scriptName = Path.GetFileName(scriptPath);
        File.Copy(scriptPath, Path.Combine(targetPath, scriptName), false);
    }
}

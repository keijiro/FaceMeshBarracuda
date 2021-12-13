using System.IO;
using UnityEditor;

namespace UMP.Editor
{
    public class UMPPostAssets : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            string assetNameWithExtension = string.Join(".", new string[] { UMPSettings.SETTINGS_FILE_NAME, "asset" });

            for (int i = 0; i < movedAssets.Length; i++)
            {
                if (movedFromAssetPaths[i].Equals(UMPSettings.Instance.AssetPath))
                    UMPSettings.Instance.AssetPath = movedAssets[i];
            }

            for (int i = 0; i < importedAssets.Length; i++)
            {
                if (Path.GetFileName(importedAssets[i]).Equals(assetNameWithExtension))
                {
                    UMPSettingsEditor.UpdateMobileLibraries(UMPSettings.Platforms.Android, UMPSettings.Instance.PlayersAndroid);
                    UMPSettingsEditor.UpdateMobileLibraries(UMPSettings.Platforms.iOS, UMPSettings.Instance.PlayersIPhone);
                }
            }
        }
    }
}
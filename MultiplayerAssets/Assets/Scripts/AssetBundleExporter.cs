#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Multiplayer.UnityAssets
{
    public static class AssetBundleBuilder
    {
        private const string ASSET_BUNDLE_NAME = "multiplayer";
        private const string BUNDLE_OUT_DIR = "Assets/Temp";
        private static readonly string BUNDLE_OUT_DIR_META = $"{BUNDLE_OUT_DIR}.meta";
        private static readonly string BUNDLE_OUT_PATH = $"{BUNDLE_OUT_DIR}/{ASSET_BUNDLE_NAME}";
        private static readonly string BUNDLE_MOVE_PATH = $"../build/{ASSET_BUNDLE_NAME}.assetbundle";

        [MenuItem("Multiplayer/Build Asset Bundles")]
        public static void BuildAssetBundles()
        {
            AssetBundleBuild build = new AssetBundleBuild {
                assetBundleName = ASSET_BUNDLE_NAME,
                assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(ASSET_BUNDLE_NAME)
            };

            Debug.Log("Building Asset Bundle");
            Directory.CreateDirectory(BUNDLE_OUT_DIR);
            BuildPipeline.BuildAssetBundles(BUNDLE_OUT_DIR, new[] { build }, BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.StandaloneWindows64);

            if (File.Exists(BUNDLE_MOVE_PATH))
            {
                Debug.Log("Removing existing Asset Bundle");
                File.Delete(BUNDLE_MOVE_PATH);
            }

            Debug.Log("Moving Asset Bundle to build folder");
            Directory.CreateDirectory(Path.GetDirectoryName(BUNDLE_MOVE_PATH));
            File.Move(BUNDLE_OUT_PATH, BUNDLE_MOVE_PATH);

            Debug.Log("Deleting temp dir");
            Directory.Delete(BUNDLE_OUT_DIR, true);
            File.Delete(BUNDLE_OUT_DIR_META);

            Debug.Log("Finished!");
        }
    }
}
#endif

using System.IO;
using UnityEditor;

public class AssetBundleBuild_
{
    [MenuItem("Expansion_/Build AssetBundleData")]
    public static void Build()
    {
        string assetBundleDirectory = "./AssetBundleData";      // 出力先ディレクトリ

        // 出力先ディレクトリが無かったら作る
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        // AssetBundleのビルド(ターゲット(プラットフォーム)毎に3つ目の引数が違うので注意)
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

        // ビルド終了を表示
        EditorUtility.DisplayDialog("アセットバンドルビルド終了", "アセットバンドルビルドが終わりました", "OK");
    }
}

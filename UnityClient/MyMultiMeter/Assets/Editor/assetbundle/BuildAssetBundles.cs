using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BuildAssetBundles : MonoBehaviour
{
    [MenuItem("Example/Build  ModSample1")]
    static void PrefabBundlePrefabAOnly()
    {
        // Create the array of bundle build details.
        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];


        buildMap[0].assetBundleName = "ModSample1.bundle";
        string[] prefabAssets = new string[2];
        prefabAssets[0] = "Assets/bundle/ModSample1/Thumbnail_Image.png";
        prefabAssets[1] = "Assets/bundle/ModSample1/MeterPanel.prefab";
        buildMap[0].assetNames = prefabAssets;

        //���̏ꍇ�A�A�Z�b�g�o���h��prefab_bundle_prefab_A_only�ɂ́AprefabA,image1.png���ǉ������

        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets", buildMap, BuildAssetBundleOptions.None, BuildTarget.Android);
    }
}

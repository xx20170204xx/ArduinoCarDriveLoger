using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeterUnitController))]
public class ModListPanel : MonoBehaviour
{
    [SerializeField]
    private string modFolderName;
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private GameObject contents;
    [SerializeField]
    private GameObject modPanels;

    [SerializeField]
    private PanelSelect panelSelect;


    public void OnUpdateModList()
    {
        /* 一覧のクリア */
        ClearList();

        string findModPath = Application.streamingAssetsPath + "/" + modFolderName;
        var list = Directory.GetFiles(findModPath, "*.bundle");

        foreach (var info in list)
        {
            FileInfo fileinfo = new FileInfo(info);
            var myLoadedAssetBundle = AssetBundle.LoadFromFile( info );
            if (myLoadedAssetBundle == null)
            {
                continue;
            }

            var thumbnailImage = myLoadedAssetBundle.LoadAsset<Texture>("Thumbnail_Image");
            // var modPanel = myLoadedAssetBundle.LoadAsset<GameObject>("ModPanel");
            // var panel = Instantiate(modPanel);

            myLoadedAssetBundle.Unload(false);

            if (prefab == null)
            {
                Debug.LogWarning(this.GetType().ToString() + " : " + "prefab is null."); ;
                continue;
            }
            GameObject _go = Instantiate(prefab);
            _go.transform.parent = this.contents.transform;
            _go.transform.localScale = this.contents.transform.localScale;

            var modinfo = _go.GetComponent<ModInfo>();
            modinfo.filepath = info;
            modinfo.thumbnail.texture = thumbnailImage;
        }

    } /* OnUpdateModList */

    private void ClearList()
    {
        if (contents == null)
        {
            return;
        }

        /* 一覧のクリア */
        int count = contents.transform.childCount;
        for (int ii = 0; ii < count; ii++)
        {
            var chld = contents.transform.GetChild(0);
            chld.gameObject.SetActive(false);
            chld.gameObject.transform.SetParent(null);
            Destroy(chld.gameObject);
        }

    } /* ClearList */

    public void OnReloadPanels()
    {
        var modinfoList = contents.GetComponentsInChildren<ModInfo>();
        panelSelect.UpdateModPanels(modinfoList, modPanels);
        /*
        foreach (var  modinfo in modinfoList)
        {
            var myLoadedAssetBundle = AssetBundle.LoadFromFile(modinfo.filepath);
            if (myLoadedAssetBundle == null)
            {
                continue;
            }

            GameObject prefab = myLoadedAssetBundle.LoadAsset<GameObject>("MeterPanel");
            var panel = Instantiate(prefab);
            panel.transform.parent = modPanels.transform;

            myLoadedAssetBundle.Unload(false);
        }
        */


    } /* OnReloadPanels */


}

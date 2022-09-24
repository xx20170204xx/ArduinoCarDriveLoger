using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SerialReceive))]
public class PanelSelect : MonoBehaviour
{
    [SerializeField]
    private List<MeterUnitController> units = new List<MeterUnitController>();

    [SerializeField]
    private MeterUnitController deviceList = null;

    [SerializeField]
    private MeterUnitController settingPanel = null;

    [SerializeField]
    private MeterUnitController modList = null;

    [SerializeField]
    [Min(0)]
    private int startNum = 0;

    private int nowNum;
    SerialReceive serial;

    private List<MeterUnitController> useUnits = new List<MeterUnitController>();

    private void Start()
    {
        serial = this.gameObject.GetComponent<SerialReceive>();
        if (serial == null)
        {
            Debug.LogError("Not found  SerialReceive.");
            this.gameObject.SetActive(false);
            return;
        }
        if (units.Count == 0)
        {
            Debug.LogError("Not found  MeterUnitController.");
            this.gameObject.SetActive(false);
            return;
        }

        useUnits.Clear();
        foreach (var u in units)
        {
            useUnits.Add(u);
        }

        if (deviceList != null) {
            useUnits.Add(deviceList);
        }
        if (settingPanel != null)
        {
            useUnits.Add(settingPanel);
        }
        if (modList != null)
        {
            useUnits.Add(modList);
        }

        for (int ii = 0; ii < useUnits.Count; ii++)
        {
            GameObject _go = useUnits[ii].gameObject;
            _go.SetActive(false);
            _go.transform.localPosition = new Vector3(0, 0, 0);
        }

        nowNum = startNum;

        if (nowNum < useUnits.Count)
        {
            useUnits[nowNum].gameObject.SetActive(true);
        }
        else {
            nowNum = 0;
            useUnits[nowNum].gameObject.SetActive(true);
        }

        serial.controller = useUnits[nowNum];


    } /* Start */

    public void OnNextPanel()
    {
        if (useUnits.Count < 1)
        {
            return;
        }

        useUnits[nowNum].gameObject.SetActive(false);

        nowNum += 1;
        if (useUnits.Count <= nowNum)
        {
            nowNum = 0;
        }

        serial.controller = useUnits[nowNum];
        serial.controller.resetValue();
        serial.controller.gameObject.SetActive(true);
    } /* OnNextPanel */
    public void OnPrevPanel()
    {
        if (useUnits.Count < 1)
        {
            return;
        }

        useUnits[nowNum].gameObject.SetActive(false);
        if (nowNum <= 0)
        {
            nowNum = useUnits.Count - 1;
        } else {
            nowNum -= 1;
        }
        serial.controller = useUnits[nowNum];
        serial.controller.resetValue();
        serial.controller.gameObject.SetActive(true);
    } /* OnPrevPanel */

    public void OnStartDemoPlay()
    {
        useUnits[nowNum].StartDemoPlay();
    } /* OnStartDemoPlay */

    public void UpdateModPanels( ModInfo[] modInfos,GameObject _parent )
    {
        useUnits.Clear();
        foreach (var u in units)
        {
            useUnits.Add(u);
        }

        if (deviceList != null)
        {
            useUnits.Add(deviceList);
        }
        if (settingPanel != null)
        {
            useUnits.Add(settingPanel);
        }
        if (modList != null)
        {
            useUnits.Add(modList);
        }

        int count = _parent.transform.childCount;
        for (int ii = 0; ii < count; ii++)
        {
            var chld = _parent.transform.GetChild(0);
            chld.gameObject.SetActive(false);
            chld.gameObject.transform.SetParent(null);
            Destroy(chld.gameObject);
        }

        foreach (var modinfo in modInfos)
        {
            var myLoadedAssetBundle = AssetBundle.LoadFromFile(modinfo.filepath);
            if (myLoadedAssetBundle == null)
            {
                continue;
            }

            GameObject prefab = myLoadedAssetBundle.LoadAsset<GameObject>("MeterPanel");
            var panel = Instantiate(prefab);
            panel.transform.parent = _parent.transform;
            useUnits.Add(panel.GetComponent<MeterUnitController>());

            myLoadedAssetBundle.Unload(false);
        }

        for (int ii = 0; ii < useUnits.Count; ii++)
        {
            GameObject _go = useUnits[ii].gameObject;
            _go.SetActive(false);
            _go.transform.localPosition = new Vector3(0, 0, 0);
        }

        nowNum = startNum;

        if (nowNum < useUnits.Count)
        {
            useUnits[nowNum].gameObject.SetActive(true);
        }
        else
        {
            nowNum = 0;
            useUnits[nowNum].gameObject.SetActive(true);
        }

        serial.controller = useUnits[nowNum];


    } /* UpdateModPanels */

} /* class */

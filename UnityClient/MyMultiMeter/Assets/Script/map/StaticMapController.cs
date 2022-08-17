using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;// Unity内でネットワークを使用するときに記入する


[RequireComponent(typeof(RawImage))]
public class StaticMapController : MonoBehaviour
{
    private const string DUMMY_API_KEY = "XXGoogleAPIKeyXX";
    private string STATIC_MAP_URL = "https://maps.googleapis.com/maps/api/staticmap?key=" + DUMMY_API_KEY + "&zoom=15&size=640x640&scale=2&maptype=terrain&style=element:labels|visibility:off";// Google Maps Static API URL、${APIKey}を作成したapiキーに書き換える
    private float remainTime = 0.0f;
    private float updateTime = 5.0f;


    // Start is called before the first frame update
    void Start()
    {
        if (string.IsNullOrEmpty(SerialReceive.Instance.GoogleAPIKey) == true)
        {
            Debug.LogError("Request Google API Key. [ SerialReceive ]");
            this.gameObject.SetActive(false);
            return;
        }
        STATIC_MAP_URL = STATIC_MAP_URL.Replace(DUMMY_API_KEY, SerialReceive.Instance.GoogleAPIKey);
        remainTime = updateTime;
        StartCoroutine(getStaticMap());// getStaticMapを実行する
    } /* Start */

    // Update is called once per frame
    void Update()
    {
        remainTime -= Time.deltaTime;

        if (remainTime <= 0)
        {
            StartCoroutine(getStaticMap());// getStaticMapを実行する
            remainTime = updateTime;
        }
    } /* Update */

    IEnumerator getStaticMap()
    {
        var query = "";// queryを初期化する
        query += "&center=" + UnityWebRequest.UnEscapeURL(string.Format("{0},{1}", SerialReceive.Instance.latitude, SerialReceive.Instance.longitude));// centerで取得するミニマップの中央座標を設定
        query += "&markers=" + UnityWebRequest.UnEscapeURL(string.Format("{0},{1}", SerialReceive.Instance.latitude, SerialReceive.Instance.longitude));// markersで渡した座標(=現在位置)にマーカーを立てる
        var req = UnityWebRequestTexture.GetTexture(STATIC_MAP_URL + query);// リクエストの定義
        yield return req.SendWebRequest();// リクエスト実行

        if (req.error == null)// もし、リクエストがエラーでなければ、
        {
            try
            {
                RawImage img = GetComponent<RawImage>();
                Destroy(img.texture); //マップをなくす
                img.texture = ((DownloadHandlerTexture)req.downloadHandler).texture; //マップを貼りつける
            }catch (System.Exception _e){ 
            }
        }


    }/* getStaticMap */
}

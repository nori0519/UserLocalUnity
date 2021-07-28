using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UserLocalTest : MonoBehaviour
{
    //web cam and RawImage  initiald
    //RawImageでWebカメラで取得した画像を貼り付ける。
    public RawImage rawImage;
    WebCamTexture webCamTexture;

    //textureをtexture2Dにする関数
    public Texture2D ToTexture2D(Texture self)
    {
        var sw = self.width;
        var sh = self.height;
        var format = TextureFormat.RGBA32;
        var result = new Texture2D(sw, sh, format, false);
        var currentRT = RenderTexture.active;
        var rt = new RenderTexture(sw, sh, 32);
        Graphics.Blit(self, rt);
        RenderTexture.active = rt;
        var source = new Rect(0, 0, rt.width, rt.height);
        result.ReadPixels(source, 0, 0);
        result.Apply();
        RenderTexture.active = currentRT;
        return result;
    }

    [SerializeField]
    GameObject image;
    //Jsonで型を揃える
    [System.Serializable]
    public class MyJson
    {
        public string image_base64;
        public string api_key;
    }

    void Start()
    {
        //カメラ起動、RawImageに表示
        webCamTexture = new WebCamTexture();
        rawImage.texture = webCamTexture;
        webCamTexture.Play();

        //取得したカメラ映像のフローについて
        //WebCamTexture>Texture2D>JPG形式>Base64形式
        //WebCamTexture>Texture2D
        var img2d = ToTexture2D(webCamTexture);
        //Texture2D>JPG形式>Base64形式(Convert.ToBase64StringメソッドでBase64形式に変換)
        //Texture2D>JPG→var img = img2d.EncodeToJPG();
        string enc = System.Convert.ToBase64String(img2d.EncodeToJPG());

        MyJson myObject = new MyJson();
        myObject.api_key = "APIキー";
        myObject.image_base64 = enc;
        string myjson = JsonUtility.ToJson(myObject);

        StartCoroutine("PostData", myjson);
    }

    //Base64に変換した画像と開発者ごとのAPIキーをHson形式に格納し、コルーチンでPostData()に渡す。
    IEnumerator PostData(string myjson)
    {
        byte[] postData = System.Text.Encoding.UTF8.GetBytes(myjson);
        var request = new UnityWebRequest("https://face-ai.userlocal.jp/api/detect", "POST");
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(postData);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.Send();
        Debug.Log(request.error);
        Debug.Log(request.downloadHandler.text);

    }
}
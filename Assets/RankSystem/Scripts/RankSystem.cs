using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using DUCK.Crypto;
using static DUCK.Crypto.SimpleAESEncryption;
using System;

public class RankSystem : MonoBehaviour
{
    public static RankSystem instanse = null;


    [Header("Connection settings")]
    [SerializeField]
    private string GameKey;
    [SerializeField]
    private string SecurityKey;
    [SerializeField]
    private string IV;
    [SerializeField]
    private string ServerAdress = "127.0.0.1:3000";


    public delegate void Del(string message, bool result);
    public delegate void DelJson(JObject message, bool result);

    private string SessionKey = string.Empty;

    private string TestConnectionPostHeade = "/testConnection";
    private string RegisterPostHeade = "/registerClient";
    private string AuthPostHeade = "/authClient";
    private string SendPostHeade = "/sendData";
    private string GetPostHeade = "/getData";
    private string GetRatingHeade = "/getRating";

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        if (instanse == null)
        {
            instanse = this;
        }
    }
    
    public void Register(string login, string password, DelJson callback, JObject regAdditions = null)
    {
        JArray jArray = new JArray();
        JObject gamekey = new JObject();
        gamekey["gameKey"] = GameKey;
        JObject reginfo = new JObject();
        reginfo["login"] = login;
        reginfo["password"] = password;
        JObject cryptData = new JObject();
        cryptData["regInfo"] = Encrypt(reginfo.ToString());
        jArray.Add(gamekey);
        if (regAdditions != null)
        {
            cryptData["regAdditions"] = Encrypt(regAdditions.ToString());
        }
        jArray.Add(cryptData);
        if (GameKey != string.Empty && ServerAdress != string.Empty)
        {
            StartCoroutine(PostRequest(ServerAdress + RegisterPostHeade, jArray.ToString(), (string result, bool status) => {
                Debug.Log(result);
                JObject a = JObject.Parse(result);
                if (a["error"] != null)
                {
                    callback(a, false);
                }
                else
                {
                    SessionKey = a["result"].ToString();
                    callback(a, true);
                }


            }));
        }
        else
        {
            JObject errorObj = new JObject();
            errorObj["error"] = "ER_INVALID_FIELDS";
            callback(errorObj, false);
        }
    }

    public void Auth(string login, string password, DelJson callback)
    {

        if (GameKey != string.Empty && ServerAdress != string.Empty )
        {
            JArray jArray = new JArray();
            JObject gamekey = new JObject();
            gamekey["gameKey"] = GameKey;
            jArray.Add(gamekey);

            JObject authinfo = new JObject();
            authinfo["login"] = login;
            authinfo["password"] = password;
            JObject cryptData = new JObject();
            cryptData["authInfo"] = Encrypt(authinfo.ToString());
            jArray.Add(cryptData);

            StartCoroutine(PostRequest(ServerAdress + AuthPostHeade, jArray.ToString(), (string result, bool status) => {
                JObject a = JObject.Parse(result);
                if(a["error"] != null)
                {
                    callback(a, false);
                }
                else
                {
                    SessionKey = a["result"].ToString();
                    callback(a, true);
                }
  
            }));
        }
        else
        {
            JObject errorObj = new JObject();
            errorObj["error"] = "ER_INVALID_FIELDS";
            callback(errorObj, false);
        }
    }

    public void SendData(JObject dataToSend, DelJson callback)
    {
        if (dataToSend != null && dataToSend["gameKey"] == null && SessionKey != string.Empty)
        {
            JArray jArray = new JArray();
            JObject gamekey = new JObject();
            gamekey["gameKey"] = GameKey;
            JObject userData = new JObject();
            userData["sessionKey"] = SessionKey;
            userData["type"] = "params";  
            userData["userData"] = "'" + dataToSend.ToString() + "'";
            JObject cryptData = new JObject();
            cryptData["userData"] = Encrypt(userData.ToString());
            jArray.Add(gamekey);
            jArray.Add(cryptData);
            StartCoroutine(PostRequest(ServerAdress + SendPostHeade, jArray.ToString(), (string result, bool status) => {
                JObject a = JObject.Parse(result);
                if (a["error"] != null)
                {
                    callback(a, false);
                }
                else
                {
                    callback(a, true);
                }

            }));
        }
        else
        {
            JObject errorObj = new JObject();
            errorObj["error"] = "ER_INVALID_FIELDS";
            callback(errorObj, false);
        }
    }

    public void GetRating(uint count, DelJson callback)
    {
        if (SessionKey != string.Empty)
        {


            JArray jArray = new JArray();
            JObject gamekey = new JObject();
            gamekey["gameKey"] = GameKey;
            JObject userData = new JObject();
            userData["sessionKey"] = SessionKey;
            userData["count"] = count;
            JObject cryptData = new JObject();
            cryptData["userData"] = Encrypt(userData.ToString());
            jArray.Add(gamekey);
            jArray.Add(cryptData);
            StartCoroutine(PostRequest(ServerAdress + GetRatingHeade, jArray.ToString(), (string result, bool status) => {
                Debug.Log(result);
                JObject a = JObject.Parse(result);
                if (a["error"] != null)
                {
                    callback(a, false);
                }
                else
                {
                    callback(a, true);
                }

            }));
        }
        else
        {
            JObject errorObj = new JObject();
            errorObj["error"] = "ER_INVALID_FIELDS";
            callback(errorObj, false);
        }
    }

    public void SendRating(float rating, DelJson callback)
    {
        if (SessionKey != string.Empty)
        {
            JArray jArray = new JArray();
            JObject gamekey = new JObject();
            gamekey["gameKey"] = GameKey;
            JObject userData = new JObject();
            userData["sessionKey"] = SessionKey;
            userData["type"] = "rating";
            userData["userData"] = rating;
            JObject cryptData = new JObject();
            cryptData["userData"] = Encrypt(userData.ToString());
            jArray.Add(gamekey);
            jArray.Add(cryptData);

            jArray.Add(userData);
            StartCoroutine(PostRequest(ServerAdress + SendPostHeade, jArray.ToString(), (string result, bool status) => {
                JObject a = JObject.Parse(result);
                Debug.Log(a);
                if (a["error"] != null)
                {
                    callback(a, false);
                }
                else
                {
                    callback(a, true);
                }

            }));
        }
        else
        {
            JObject errorObj = new JObject();
            errorObj["error"] = "ER_INVALID_FIELDS";
            callback(errorObj, false);
        }
    }


    public void GettingData( DelJson callback)
    {
        if (SessionKey != string.Empty) { 
           JArray jArray = new JArray();
        JObject gamekey = new JObject();
        gamekey["gameKey"] = GameKey;
        JObject userData = new JObject();
        userData["sessionKey"] = SessionKey;
        JObject cryptData = new JObject();
        cryptData["sessionKey"] = Encrypt(userData.ToString());
        jArray.Add(gamekey);
        jArray.Add(cryptData);

        jArray.Add(userData);


        StartCoroutine(PostRequest(ServerAdress + GetPostHeade, jArray.ToString(), (string result, bool status) => {
                JObject a = JObject.Parse(result);
                if (a["error"] != null)
                {
                    callback(a, false);
                }
                else
                {
                    callback(a, true);
                }

            }));
        }
        else
        {
            JObject errorObj = new JObject();
            errorObj["error"] = "ER_INVALID_FIELDS";
            callback(errorObj, false);
        }
    }


    public void TestConnection()
    {
                

    JObject jObject = new JObject();
        if(GameKey != string.Empty && ServerAdress != string.Empty)
        {
            jObject["gameKey"] = GameKey ;
            StartCoroutine(PostRequest(ServerAdress + TestConnectionPostHeade, jObject.ToString(),(string result, bool state)=>{
                Debug.Log("Received: " + result);
            }));
        }
    }

    private IEnumerator PostRequest(string url, string json, Del callback)
    {
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            callback(uwr.error, false);
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            callback(uwr.downloadHandler.text,true);
        }
    }

    public string Encrypt(string message)
    {
        AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
        aes.BlockSize = 128;
        aes.KeySize = 256;
        aes.IV = UTF8Encoding.UTF8.GetBytes(IV);
        aes.Key = UTF8Encoding.UTF8.GetBytes(SecurityKey);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        byte[] data = Encoding.UTF8.GetBytes(message);
        using (ICryptoTransform encrypt = aes.CreateEncryptor())
        {
            byte[] dest = encrypt.TransformFinalBlock(data, 0, data.Length);
            return Convert.ToBase64String(dest);
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(RankSystem))]
    public class MyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            RankSystem myScript = (RankSystem)target;
            if (GUILayout.Button("Test Connection"))
            {

                myScript.TestConnection();
            }

        }
    }
#endif
}

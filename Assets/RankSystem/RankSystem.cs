using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public enum ContainerBase
{
    Constant,
    Parameters
}


public class RankSystem : MonoBehaviour
{
    public static RankSystem instanse = null; 

    [Header("Connection settings")]
    [SerializeField]
    private string GameKey;
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

    // Start is called before the first frame update
    void Start()
    {
        if (instanse == null)
        {
            instanse = this;
        }
    }
    
    public void Connect(Del callback)
    {
        JObject jObject = new JObject();
        if (GameKey != string.Empty && ServerAdress != string.Empty)
        {
            jObject["gameKey"] = GameKey;
            StartCoroutine(PostRequest(ServerAdress + TestConnectionPostHeade, jObject.ToString(), (string result, bool status) => {
                    callback(result, status);
            }));
        }

    }

    public void Register(string login, string password, DelJson callback, JObject regAdditions = null)
    {
        JArray jArray = new JArray();
        JObject jObject = new JObject();
        jObject["gameKey"] = GameKey;
        jObject["login"] = login;
        jObject["password"] = password;
        jArray.Add(jObject);

        if (regAdditions != null)
        {
            jArray.Add(regAdditions); 
        }

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
    }

    public void Auth(string login, string password, DelJson callback)
    {
        JObject jObject = new JObject();
        if (GameKey != string.Empty && ServerAdress != string.Empty )
        {
            jObject["gameKey"] = GameKey;
            jObject["login"] = login;
            jObject["password"] = password;
            StartCoroutine(PostRequest(ServerAdress + AuthPostHeade, jObject.ToString(), (string result, bool status) => {
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
    }

    public void SendData(JObject dataToSend, Del callback)
    {
        if (dataToSend != null && dataToSend["gameKey"] == null && SessionKey != string.Empty)
        {
            dataToSend["gameKey"] = GameKey;
            dataToSend["sessionKey"] = SessionKey;

            StartCoroutine(PostRequest(ServerAdress + SendPostHeade, dataToSend.ToString(), (string result, bool status) => {
                JObject a = JObject.Parse(result);
                if (a["error"] != null)
                {
                    callback(a["error"].ToString(), false);
                }
                else
                {
                    callback(a["result"].ToString(), true);
                }

            }));
        }
    }


    public void GettingData( DelJson callback, ContainerBase container = ContainerBase.Parameters)
    {
        JObject jObject = new JObject();
        if (SessionKey != string.Empty)
        {
            jObject["gameKey"] = GameKey;
            jObject["sessionKey"] = SessionKey;

            if(container == ContainerBase.Parameters)
            {
                jObject["base"] = "params";

            }
            else
            {
                jObject["base"] = "const_params";
            }

            StartCoroutine(PostRequest(ServerAdress + GetPostHeade, jObject.ToString(), (string result, bool status) => {
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
    }


    public void TestConnection()
    {
        JObject jObject = new JObject();
        if(GameKey != string.Empty && ServerAdress != string.Empty)
        {
            jObject["gameKey"] = GameKey;
            StartCoroutine(PostRequest(ServerAdress + TestConnectionPostHeade, jObject.ToString(),(string result, bool state)=>{
                Debug.Log("Received: " + result);
            }));
        }
    }

    IEnumerator PostRequest(string url, string json, Del callback)
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

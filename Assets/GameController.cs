using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class GameController : MonoBehaviour
{
    [Header("Register Fields")]
    [SerializeField]
    private InputField RegFIO;
    [SerializeField]
    private InputField RegLogin;
    [SerializeField]
    private InputField RegPassword;
    [SerializeField]
    private InputField RegRePassword;

    [Header("Auth Fields")]
    [SerializeField]
    private InputField AuthLogin;
    [SerializeField]
    private InputField AuthPassword;

    public GameObject MainMenuArray;
    public GameObject RegArray;
    public GameObject AuthArray;
    public GameObject LobbyArray;



    public InputField StringToSend;
    public InputField IntToSend;
    public Toggle BoolToSend;
    public Text ParamsText;
    

    // Start is called before the first frame update
    void Start()
    {
        //PlayerPrefs.DeleteAll();
        if(PlayerPrefs.HasKey("login") && PlayerPrefs.HasKey("password"))
        {

            string log = PlayerPrefs.GetString("login");
            string pass = PlayerPrefs.GetString("password");

            AuthProcess(log, pass);

        }
    }


    public void Auth()
    {

        if (AuthLogin.text != string.Empty && AuthPassword.text != string.Empty )
        {
            AuthProcess(AuthLogin.text, AuthPassword.text);
        }
    }


    public void AuthProcess(string login, string pass)
    {
        RankSystem.instanse.Auth(login, pass, (msg, status) =>
        {
            if (status)
            {
                PlayerPrefs.SetString("login", login);
                PlayerPrefs.SetString("password", pass);
                PlayerPrefs.Save();
                AuthArray.SetActive(false);
                MainMenuArray.SetActive(false);
                RegArray.SetActive(false);
                LobbyArray.SetActive(true);
                ParamsText.text = msg["useparams"].ToString();
            }
            Debug.Log(msg + " " + status);

        });
    }

    public void Register()
    {
        
        if (RegFIO.text != string.Empty && RegLogin.text != string.Empty && RegPassword.text != string.Empty && RegPassword.text == RegRePassword.text)
        {
            string login = RegLogin.text;
            string password = RegPassword.text;
            JObject jObject = new JObject();
            jObject["FIO"] = RegFIO.text;

            RankSystem.instanse.Register(RegLogin.text, RegPassword.text, (msg, status) =>
            {
                Debug.Log(msg + " " + status);
                if (status)
                {
                    AuthProcess(login, password);
                }
            }, jObject);
        }
    }


    public void OpenAuth()
    {
        MainMenuArray.SetActive(false);
        AuthArray.SetActive(true);
    }

    public void OpenReg()
    {
        MainMenuArray.SetActive(false);
        RegArray.SetActive(true);
    }

    public void LogOut()
    {

        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Sending()
    {
        JObject jObject = new JObject();
        if(StringToSend.text != string.Empty && IntToSend.text != string.Empty)
        {
            int temp = int.Parse(IntToSend.text);
            jObject["stringData"] = StringToSend.text;
            jObject["intData"] = temp;
            jObject["boolData"] = BoolToSend.isOn;
            RankSystem.instanse.SendData(jObject, (msg, status) =>
            {
                Debug.Log(msg + " " + status);
            });
        }
    }

    public void Getting()
    {

        RankSystem.instanse.GettingData((result, status) =>
        {
            if (status)
            {
                StringToSend.text = result["stringData"].Value<string>();
                IntToSend.text = result["intData"].Value<string>();
                BoolToSend.isOn = result["boolData"].Value<bool>();
            }
        });
    }
}

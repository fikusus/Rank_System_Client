using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameController : MonoBehaviour
{
    [Header("Register Fields")]
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

    // Update is called once per frame
    void Update()
    {

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
                LobbyArray.SetActive(true);
            }
            Debug.Log(msg + " " + status);

        });
    }

    public void Register()
    {
        
        if (RegLogin.text != string.Empty && RegPassword.text != string.Empty && RegPassword.text == RegRePassword.text)
        {
            RankSystem.instanse.Register(RegLogin.text, RegPassword.text, (msg, status) =>
            {
                                    Debug.Log(msg + " " + status);
                if (status)
                {

                    PlayerPrefs.SetString("login", RegLogin.text);
                    PlayerPrefs.SetString("password", RegPassword.text);
                    PlayerPrefs.Save();
                    RegArray.SetActive(false);
                    LobbyArray.SetActive(true);
                }
            });
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
}

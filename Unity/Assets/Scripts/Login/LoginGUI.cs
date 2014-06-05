using UnityEngine;
using System.Collections;

public class LoginGUI : MonoBehaviour {

    public GameObject ProgressBar;

    private Rect _LoginWindow = new Rect(Screen.width / 3.0f, Screen.height / 2.75f, Screen.width / 3.0f, Screen.height / 4.0f);

    private string _Login = string.Empty;
    private string _Pass = string.Empty;

    private bool _DrawRoundProgressBar = false;
    private float _ProgressBarRotation = 130.0f;

    private Grooveshark _GroovesharkAPI;

	void Start () {
        _GroovesharkAPI = GameObject.FindGameObjectWithTag("GroovesharkAPI").GetComponent<Grooveshark>();

        StartCoroutine(_GroovesharkAPI.StartSession());
	}
	
	void Update () {
	
	}

    void OnGUI()
    {
        _LoginWindow = new Rect(Screen.width / 3.0f, Screen.height / 2.75f, Screen.width / 3.0f, Screen.height / 4.0f);

        _LoginWindow = GUI.Window(0, _LoginWindow, DrawLoginWindow, "");
    }

    void DrawLoginWindow(int windowID)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Username :", GUILayout.Width(75.0f));
        _Login = GUILayout.TextField(_Login);
        GUILayout.Label("Password :", GUILayout.Width(75.0f));
        _Pass = GUILayout.PasswordField(_Pass, "*"[0]);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Login"))
        {
            StartCoroutine(_GroovesharkAPI.Authenticate(LoginCallback, _Login, _Pass));
            _DrawRoundProgressBar = true;
            ProgressBar.SetActive(true);
        }
        if (GUILayout.Button("Pass"))
        {
            Application.LoadLevel("0");
        }
        GUILayout.EndHorizontal();
        if (_DrawRoundProgressBar)
        {
            ProgressBar.transform.Rotate(Vector3.up, _ProgressBarRotation * Time.deltaTime);
        }
        else
            ProgressBar.SetActive(false);
    }

    private void LoginCallback(bool success, string nickname)
    {
        Debug.Log("Hello");
        ProgressBar.SetActive(false);

        if (success)
            Application.LoadLevel("0");
    }
}

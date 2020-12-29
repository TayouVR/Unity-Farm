using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    
    public Button connectButton;
    public Button exitButton;
    public InputField ipInput;
    public InputField portInput;
    
    public NetworkManager ntwkMgr;

    private void Start() {
        DontDestroyOnLoad(gameObject);
        
        connectButton.onClick.AddListener(StartGame);
        exitButton.onClick.AddListener(Quit);
    }

    /// <summary>
    /// Quit App method for button call
    /// </summary>
    public void Quit() {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void StartGame() {
        ntwkMgr.serverIp = ipInput.text;
        ntwkMgr.port = int.Parse(portInput.text);

        SceneManager.LoadScene(1, LoadSceneMode.Single);
        
        ntwkMgr.Connect();
    }
}

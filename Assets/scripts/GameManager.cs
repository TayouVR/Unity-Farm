using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public GameObject ui;
    
    private NetworkManager ntwkMgr;
    // Start is called before the first frame update
    void Start() {
        ui.transform.Find("");
        //ntwkMgr.networkAddress = "";
    }

    public void Connect() {
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

    // Update is called once per frame
    void Update()
    {
        
    }
}

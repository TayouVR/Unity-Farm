using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class SpawnCharacter : MonoBehaviour {
    public GameObject characterPrefab;
    
    // Start is called before the first frame update
    void Start() {
        var character = Instantiate(characterPrefab, transform);
        //character.GetComponent<NetworkTransformChild>().target = transform;
    }
}

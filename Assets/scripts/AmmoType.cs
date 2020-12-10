using UnityEngine;

[System.Serializable]
public abstract class AmmoType : MonoBehaviour {
	public string name;
	public int damage;
	public Sprite icon;
	public GameObject droppedAmmo;
}
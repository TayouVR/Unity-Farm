using UnityEngine;

public class Interactable : MonoBehaviour {

	public Material selectedMaterial;
	private Material originalMaterial;

	private void Start() {
		originalMaterial = GetComponent<MeshRenderer>().material;
	}
	

	public void Select() {
		Debug.Log("Object " + name + " selected");
		GetComponent<MeshRenderer>().material = selectedMaterial;
	}

	public void Deselect() {
		Debug.Log("Object " + name + " deselected");
		GetComponent<MeshRenderer>().material = originalMaterial;
	}
}
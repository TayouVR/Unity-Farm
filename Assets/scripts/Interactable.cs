using UnityEngine;

public class Interactable : MonoBehaviour {

	public Material selectedMaterial;
	private Material originalMaterial;

	private void Start() {
		originalMaterial = GetComponent<MeshRenderer>().material;
	}
	

	public void Select() {
		GetComponent<MeshRenderer>().material = selectedMaterial;
	}

	public void Deselect() {
		GetComponent<MeshRenderer>().material = originalMaterial;
	}
}
using UnityEngine;

public class Interactable : MonoBehaviour {

	public Material selectedMaterial;
	private Material originalMaterial;

	private Renderer meshRenderer;

	private void Start() {
		originalMaterial = GetComponentInChildren<Renderer>().material;
		meshRenderer = GetComponentInChildren<Renderer>();
	}
	

	public void Select() {
		Debug.Log("Object " + name + " selected");
		meshRenderer.material = selectedMaterial;
	}

	public void Deselect() {
		Debug.Log("Object " + name + " deselected");
		meshRenderer.material = originalMaterial;
	}
}
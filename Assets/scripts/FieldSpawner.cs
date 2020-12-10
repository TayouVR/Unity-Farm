using System;
using Unity.Properties.UI;
using UnityEngine;

public class FieldSpawner : MonoBehaviour {
	public GameObject plantPrefab;

	[MinMax(0, 100)] public Vector2 distance;
	[MinMax(0, 100)] public Vector2 count;

	private void Start() {
		float x = 0;
		float y = 0;
		for (int i = 0; i < count.x; i++) {
			for (int e = 0; e < count.y; e++) {
				Instantiate(plantPrefab, transform.position + new Vector3(x, 0, y), transform.rotation, transform);
				y += distance.y;
			}

			x += distance.x;
			y = 0;
		}
	}
}
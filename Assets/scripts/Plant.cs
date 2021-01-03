using System;
using UnityEngine;
using Random = System.Random;

public class Plant : Interactable {
	public int reseedTime;

	public Vector2Int minMaxAmmoYield;
	
	public string plantName;
	public float growthStage;
	public int reseedWaitTime;
	[Tooltip("Growth Time in Seconds")]
	public int growthTime = 100;

	public AmmoType ammoObject;

	private float timerTimestamp;

	private void Update() {
		if (growthStage > 0) {
			transform.localScale = new Vector3(growthStage / growthTime, growthStage / growthTime, growthStage / growthTime);
		} else {
			transform.localScale = new Vector3(0, 0, 0);
		}
		if (growthStage >= 0) {
			if (growthStage > growthTime) {
				growthStage = growthTime;
			} else if (growthStage < growthTime) {
				growthStage = Time.time - timerTimestamp;
			}
		} else {

			if (reseedWaitTime > reseedTime) {
				reseedWaitTime = reseedTime;
			} else if (reseedWaitTime < reseedTime) {
				reseedWaitTime = (int) Math.Floor(Time.time - timerTimestamp);
			}
		}
	}

	public int Harvest() {
		timerTimestamp = Time.time;
		growthStage = 0;
		
		return new Random().Next(minMaxAmmoYield.x,minMaxAmmoYield.y);
	}
}
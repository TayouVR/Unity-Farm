using System;
using UnityEngine;

public class Plant : MonoBehaviour {
	public int reseedTime;
	
	public string plantName;
	public int growthStage;
	public int reseedWaitTime;

	private float timerTimestamp;

	private void Update() {
		if (growthStage >= 1) {
			if (growthStage == 1) {
				timerTimestamp = Time.time;
			}

			if (growthStage > 100) {
				growthStage = 100;
			} else if (growthStage < 100) {
				growthStage = (int) Math.Floor(Time.time - timerTimestamp);
			}
		} else {

			if (reseedWaitTime > reseedTime) {
				reseedWaitTime = reseedTime;
			} else if (reseedWaitTime < reseedTime) {
				reseedWaitTime = (int) Math.Floor(Time.time - timerTimestamp);
			}
		}
	}

	private void Harvest() {
		growthStage = 0;
		timerTimestamp = Time.time;
	}
}
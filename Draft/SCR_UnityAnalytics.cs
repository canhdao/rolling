using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class SCR_UnityAnalytics : MonoBehaviour {
	private static System.DateTime challengeStartTime;
	private static System.DateTime endlessStartTime;
	private static System.DateTime multiplayerStartTime;
	
	public static void Challenge_Start () {
		Analytics.CustomEvent("Challenge_Start", null);
	}

	public static void Endless_Start () {
		endlessStartTime = System.DateTime.Now;
		Analytics.CustomEvent("Endless_Start", null);
	}
	
	public static void Multiplayer_Start () {
		multiplayerStartTime = System.DateTime.Now;
		Analytics.CustomEvent("Multiplayer_Start", null);
	}
	
	public static void Challenge_StartLevel (int level) {
		challengeStartTime = System.DateTime.Now;
		Analytics.CustomEvent("Challenge_StartLevel", new Dictionary<string, object>
		{
			{ "Level", level }
		});
	}
	
	public static void Challenge_FailLevel (int level) {
		System.DateTime finishTime = System.DateTime.Now;
		float deltaSeconds = (float)(finishTime - challengeStartTime).TotalSeconds;
		Analytics.CustomEvent("Challenge_FailLevel", new Dictionary<string, object>
		{
			{ "Level", level },
			{ "TimeInSeconds", deltaSeconds }
		});
	}
	
	public static void Challenge_PassLevel (int level) {
		System.DateTime finishTime = System.DateTime.Now;
		float deltaSeconds = (float)(finishTime - challengeStartTime).TotalSeconds;
		Analytics.CustomEvent("Challenge_PassLevel", new Dictionary<string, object>
		{
			{ "Level", level },
			{ "TimeInSeconds", deltaSeconds }
		});
	}
	
	public static void Endless_Finish (int score) {
		System.DateTime finishTime = System.DateTime.Now;
		float deltaSeconds = (float)(finishTime - endlessStartTime).TotalSeconds;
		Analytics.CustomEvent("Endless_Finish", new Dictionary<string, object>
		{
			{ "Score", score },
			{ "TimeInSeconds", deltaSeconds }
		});
	}
	
	public static void Multiplayer_Finish () {
		System.DateTime finishTime = System.DateTime.Now;
		float deltaSeconds = (float)(finishTime - multiplayerStartTime).TotalSeconds;
		Analytics.CustomEvent("Multiplayer_Finish", new Dictionary<string, object>
		{
			{ "TimeInSeconds", deltaSeconds }
		});
	}
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

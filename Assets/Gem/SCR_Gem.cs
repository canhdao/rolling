using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Gem : MonoBehaviour {
	private Transform pivot;
	//private Transform mesh;
	
	void Start() {
		pivot = transform.Find("Pivot");
		//mesh = pivot.Find("Mesh");
		
		pivot.localEulerAngles = new Vector3(0, Random.Range(0f, 360f), 0);
	}
	
	void Update() {
		const float speed = 10;
		pivot.localEulerAngles = new Vector3(0, pivot.localEulerAngles.y + speed * Time.unscaledDeltaTime, 0);
	}
	/*
	public void FadeIn() {
		gameObject.SetActive(true);
		
		Material m = mesh.GetComponent<Renderer>().material;
		m.color = new Color(m.color.r, m.color.g, m.color.b, 0);
		iTween.FadeTo(gameObject, iTween.Hash("alpha", 1, "time", 0.5f, "easetype", "linear"));
	}
	
	public void FadeOut() {
		iTween.FadeTo(gameObject, iTween.Hash("alpha", 0, "time", 0.5f, "easetype", "linear", "oncomplete", "CompleteFadeOut"));
	}
	
	private void CompleteFadeOut() {
		gameObject.SetActive(false);
	}
	*/
}

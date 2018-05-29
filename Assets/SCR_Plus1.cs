using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SCR_Plus1 : MonoBehaviour {
	private Text text;
	private Color c;
	private RectTransform rt;
	
	private void Start() {
		text = GetComponent<Text>();
		rt = GetComponent<RectTransform>();
		c = text.color;
	}
	
	public void Fly() {
		iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", 300, "time", 1.5f, "easetype", "easeInOutSine", "onupdate", "UpdateY"));
		iTween.ValueTo(gameObject, iTween.Hash("from", 1, "to", 0, "time", 0.5f, "delay", 1, "onupdate", "UpdateAlpha", "oncomplete", "Destroy"));
	}
	
	private void UpdateAlpha(float alpha) {
		text.color = new Color(c.r, c.g, c.b, alpha);
	}
	
	private void Destroy() {
		Destroy(gameObject);
	}
	
	private void UpdateY(float y) {
		rt.anchoredPosition = new Vector2(0, y);
	}
}

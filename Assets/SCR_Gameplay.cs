using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public enum State {
	READY,
	PLAY,
	FINISH
}

public class SCR_Gameplay : MonoBehaviour {
	public Transform PFB_BOX;
	public Transform PFB_CORNER;
	public Transform PFB_CYLINDER;
	
	public Transform lastBox;
	public SCR_Sphere scrSphere;
	public Text txtScore;
	public Text txtBest;
	public Text txtTapToPlay;
	public GameObject arrowLeft;
	public GameObject arrowRight;
	public GameObject gemImage;
	public GameObject gemNumber;
	public GameObject shop;
	public GameObject leaderboard;
	
	public static int numberGems = 0;
	private static int lastNumberGems = 0;
	
	public State state;
	
	public Material matBox;
	public Material matCylinder;
	
	public Material[] MAT_BOXES;
	
	private WallDirection savedWallDirection = WallDirection.NONE;
	
	private TurnDirection nextTurnDirection;
	
	private int count = 0;
	private int nextCount = 0;
	
	private float rateShort;
	private float rateMedium;
	
	private int key = 0;
	
	private Transform[] boxesToDestroy = new Transform[5];
	
	private static int score = 0;
	private static int best = 0;
	private static int lastBest = -1;
	
	private Color currentColor;
	private Color nextColor;
	
	private int colorIndex;
	
	private SCR_Camera scrCamera;
	
	private const float speedRange = 2f;
	private const float speedMin = 1.4f;
	private const float speedAccelRev = 70f;
	
	// Use this for initialization
	void Start () {
		Application.targetFrameRate = 60;
		
		for (int i = 0; i < boxesToDestroy.Length; i++) {
			boxesToDestroy[i] = null;
		}
		
		rateShort = 1.0f / 3;
		rateMedium = 1.0f / 3;
		
		nextTurnDirection = TurnDirection.LEFT;
		
		for (int i = 0; i < 3; i++) {
			GenerateNextBox(false);
		}
		
		txtScore.text = score.ToString();
		
		best = PlayerPrefs.GetInt("best", 0);
		if (lastBest == -1) lastBest = best;
		
		if (best == 0) {
			txtBest.gameObject.SetActive(false);
		}
		else {
			if (best != lastBest) {
				iTween.ValueTo(gameObject, iTween.Hash("from", lastBest, "to", best, "time", 0.5f, "onupdate", "UpdateBest"));
				
				float bigScale = 1.5f;
				float time = 0.5f;
				
				iTween.ScaleTo(txtBest.gameObject, iTween.Hash("scale", new Vector3(bigScale, bigScale, 1), "easetype", "easeInOutSine", "time", time));
				iTween.ScaleTo(txtBest.gameObject, iTween.Hash("scale", new Vector3(1, 1, 1), "easetype", "easeInOutSine", "time", time, "delay", time));
			}
			else {
				txtBest.text = "BEST " + best.ToString();
			}
			
			txtBest.gameObject.SetActive(true);
		}
		
		lastBest = best;
		/*
		currentColor = new Color(84 / 255.0f, 140 / 255.0f, 215 / 255.0f, 1);
		
		// color correction
		Vector3 hcy = SCR_ColorConverter.RGBToHCY(new Vector3(currentColor.r, currentColor.g, currentColor.b));
		hcy.y = 1;
		hcy.z += 0.075f;
		Vector3 rgb = SCR_ColorConverter.HCYToRGB(hcy);
		currentColor = new Color(rgb.x, rgb.y, rgb.z, 1);
		*/
		colorIndex = Random.Range(0, MAT_BOXES.Length);
		currentColor = MAT_BOXES[colorIndex].color;
		
		GenerateNextColor();
		
		matBox.color = currentColor;
		matCylinder.color = currentColor;
		
		iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", 1, "time", 1f, "easetype", "easeInOutSine", "onupdate", "UpdateTapToPlay", "looptype", "pingPong", "ignoretimescale", true));
		
		txtTapToPlay.gameObject.SetActive(true);
		
		scrCamera = Camera.main.GetComponent<SCR_Camera>();
		
		if (numberGems > lastNumberGems) {
			iTween.ValueTo(gameObject, iTween.Hash("from", lastNumberGems, "to", numberGems, "time", 0.5f, "onupdate", "UpdateGems", "ignoretimescale", true));
			
			float bigScale = 1.5f;
			float time = 0.5f;
			
			iTween.ScaleTo(gemNumber, iTween.Hash("scale", new Vector3(bigScale, bigScale, 1), "easetype", "easeInOutSine", "time", time, "ignoretimescale", true));
			iTween.ScaleTo(gemNumber, iTween.Hash("scale", new Vector3(1, 1, 1), "easetype", "easeInOutSine", "time", time, "delay", time, "ignoretimescale", true));
			iTween.ScaleTo(gemImage, iTween.Hash("scale", new Vector3(bigScale, bigScale, 1), "easetype", "easeInOutSine", "time", time, "ignoretimescale", true));
			iTween.ScaleTo(gemImage, iTween.Hash("scale", new Vector3(1, 1, 1), "easetype", "easeInOutSine", "time", time, "delay", time, "ignoretimescale", true));
		}
		else {
			gemNumber.GetComponent<Text>().text = numberGems.ToString();
		}
		
		lastNumberGems = numberGems;
		
		Time.timeScale = speedMin;
		
		state = State.READY;
	}
	
	void Update () {
		if (Input.touchCount > 0) {
			if (Input.GetTouch(0).phase == TouchPhase.Began) {
				if (state == State.READY) {
					if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) {
						Play();
					}
				}
				else if (state == State.PLAY) {
					scrSphere.Jump();
				}
			}
		}
		else if (Input.GetMouseButtonDown (0)) {
			if (state == State.READY) {
				if (!EventSystem.current.IsPointerOverGameObject()) {
					Play();
				}
			}
			else if (state == State.PLAY) {
				scrSphere.Jump();
			}
		}		
	}
	
	private void Play() {
		score = 0;
		txtScore.text = "0";
		
		txtTapToPlay.gameObject.SetActive(false);
		iTween.ValueTo(gameObject, iTween.Hash("from", 1, "to", 0, "time", 0.3f, "onupdate", "UpdateArrows", "oncomplete", "DisableArrows", "ignoretimescale", true));
		iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", 200, "time", 0.3f, "easetype", "easeInOutSine", "onupdate", "UpdateButtons", "oncomplete", "DisableButtons", "ignoretimescale", true));

		state = State.PLAY;
		scrCamera.ZoomOut();
	}
	
	public void GameOver() {
		state = State.FINISH;
		scrCamera.ZoomIn();
		Invoke("Replay", 3);
	}
	
	private void Replay() {
		SceneManager.LoadScene("Jump");
	}
	
	public void GenerateNextBox(bool fade = true) {
		count++;
		
		Transform next = null;
		
		float width = 1;
		float length = 1;
		
		if (count < nextCount) {
			next = Instantiate(PFB_BOX);
			
			if (lastBox.tag == "Box") {
				width = 1;
				
				float r = Random.Range(0f, 1f);
				
				if (r < rateShort) {
					length = 1.5f;
					next.GetComponent<SCR_Box>().SetLength(BoxLength.SHORT);
				}
				else if (r < rateShort + rateMedium) {
					length = 2.5f;
					next.GetComponent<SCR_Box>().SetLength(BoxLength.MEDIUM);
				}
				else {
					length = 3.5f;
					next.GetComponent<SCR_Box>().SetLength(BoxLength.LONG);
				}
				
				key++;
				
				float shortRange = 0.666f;
				float shortMin = 0.333f;
				float shortAccelRev = 10.0f;
				rateShort = (key * shortRange) / (key + shortAccelRev) + shortMin;
				/*
				float mediumRange = 0.166f;
				float mediumMin = 0.333f;
				float mediumAccelRev = 50.0f;
				rateMedium = (key * mediumRange) / (key + mediumAccelRev) + mediumMin;
				*/
				
				Time.timeScale = (key * speedRange) / (key + speedAccelRev) + speedMin;
			}
			
			if (lastBox.tag == "Corner") {
				width = 1;
				length = 2.2f;
				next.GetComponent<SCR_Box>().SetLength(BoxLength.MEDIUM);
			}
		}
		else {
			next = Instantiate(PFB_CORNER);
			/*
			float r = Random.Range(0f, 1f);
			if (r < 0.5f) {
				next.GetComponent<SCR_Corner>().turnDirection = TurnDirection.LEFT;
			}
			else {
				next.GetComponent<SCR_Corner>().turnDirection = TurnDirection.RIGHT;
			}
			*/
			next.GetComponent<SCR_Corner>().turnDirection = nextTurnDirection;
			if (nextTurnDirection == TurnDirection.LEFT) {
				nextTurnDirection = TurnDirection.RIGHT;
			}
			else {
				nextTurnDirection = TurnDirection.LEFT;
			}
			
			width = 1;
			length = 2.2f;
			next.GetComponent<SCR_Box>().SetLength(BoxLength.MEDIUM);
			
			count = 0;
			
			float r = Random.Range(0f, 1f);
			if (r < 0.3f) {
				nextCount = 2;
			}
			else {
				nextCount = Random.Range(3, 7);
			}
		}
		
		float x = lastBox.position.x;
		float y = lastBox.position.y;
		float z = lastBox.position.z;
		
		SCR_Box scrBoxLast = lastBox.GetComponent<SCR_Box>();
		SCR_Box scrBoxNext = next.GetComponent<SCR_Box>();
		
		if (lastBox.tag == "Box") {
			WallDirection wallDirection = scrBoxLast.wallDirection;
			
			if (wallDirection == WallDirection.NONE) {
				wallDirection = savedWallDirection;
			}
			
			if (wallDirection == WallDirection.NEGATIVE_X) {
				next.localScale = new Vector3(length, PFB_BOX.localScale.y, width);
				
				x += lastBox.localScale.x * 0.5f + next.localScale.x * 0.5f;
				y += PFB_BOX.localScale.y;
			}
			
			if (wallDirection == WallDirection.POSITIVE_X) {
				next.localScale = new Vector3(length, PFB_BOX.localScale.y, width);
				
				x -= lastBox.localScale.x * 0.5f + next.localScale.x * 0.5f;
				y += PFB_BOX.localScale.y;
			}
			
			if (wallDirection == WallDirection.NEGATIVE_Z) {
				next.localScale = new Vector3(width, PFB_BOX.localScale.y, length);
				
				y += PFB_BOX.localScale.y;
				z += lastBox.localScale.z * 0.5f + next.localScale.z * 0.5f;
			}
			
			if (wallDirection == WallDirection.POSITIVE_Z) {
				next.localScale = new Vector3(width, PFB_BOX.localScale.y, length);
				
				y += PFB_BOX.localScale.y;
				z -= lastBox.localScale.z * 0.5f + next.localScale.z * 0.5f;
			}
			
			scrBoxNext.wallDirection = wallDirection;
		}
		
		if (lastBox.tag == "Corner") {
			SCR_Corner scrCorner = lastBox.GetComponent<SCR_Corner>();
			
			scrBoxNext.wallDirection = WallDirection.NONE;
			
			Transform cylinder = Instantiate(PFB_CYLINDER);
			scrBoxNext.cylinder = cylinder;
			
			if (scrBoxLast.wallDirection == WallDirection.NEGATIVE_X) {
				next.localScale = new Vector3(width, PFB_BOX.localScale.y, length);
				
				x += lastBox.localScale.x * 0.5f;
				
				if (scrCorner.turnDirection == TurnDirection.LEFT) {
					z += next.localScale.z * 0.5f;
					savedWallDirection = WallDirection.NEGATIVE_Z;
				}
				
				if (scrCorner.turnDirection == TurnDirection.RIGHT) {
					z -= next.localScale.z * 0.5f;
					savedWallDirection = WallDirection.POSITIVE_Z;
				}
			
				cylinder.position = new Vector3(lastBox.position.x + lastBox.localScale.x * 0.5f, lastBox.position.y, lastBox.position.z);
			}
			
			if (scrBoxLast.wallDirection == WallDirection.POSITIVE_X) {
				next.localScale = new Vector3(width, PFB_BOX.localScale.y, length);
				
				x -= lastBox.localScale.x * 0.5f;
				
				if (scrCorner.turnDirection == TurnDirection.LEFT) {
					z -= next.localScale.z * 0.5f;
					savedWallDirection = WallDirection.POSITIVE_Z;
				}
				
				if (scrCorner.turnDirection == TurnDirection.RIGHT) {
					z += next.localScale.z * 0.5f;
					savedWallDirection = WallDirection.NEGATIVE_Z;
				}
			
				cylinder.position = new Vector3(lastBox.position.x - lastBox.localScale.x * 0.5f, lastBox.position.y, lastBox.position.z);
			}
			
			if (scrBoxLast.wallDirection == WallDirection.NEGATIVE_Z) {
				next.localScale = new Vector3(length, PFB_BOX.localScale.y, width);
				
				z += lastBox.localScale.z * 0.5f;
				
				if (scrCorner.turnDirection == TurnDirection.LEFT) {
					x -= next.localScale.x * 0.5f;
					savedWallDirection = WallDirection.POSITIVE_X;
				}
				
				if (scrCorner.turnDirection == TurnDirection.RIGHT) {
					x += next.localScale.x * 0.5f;
					savedWallDirection = WallDirection.NEGATIVE_X;
				}
			
				cylinder.position = new Vector3(lastBox.position.x, lastBox.position.y, lastBox.position.z + lastBox.localScale.z * 0.5f);
			}
			
			if (scrBoxLast.wallDirection == WallDirection.POSITIVE_Z) {
				next.localScale = new Vector3(length, PFB_BOX.localScale.y, width);
				
				z -= lastBox.localScale.z * 0.5f;
				
				if (scrCorner.turnDirection == TurnDirection.LEFT) {
					x += next.localScale.x * 0.5f;
					savedWallDirection = WallDirection.NEGATIVE_X;
				}
				
				if (scrCorner.turnDirection == TurnDirection.RIGHT) {
					x -= next.localScale.x * 0.5f;
					savedWallDirection = WallDirection.POSITIVE_X;
				}
			
				cylinder.position = new Vector3(lastBox.position.x, lastBox.position.y, lastBox.position.z - lastBox.localScale.z * 0.5f);
			}
		}
		
		next.position = new Vector3(x, y, z);
		
		//if (fade) {
			//iTween.FadeFrom(next.gameObject, iTween.Hash("alpha", 0, "time", 0.5f));
		//}
		
		scrBoxNext.GenerateGem();
		
		lastBox = next;
	}
	
	public void DestroyLastBox(Transform box) {
		if (boxesToDestroy[0] != null) {
			boxesToDestroy[0].GetComponent<SCR_Box>().Destroy();
		}
		
		for (int i = 0; i < boxesToDestroy.Length - 1; i++) {
			boxesToDestroy[i] = boxesToDestroy[i + 1];
		}
		
		boxesToDestroy[boxesToDestroy.Length - 1] = box;
		
		if (box != null) {
			if (box.gameObject.tag != "Corner") {
				iTween.MoveTo(box.gameObject, iTween.Hash("y", box.position.y - 5, "time", 1, "easetype", "easeInSine"));
				
				Transform lastBox = boxesToDestroy[boxesToDestroy.Length - 2];
				if (lastBox != null) {
					if (lastBox.gameObject.tag == "Corner") {
						iTween.MoveTo(lastBox.gameObject, iTween.Hash("y", box.position.y - 5, "time", 1, "easetype", "easeInSine"));
					}
				}
			}
		}
	}
	
	public void IncreaseScore() {
		score++;
		txtScore.text = score.ToString();
		
		if (score > best) {
			best = score;
			PlayerPrefs.SetInt("best", best);
		}
		
		if (score % 10 == 0) {
			//matBox.color = nextColor;
			//matCylinder.color = nextColor;
			
			iTween.ValueTo(gameObject, iTween.Hash("from", currentColor.r, "to", nextColor.r, "time", 0.25f, "easetype", "easeInOutSine", "onupdate", "UpdateRed", "ignoretimescale", true));
			iTween.ValueTo(gameObject, iTween.Hash("from", currentColor.g, "to", nextColor.g, "time", 0.25f, "easetype", "easeInOutSine", "onupdate", "UpdateGreen", "ignoretimescale", true));
			iTween.ValueTo(gameObject, iTween.Hash("from", currentColor.b, "to", nextColor.b, "time", 0.25f, "easetype", "easeInOutSine", "onupdate", "UpdateBlue", "ignoretimescale", true));
			
			currentColor = nextColor;
			GenerateNextColor();
		}
	}
	
	private void UpdateRed(float r) {
		matBox.color = new Color(r, matBox.color.g, matBox.color.b, 1);
		matCylinder.color = new Color(r, matCylinder.color.g, matCylinder.color.b, 1);
	}
	
	private void UpdateGreen(float g) {
		matBox.color = new Color(matBox.color.r, g, matBox.color.b, 1);
		matCylinder.color = new Color(matCylinder.color.r, g, matCylinder.color.b, 1);
	}
	
	private void UpdateBlue(float b) {
		matBox.color = new Color(matBox.color.r, matBox.color.g, b, 1);
		matCylinder.color = new Color(matCylinder.color.r, matCylinder.color.g, b, 1);
	}
	
	private void GenerateNextColor() {
		/*
		Vector3 hcy = SCR_ColorConverter.RGBToHCY(new Vector3(currentColor.r, currentColor.g, currentColor.b));
		
		do {
			hcy.x += Random.Range(0.3f, 0.7f);
			if (hcy.x > 1) hcy.x -= 1;
		} while (hcy.x > 0.2f && hcy.x < 0.7f);
		
		Vector3 rgb = SCR_ColorConverter.HCYToRGB(hcy);
		nextColor = new Color(rgb.x, rgb.y, rgb.z, 1);
		*/
		
		colorIndex++;
		if (colorIndex >= MAT_BOXES.Length) colorIndex = 0;
		nextColor = MAT_BOXES[colorIndex].color;
	}
	
	private void UpdateTapToPlay(float alpha) {
		txtTapToPlay.color = new Color(txtTapToPlay.color.r, txtTapToPlay.color.g, txtTapToPlay.color.b, alpha);
	}
	
	private void UpdateArrows(float alpha) {
		Color c = arrowLeft.GetComponent<Image>().color;
		c = new Color(c.r, c.g, c.b, alpha);
		
		arrowLeft.GetComponent<Image>().color = c;
		arrowRight.GetComponent<Image>().color = c;
		
		gemImage.GetComponent<Image>().color = c;
		
		c = gemNumber.GetComponent<Text>().color;
		c = new Color(c.r, c.g, c.b, alpha);
		gemNumber.GetComponent<Text>().color = c;
		
		txtBest.color = c;
	}
	
	private void DisableArrows() {
		arrowLeft.SetActive(false);
		arrowRight.SetActive(false);
		
		gemImage.SetActive(false);
		gemNumber.SetActive(false);
		
		txtBest.gameObject.SetActive(false);
	}
	
	private void UpdateGems(int number) {
		gemNumber.GetComponent<Text>().text = number.ToString();
	}
	
	private void UpdateBest(int number) {
		txtBest.text = "BEST " + number.ToString();
	}
	
	private void UpdateButtons(float offset) {
		shop.GetComponent<RectTransform>().anchoredPosition = new Vector2(-offset, 0);
		leaderboard.GetComponent<RectTransform>().anchoredPosition = new Vector2(offset, 0);
	}
	
	private void DisableButtons() {
		shop.SetActive(false);
		leaderboard.SetActive(false);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Camera : MonoBehaviour {
	public Transform sphere;
	
	private SCR_Gameplay scrGameplay;
	private SCR_Sphere scrSphere;
	
	private const float MOVE_UP_SPEED = 1.0f;
	private const float ROTATE_TIME = 0.5f;

	private float radius;
	
	private float startX;
	private float startY;
	private float startZ;
	
	private float targetY;
	
	private float targetRotation;
	private float currentRotation;
	
	private WallDirection wallDirection;
	private TurnDirection turnDirection;
	
	private Transform lastHit;
	
	// overlapsed raycast
	private Transform lastCorner;
	private Transform lastBox;
	
	// Use this for initialization
	void Start () {
		startX = transform.position.x;
		startY = transform.position.y;
		startZ = transform.position.z;
		
		radius = Mathf.Abs(startX);
		
		targetY = startY;
		currentRotation = transform.localEulerAngles.y;
		targetRotation = currentRotation;
		
		scrGameplay = GetComponent<SCR_Gameplay>();
		scrSphere = sphere.GetComponent<SCR_Sphere>();
		
		lastHit = null;
		
		//ZoomIn();
		Camera.main.orthographicSize = 1;
		transform.position = new Vector3(transform.position.x, startY - 1.25f, transform.position.z);
	}
	
	public void Update() {
		if (scrGameplay.state == State.PLAY) {
			transform.position = new Vector3(startX + sphere.position.x, transform.position.y, startZ + sphere.position.z);
			
			RaycastHit hitInfo;
			if (Physics.Raycast(sphere.position, new Vector3(0, -1, 0), out hitInfo, 20, 1 << SCR_Box.LAYER)) {
				targetY = startY + hitInfo.transform.position.y;
				scrSphere.RestrictPosition(hitInfo.transform);
				
				if (hitInfo.transform != lastHit) {
					if ((hitInfo.transform.gameObject.tag == "Corner" && hitInfo.transform != lastCorner) ||
						(hitInfo.transform.gameObject.tag == "Box" && hitInfo.transform != lastBox)) {
						scrGameplay.GenerateNextBox();
						scrGameplay.DestroyLastBox(lastHit);
					}
				}
				
				lastHit = hitInfo.transform;
				
				if (hitInfo.transform.gameObject.tag == "Corner") {
					lastCorner = hitInfo.transform;
				}
				
				if (hitInfo.transform.gameObject.tag == "Box") {
					lastBox = hitInfo.transform;
				}
			}
			else {
				targetY = transform.position.y;
			}
			
			if (targetY > transform.position.y) {
				float newY = transform.position.y + MOVE_UP_SPEED * Time.deltaTime * (targetY - transform.position.y);
				transform.position = new Vector3(transform.position.x, newY, transform.position.z);
			}
			
			transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, currentRotation, transform.localEulerAngles.z);
		}
	}
	
	public void ZoomIn() {
		iTween.ValueTo(gameObject, iTween.Hash("from", Camera.main.orthographicSize, "to", 1, "time", 1f, "onupdate", "UpdateSize", "easetype", "easeInOutSine"));
		
		float fromY = Camera.main.transform.position.y;
		float toY = fromY - 1.25f;
		
		iTween.ValueTo(gameObject, iTween.Hash("from", fromY, "to", toY, "time", 1f, "onupdate", "UpdateY", "easetype", "easeInOutSine"));
	}
	
	public void ZoomOut() {
		iTween.ValueTo(gameObject, iTween.Hash("from", Camera.main.orthographicSize, "to", 4, "time", 1f, "onupdate", "UpdateSize", "easetype", "easeInOutSine"));
		
		float fromY = Camera.main.transform.position.y;
		float toY = fromY + 1.25f;
		
		iTween.ValueTo(gameObject, iTween.Hash("from", fromY, "to", toY, "time", 1f, "onupdate", "UpdateY", "easetype", "easeInOutSine"));
	}
	
	private void UpdateSize(float size) {
		Camera.main.orthographicSize = size;
	}
	
	private void UpdateY(float y) {
		transform.position = new Vector3(transform.position.x, y, transform.position.z);
	}
	
	public void ChangeDirection(WallDirection wallDir, TurnDirection turnDir) {
		if (!Camera.main.orthographic) {
			wallDirection = wallDir;
			turnDirection = turnDir;
			
			if (turnDirection == TurnDirection.LEFT) targetRotation = currentRotation - 90;
			if (turnDirection == TurnDirection.RIGHT) targetRotation = currentRotation + 90;
			
			iTween.ValueTo(gameObject, iTween.Hash("from", currentRotation, "to", targetRotation, "time", ROTATE_TIME,
				"onupdate", "UpdateRotation", "oncomplete", "CompleteRotation", "easetype", "easeInOutSine"));
		}
	}
	
	private void UpdateRotation(float angle) {
		currentRotation = angle;
		
		float directionSign = 0;
		if (turnDirection == TurnDirection.LEFT) directionSign = -1;
		if (turnDirection == TurnDirection.RIGHT) directionSign = 1;
		
		float deltaAngle = (targetRotation - currentRotation) * Mathf.Deg2Rad * directionSign;
		
		if (wallDirection == WallDirection.NEGATIVE_X) {
			startX = -Mathf.Sin(deltaAngle) * radius;
			startZ = Mathf.Cos(deltaAngle) * radius * directionSign;
		}
		
		if (wallDirection == WallDirection.POSITIVE_Z) {
			startX = Mathf.Cos(deltaAngle) * radius * directionSign;
			startZ = Mathf.Sin(deltaAngle) * radius;
		}
		
		if (wallDirection == WallDirection.POSITIVE_X) {
			startX = Mathf.Sin(deltaAngle) * radius;
			startZ = -Mathf.Cos(deltaAngle) * radius * directionSign;
		}
		
		if (wallDirection == WallDirection.NEGATIVE_Z) {
			startX = -Mathf.Cos(deltaAngle) * radius * directionSign;
			startZ = -Mathf.Sin(deltaAngle) * radius;
		}
	}
	
	private void CompleteRotation() {
		if (currentRotation >= 360) currentRotation -= 360;
		if (currentRotation < 0) currentRotation += 360;
		SnapRotation(0);
		SnapRotation(90);
		SnapRotation(180);
		SnapRotation(270);
		UpdateRotation(currentRotation);
	}
	
	private void SnapRotation(float angle) {
		const float snapRange = 1;
		if (currentRotation > angle + snapRange && currentRotation < angle + snapRange) {
			currentRotation = angle;
		}
	}
}

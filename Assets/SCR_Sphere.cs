using UnityEngine;
using System.Collections;

public class SCR_Sphere : MonoBehaviour {
	public GameObject PFB_EXPLOSION;
	public Transform PFB_JUMP_EFFECT;
	public Transform PFB_GEM_SPLASH;
	public RectTransform PFB_PLUS_1;
	
	public RectTransform canvas;
	
	public Transform shadow;
	
	private const float MOVE_SPEED = 2.25f;
	
	private const float JUMP_SPEED = 6;
	private const float ROTATE_SPEED = -600;
	private const float TURN_SPEED = 4;
	
	private const float JUMP_EFFECT_OFFSET_Y = -0.19f;
	
	private const int NUMBER_ANIMALS = 3;
	
	private bool allowJump = false;
	private Rigidbody rb;
	
	private bool waitingToTurn = false;
	private bool turning = false;
	
	private Vector3 turnCenter;
	private Vector3 turnStart;
	private Vector3 turnEnd;
	private float turnRadius;
	private float turnAngle;
	private WallDirection wallDirection;
	private TurnDirection turnDirection;
	
	private Transform model;
	
	private float startRotationX;
	private float startRotationZ;
	
	private float speedX;
	private float speedZ;
	
	private SCR_Gameplay scrGameplay;
	private Transform lastCollision;
	private Transform lastCorner;
	
	private float endYLow;
	private float endYHigh;
	
	private static int currentAnimal = 0;

	// Use this for initialization
	void Start() {
		rb = GetComponent<Rigidbody>();
		speedX = MOVE_SPEED;
		speedZ = 0;
		turnAngle = 0;
		
		for (int i = 0; i < NUMBER_ANIMALS; i++) {
			transform.GetChild(i).gameObject.SetActive(false);
		}
		
		model = transform.GetChild(currentAnimal);
		model.gameObject.SetActive(true);
		
		startRotationX = transform.localEulerAngles.x;
		startRotationZ = transform.localEulerAngles.z;
		
		scrGameplay = Camera.main.GetComponent<SCR_Gameplay>();
		lastCollision = null;
		lastCorner = null;
		
		allowJump = true;
	}
	
	void Update() {
		if (scrGameplay.state == State.PLAY) {
			//Camera.main.GetComponent<SCR_Camera>().UpdateCamera();
		}
		
		shadow.position = new Vector3(transform.position.x, transform.position.y - 0.195f, transform.position.z);
	//}
	
	//void FixedUpdate() {
		if (scrGameplay.state == State.PLAY) {
			float deltaTime = Time.deltaTime;
			
			// Fall
			if (transform.position.y <= -1) {
				Camera.main.GetComponent<SCR_Gameplay>().GameOver();
			}
			
			// Change the direction of sphere
			if (waitingToTurn) {
				if ((wallDirection == WallDirection.NEGATIVE_X && transform.position.x >= turnStart.x) ||
					(wallDirection == WallDirection.POSITIVE_Z && transform.position.z <= turnStart.z) ||
					(wallDirection == WallDirection.POSITIVE_X && transform.position.x <= turnStart.x) ||
					(wallDirection == WallDirection.NEGATIVE_Z && transform.position.z >= turnStart.z)) {
					
					turnAngle = 0;
					Camera.main.GetComponent<SCR_Camera>().ChangeDirection(wallDirection, turnDirection);
					waitingToTurn = false;
					turning = true;
					rb.velocity = new Vector3(0, rb.velocity.y, 0);
					speedX = 0;
					speedZ = 0;
				}
			}
			
			if (turning) {
				turnAngle += TURN_SPEED * deltaTime;
				
				TurnSphere();
			}
			else {
				float x = transform.position.x + speedX * deltaTime;
				float z = transform.position.z + speedZ * deltaTime;
				
				transform.position = new Vector3(x, transform.position.y, z);
			}
					
			// Auto rotation
			model.localEulerAngles = new Vector3(model.localEulerAngles.x, model.localEulerAngles.y + ROTATE_SPEED * deltaTime, model.localEulerAngles.z);
			
			//Camera.main.GetComponent<SCR_Camera>().UpdateCamera();
		}
	}

	void OnCollisionEnter(Collision collision) {
		if (scrGameplay.state == State.PLAY) {
			if (collision.gameObject.tag == "Box" || collision.gameObject.tag == "Corner") {
				allowJump = true;
				
				if (lastCollision == null) {
					scrGameplay.IncreaseScore();
				}
				
				if (collision.transform != lastCollision && lastCollision != null) {
					if (lastCollision.gameObject.tag != "Corner") {
						// overlapsed boxes at corner
						if (collision.gameObject.tag != "Corner" || collision.transform != lastCorner) {
							// collision from top
							if (transform.position.y >= collision.transform.position.y + (collision.transform.localScale.y + transform.localScale.y) * 0.4f) {
								scrGameplay.IncreaseScore();
							}
						}
					}
				}
				
				lastCollision = collision.transform;
				
				if (collision.gameObject.tag == "Corner") {
					lastCorner = collision.transform;
				}
				
				shadow.gameObject.SetActive(true);
			}
			
			if (collision.gameObject.tag == "Corner" && !turning) {
				waitingToTurn = true;
				SetTurnParams(collision.transform);
			}
			
			if (collision.gameObject.tag == "Wall") {
				Camera.main.GetComponent<SCR_Gameplay>().GameOver();
				Instantiate(PFB_EXPLOSION, transform.position, collision.transform.rotation);
				//gameObject.SetActive(false);
				//shadow.gameObject.SetActive(false);
				
				float fromAngle = transform.localEulerAngles.x;
				float toAngle = 0;
				
				iTween.ValueTo(gameObject, iTween.Hash("from", fromAngle, "to", toAngle, "time", 0.4f, "onupdate", "UpdateAngle"));
				
				fromAngle = transform.GetChild(currentAnimal).localEulerAngles.z;
				toAngle = fromAngle + Random.Range(-20f, 20f);
				
				iTween.ValueTo(gameObject, iTween.Hash("from", fromAngle, "to", toAngle, "time", 0.4f, "onupdate", "UpdateAngleChild"));
				
				endYLow = transform.position.y;
				endYHigh = endYLow + 1;
				iTween.ValueTo(gameObject, iTween.Hash("from", endYLow, "to", endYHigh, "time", 0.2f, "onupdate", "UpdateY", "oncomplete", "Fall", "easetype", "easeOutSine"));
			}
		}
	}
	
	void OnCollisionExit(Collision collision) {
		if (collision.gameObject.tag == "Wall") {
			rb.velocity = Vector3.zero;
		}
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Gem") {
			other.gameObject.SetActive(false);
			Instantiate(PFB_GEM_SPLASH, other.transform.position, PFB_GEM_SPLASH.rotation);
			SCR_Gameplay.numberGems += 1;
			/*
			RectTransform plus1 = Instantiate(PFB_PLUS_1);
			plus1.SetParent(canvas, false);
			
			Vector3 viewportPoint = Camera.main.WorldToViewportPoint(other.transform.position);
			Vector2 anchor = new Vector2(viewportPoint.x + Random.Range(0f, 0.1f), viewportPoint.y);
			plus1.anchorMin = anchor;
			plus1.anchorMax = anchor;
			
			plus1.GetComponent<SCR_Plus1>().Fly();
			*/
		}
	}
	
	private void UpdateAngle(float angleX) {
		transform.localEulerAngles = new Vector3(angleX, transform.localEulerAngles.y, transform.localEulerAngles.z);
	}
	
	private void UpdateAngleChild(float angleZ) {
		transform.GetChild(0).localEulerAngles = new Vector3(transform.GetChild(0).localEulerAngles.x, transform.GetChild(0).localEulerAngles.y, angleZ);
	}
	
	private void UpdateY(float y) {
		transform.position = new Vector3(transform.position.x, y, transform.position.z);
	}
	
	private void Fall() {
		iTween.ValueTo(gameObject, iTween.Hash("from", endYHigh, "to", endYLow, "time", 0.2f, "onupdate", "UpdateY", "easetype", "easeInSine"));
	}
	
	private void SetTurnParams(Transform box) {
		float offsetY = (box.localScale.y + transform.localScale.y) * 0.5f;
		
		wallDirection = box.GetComponent<SCR_Box>().wallDirection;
		turnDirection = box.GetComponent<SCR_Corner>().turnDirection;
		
		float directionSign = 0;
		if (turnDirection == TurnDirection.LEFT) directionSign = -1;
		if (turnDirection == TurnDirection.RIGHT) directionSign = 1;
		
		if (wallDirection == WallDirection.NEGATIVE_X) {
			turnRadius = box.localScale.z * 0.5f;
			turnStart = new Vector3(box.position.x + box.localScale.x * 0.5f - turnRadius, box.position.y + offsetY, box.position.z);
			
			turnCenter = new Vector3(box.position.x + box.localScale.x * 0.5f - turnRadius, box.position.y + offsetY, box.position.z - turnRadius * directionSign);
			turnEnd = new Vector3(box.position.x + box.localScale.x * 0.5f, box.position.y + offsetY, box.position.z - turnRadius * directionSign);
		}
		
		if (wallDirection == WallDirection.POSITIVE_Z) {
			turnRadius = box.localScale.x * 0.5f;
			turnStart = new Vector3(box.position.x, box.position.y + offsetY, box.position.z - box.localScale.z * 0.5f + turnRadius);
			
			turnCenter = new Vector3(box.position.x - turnRadius * directionSign, box.position.y + offsetY, box.position.z - box.localScale.z * 0.5f + turnRadius);
			turnEnd = new Vector3(box.position.x - turnRadius * directionSign, box.position.y + offsetY, box.position.z - box.localScale.z * 0.5f);
		}
		
		if (wallDirection == WallDirection.POSITIVE_X) {
			turnRadius = box.localScale.z * 0.5f;
			turnStart = new Vector3(box.position.x - box.localScale.x * 0.5f + turnRadius, box.position.y + offsetY, box.position.z);
			
			turnCenter = new Vector3(box.position.x - box.localScale.x * 0.5f + turnRadius, box.position.y + offsetY, box.position.z + turnRadius * directionSign);
			turnEnd = new Vector3(box.position.x - box.localScale.x * 0.5f, box.position.y + offsetY, box.position.z + turnRadius * directionSign);
		}
		
		if (wallDirection == WallDirection.NEGATIVE_Z) {
			turnRadius = box.localScale.x * 0.5f;
			turnStart = new Vector3(box.position.x, box.position.y + offsetY, box.position.z + box.localScale.z * 0.5f - turnRadius);
			
			turnCenter = new Vector3(box.position.x + turnRadius * directionSign, box.position.y + offsetY, box.position.z + box.localScale.z * 0.5f - turnRadius);
			turnEnd = new Vector3(box.position.x + turnRadius * directionSign, box.position.y + offsetY, box.position.z + box.localScale.z * 0.5f);
		}
	}
	
	private void TurnSphere() {
		float x = 0;
		float z = 0;
		
		float directionSign = 0;
		if (turnDirection == TurnDirection.LEFT) directionSign = -1;
		if (turnDirection == TurnDirection.RIGHT) directionSign = 1;
		
		if (wallDirection == WallDirection.NEGATIVE_X) {
			x = turnCenter.x + Mathf.Sin(turnAngle) * turnRadius;
			z = turnCenter.z + Mathf.Cos(turnAngle) * turnRadius * directionSign;
			
			transform.localEulerAngles = new Vector3(startRotationX, turnAngle * Mathf.Rad2Deg * directionSign, startRotationZ);
			
			if ((z - turnEnd.z) * directionSign <= 0) {
				turning = false;
				speedX = 0;
				speedZ = -MOVE_SPEED * directionSign;
				
				transform.localEulerAngles = new Vector3(startRotationX, 90 * directionSign, startRotationZ);
			}
		}
		
		if (wallDirection == WallDirection.POSITIVE_Z) {
			x = turnCenter.x + Mathf.Cos(turnAngle) * turnRadius * directionSign;
			z = turnCenter.z - Mathf.Sin(turnAngle) * turnRadius;
			
			if (turnDirection == TurnDirection.LEFT) {
				transform.localEulerAngles = new Vector3(startRotationX, -270 - turnAngle * Mathf.Rad2Deg, startRotationZ);
			}
			
			if (turnDirection == TurnDirection.RIGHT) {
				transform.localEulerAngles = new Vector3(startRotationX, 90 + turnAngle * Mathf.Rad2Deg, startRotationZ);
			}
			
			if ((x - turnEnd.x) * directionSign <= 0) {
				turning = false;
				speedX = -MOVE_SPEED * directionSign;
				speedZ = 0;
				
				if (turnDirection == TurnDirection.LEFT) {
					transform.localEulerAngles = new Vector3(startRotationX, 0, startRotationZ);
				}
				
				if (turnDirection == TurnDirection.RIGHT) {
					transform.localEulerAngles = new Vector3(startRotationX, 180, startRotationZ);
				}
			}
		}
		
		if (wallDirection == WallDirection.POSITIVE_X) {
			x = turnCenter.x - Mathf.Sin(turnAngle) * turnRadius;
			z = turnCenter.z - Mathf.Cos(turnAngle) * turnRadius * directionSign;
			
			transform.localEulerAngles = new Vector3(startRotationX, (180 + turnAngle * Mathf.Rad2Deg) * directionSign, startRotationZ);

			if ((z - turnEnd.z) * directionSign >= 0) {
				turning = false;
				speedX = 0;
				speedZ = MOVE_SPEED * directionSign;
				
				transform.localEulerAngles = new Vector3(startRotationX, 270 * directionSign, startRotationZ);
			}
		}
		
		if (wallDirection == WallDirection.NEGATIVE_Z) {
			x = turnCenter.x - Mathf.Cos(turnAngle) * turnRadius * directionSign;
			z = turnCenter.z + Mathf.Sin(turnAngle) * turnRadius;
			
			if (turnDirection == TurnDirection.LEFT) {
				transform.localEulerAngles = new Vector3(startRotationX, -90 - turnAngle * Mathf.Rad2Deg, startRotationZ);
			}
			
			if (turnDirection == TurnDirection.RIGHT) {
				transform.localEulerAngles = new Vector3(startRotationX, 270 + turnAngle * Mathf.Rad2Deg, startRotationZ);
			}
			
			if ((x - turnEnd.x) * directionSign >= 0) {
				turning = false;
				speedX = MOVE_SPEED * directionSign;
				speedZ = 0;
				
				if (turnDirection == TurnDirection.LEFT) {
					transform.localEulerAngles = new Vector3(startRotationX, -180, startRotationZ);
				}
				
				if (turnDirection == TurnDirection.RIGHT) {
					transform.localEulerAngles = new Vector3(startRotationX, 0, startRotationZ);
				}
			}
		}
		
		transform.position = new Vector3(x, transform.position.y, z);
	}

	public bool Jump() {
		if (allowJump && Physics.Raycast(transform.position, new Vector3(0, -1, 0), 20, 1 << SCR_Box.LAYER)) {
			Vector3 v = rb.velocity;
			rb.velocity = new Vector3(0, JUMP_SPEED, 0);
			
			Vector3 pos = new Vector3(transform.position.x, transform.position.y + JUMP_EFFECT_OFFSET_Y, transform.position.z);
			
			Instantiate(PFB_JUMP_EFFECT, pos, PFB_JUMP_EFFECT.transform.rotation);
			shadow.gameObject.SetActive(false);

			allowJump = false;

			return true;
		}

		return false;
	}
	
	public void RestrictPosition(Transform box) {
		if (!turning) {
			SCR_Box scrBox = box.GetComponent<SCR_Box>();
			if (scrBox != null) {
				WallDirection wallDirection = scrBox.wallDirection;
				
				if (wallDirection == WallDirection.NEGATIVE_X || wallDirection == WallDirection.POSITIVE_X) {
					transform.position = new Vector3(transform.position.x, transform.position.y, box.position.z);
				}
				
				if (wallDirection == WallDirection.NEGATIVE_Z || wallDirection == WallDirection.POSITIVE_Z) {
					transform.position = new Vector3(box.position.x, transform.position.y, transform.position.z);
				}
			}
		}
	}
	
	public void ChangeAnimalLeft() {
		transform.GetChild(currentAnimal).gameObject.SetActive(false);
		
		currentAnimal--;
		if (currentAnimal < 0) currentAnimal = NUMBER_ANIMALS - 1;
		
		transform.GetChild(currentAnimal).gameObject.SetActive(true);
		
		model = transform.GetChild(currentAnimal);
	}
	
	public void ChangeAnimalRight() {
		transform.GetChild(currentAnimal).gameObject.SetActive(false);
		
		currentAnimal++;
		if (currentAnimal >= NUMBER_ANIMALS) currentAnimal = 0;
		
		transform.GetChild(currentAnimal).gameObject.SetActive(true);
		
		model = transform.GetChild(currentAnimal);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WallDirection {
	NONE,
	NEGATIVE_X,
	POSITIVE_X,
	NEGATIVE_Z,
	POSITIVE_Z
}

public enum BoxLength {
	SHORT,
	MEDIUM,
	LONG
}

public class SCR_Box : MonoBehaviour {
	public const int LAYER = 8;
	
	public Transform PFB_GEM;
	
	public GameObject PFB_WALL;
	public WallDirection wallDirection = WallDirection.NONE;
	
	public Transform cylinder = null;
	
	private const float epsilon = 0.01f;
	
	// gem
	private const float offsetY = 0.23f;
	private const float distance = 0.5f;
	
	private Transform[] gems = null;
	
	private BoxLength boxLength;
	
	// Use this for initialization
	void Start() {
		Vector3 pos = Vector3.zero;
		Vector3 rot = Vector3.zero;
		if (wallDirection == WallDirection.NEGATIVE_X) {
			pos = new Vector3(transform.position.x - transform.localScale.x * 0.5f - epsilon, transform.position.y, transform.position.z);
			rot = new Vector3(0, 90, 0);
		}
		
		if (wallDirection == WallDirection.POSITIVE_X) {
			pos = new Vector3(transform.position.x + transform.localScale.x * 0.5f + epsilon, transform.position.y, transform.position.z);
			rot = new Vector3(0, -90, 0);
		}
		
		if (wallDirection == WallDirection.NEGATIVE_Z) {
			pos = new Vector3(transform.position.x, transform.position.y, transform.position.z - transform.localScale.z * 0.5f - epsilon);
			rot = new Vector3(0, 0, 0);
		}
		
		if (wallDirection == WallDirection.POSITIVE_Z) {
			pos = new Vector3(transform.position.x, transform.position.y, transform.position.z + transform.localScale.z * 0.5f + epsilon);
			rot = new Vector3(0, 180, 0);
		}
		
		if (wallDirection != WallDirection.NONE) {
			Instantiate(PFB_WALL, pos, Quaternion.Euler(rot));
		}
	}
	
	void Update() {
		if (cylinder != null) {
			cylinder.position = new Vector3(cylinder.position.x, transform.position.y, cylinder.position.z);
		}
		
		if (gems != null) {
			for (int i = 0; i < gems.Length; i++) {
				gems[i].position = new Vector3(gems[i].position.x, transform.position.y + offsetY, gems[i].position.z);
			}
		}
	}
	
	public void SetLength(BoxLength length) {
		boxLength = length;
	}
	
	public void GenerateGem() {
		float r = Random.Range(0f, 1f);
		if (r >= 0.5f) {
			int numberGems = 2;
			if (boxLength == BoxLength.MEDIUM) numberGems = 3;
			if (boxLength == BoxLength.LONG) numberGems = 5;
			
			gems = new Transform[numberGems];
			
			for (int i = 0; i < numberGems; i++) {
				Vector3 pos = Vector3.zero;
				
				if (wallDirection == WallDirection.NEGATIVE_X || wallDirection == WallDirection.POSITIVE_X ||
				   (wallDirection == WallDirection.NONE && transform.localScale.x >= transform.localScale.z)) {
					pos = new Vector3(transform.position.x + i * distance - (numberGems - 1) * distance * 0.5f, transform.position.y + offsetY, transform.position.z);
				}
				
				if (wallDirection == WallDirection.NEGATIVE_Z || wallDirection == WallDirection.POSITIVE_Z ||
				   (wallDirection == WallDirection.NONE && transform.localScale.z >= transform.localScale.x)) {
					pos = new Vector3(transform.position.x, transform.position.y + offsetY, transform.position.z + i * distance - (numberGems - 1) * distance * 0.5f);
				}
				
				gems[i] = Instantiate(PFB_GEM, pos, PFB_GEM.rotation);
			}
		}
	}
	
	public void Destroy() {
		if (cylinder != null) {
			Destroy(cylinder.gameObject);
		}
		
		if (gems != null) {
			for (int i = 0; i < gems.Length; i++) {
				Destroy(gems[i].gameObject);
			}
		}
		
		Destroy(gameObject);
	}
}

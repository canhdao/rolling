using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnDirection {
	LEFT,
	RIGHT
}

public class SCR_Corner : MonoBehaviour {
	public TurnDirection turnDirection = TurnDirection.LEFT;
}

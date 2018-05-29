using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_ColorConverter {
	private const float Epsilon = 1e-10f;
	private static Vector3 HCYwts = new Vector3(0.299f, 0.587f, 0.114f);

	public static Vector3 HUEToRGB(float H) {
		float R = Mathf.Abs(H * 6 - 3) - 1;
		float G = 2 - Mathf.Abs(H * 6 - 2);
		float B = 2 - Mathf.Abs(H * 6 - 4);
		
		if (R < 0) R = 0;
		if (R > 1) R = 1;
		if (G < 0) G = 0;
		if (G > 1) G = 1;
		if (B < 0) B = 0;
		if (B > 1) B = 1;
		
		return new Vector3(R, G, B);
	}
	
	public static Vector3 RGBToHCV(Vector3 RGB) {
		Vector4 P = (RGB.y < RGB.z) ? new Vector4(RGB.z, RGB.y, -1.0f, 2.0f/3.0f) : new Vector4(RGB.y, RGB.z, 0.0f, -1.0f/3.0f);
		Vector4 Q = (RGB.x < P.x) ? new Vector4(P.x, P.y, P.w, RGB.x) : new Vector4(RGB.x, P.y, P.z, P.x);
		float C = Q.x - Mathf.Min(Q.w, Q.y);
		float H = Mathf.Abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
		return new Vector3(H, C, Q.x);
	}
	
	public static Vector3 RGBToHCY(Vector3 RGB) {
		Vector3 HCV = RGBToHCV(RGB);
		float Y = Vector3.Dot(RGB, HCYwts);
		float Z = Vector3.Dot(HUEToRGB(HCV.x), HCYwts);
		if (Y < Z) {
			HCV.y *= Z / (Epsilon + Y);
		}
		else {
			HCV.y *= (1 - Z) / (Epsilon + 1 - Y);
		}
		
		return new Vector3(HCV.x, HCV.y, Y);
	}
	
	public static Vector3 HCYToRGB(Vector3 HCY) {
		Vector3 RGB = HUEToRGB(HCY.x);
		float Z = Vector3.Dot(RGB, HCYwts);
		if (HCY.z < Z) {
			HCY.y *= HCY.z / Z;
		}
		else if (Z < 1) {
			HCY.y *= (1 - HCY.z) / (1 - Z);
		}
		
		// return (RGB - Z) * HCY.y + HCY.z;
		return new Vector3((RGB.x - Z) * HCY.y + HCY.z, (RGB.y - Z) * HCY.y + HCY.z, (RGB.z - Z) * HCY.y + HCY.z);
	}
}

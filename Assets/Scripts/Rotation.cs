using UnityEngine;
using System.Collections;

public class Rotation : MonoBehaviour {

	public float speed = 45f;
	public Vector3 axis;

	void Update () {
		transform.Rotate (axis * Time.deltaTime * speed);
	}
}

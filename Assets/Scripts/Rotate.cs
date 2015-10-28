using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {
	private float maxDistance = 60.0f;
	private float starDistance = 30.0f;

	private float maxDistanceSqr;
	private Vector3 originalPosition;

	// Use this for initialization
	void Start () {
		maxDistanceSqr = maxDistance * maxDistance;
		originalPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(Vector3.left * Time.deltaTime * 2);
		transform.Rotate(Vector3.up * Time.deltaTime * 2, Space.World);

		transform.Translate(Vector3.back * Time.deltaTime * 1, Space.World);

		if ((transform.position).sqrMagnitude > maxDistanceSqr) {
			transform.position = originalPosition + Vector3.forward * 20;
		}
	}
}

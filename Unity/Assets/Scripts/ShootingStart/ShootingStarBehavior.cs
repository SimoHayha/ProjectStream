using UnityEngine;
using System.Collections;

public class ShootingStarBehavior : MonoBehaviour {

    public float Speed = 1.0f;

	void Start () {
	
	}
	
	void Update () {
        transform.Translate(transform.right * Speed * Time.deltaTime);
	}
}

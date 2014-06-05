using UnityEngine;
using System.Collections;

public class BulletBehavior : MonoBehaviour {

    public float Dispersion;
    public float Speed = 10.0f;
    public Vector3 Direction = Vector3.zero;

	void Start () {
        Destroy(gameObject, 5.0f);
        Direction = Vector3.right + Vector3.up * Random.Range(-Dispersion, Dispersion);
	}
	
	void Update () {
        UpdatePosition();
	}

    private void UpdatePosition()
    {
        transform.Translate(Direction * Speed * Time.deltaTime);
    }

}

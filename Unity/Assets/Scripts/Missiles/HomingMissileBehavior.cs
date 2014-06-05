using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class HomingMissileBehavior : MonoBehaviour {

    public float StartSpeed = 2.0f;
    public float FullSpeed = 10.0f;
    public float Force = 0.1f;

    private float _TimeSinceStart;
    private float _Speed;

    void Start()
    {
        _Speed = StartSpeed;
        _TimeSinceStart = 0.0f;

        Destroy(gameObject, 5.0f);
    }

    void Update()
    {
        _TimeSinceStart += Time.deltaTime;
        _Speed = Mathf.Lerp(StartSpeed, FullSpeed, _TimeSinceStart);

        var mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        mousePosition.z = 0.0f;

        Vector3 targetDelta = -(mousePosition - transform.position);

        float angleDiff = Vector3.Angle(transform.right, targetDelta);

        Vector3 cross = Vector3.Cross(transform.right, targetDelta);

        rigidbody.AddTorque(cross * angleDiff * Force);

        transform.Translate(Vector3.right * _Speed * Time.deltaTime);
    }
}

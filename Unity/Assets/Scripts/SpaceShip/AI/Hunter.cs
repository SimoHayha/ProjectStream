using UnityEngine;
using System.Collections;

public class Hunter : BasicAI {

    protected override void OnUpdate()
    {
        if (Health <= 0.0f)
            Destroy(gameObject);

        Arrive();
    }

    protected override void OnStart()
    {
    }

    protected override void OnCollision(Collision2D coll)
    {
        if (coll.gameObject.tag == "PlayerBullet")
        {
            Destroy(coll.gameObject);
            Health -= 1.0f;
        }
    }

    private void Arrive()
    {
        var direction = _Player.transform.position - transform.position;
        direction.z = 0.0f;
        var distance = direction.magnitude;
        var decelerationFactor = distance / 5.0f;
        var speed = MoveSpeed * decelerationFactor;

        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction, -transform.forward), 10.0f * Time.deltaTime);
        Quaternion rotation = Quaternion.LookRotation(transform.position - _Player.transform.position, Vector3.forward);
        transform.rotation = rotation;
        float rot_z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0.0f, 0.0f, rot_z - 90.0f);

        var moveVector = direction.normalized * Time.deltaTime * speed;
        transform.position += moveVector;
    }
}

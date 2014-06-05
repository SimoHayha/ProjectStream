using UnityEngine;
using System.Collections;
using Assets.Scripts.BassWrapper;

public abstract class BasicAI : MonoBehaviour {

    protected BassManager _Bass;
    protected GameObject _Player;

    public float MoveSpeed;
    public float Health;

	void Start () {
        _Bass = GameObject.FindGameObjectWithTag("BassManager").GetComponent<BassManager>();
        _Player = GameObject.FindGameObjectWithTag("Player");

        OnStart();
	}
	
	void Update () {
        OnUpdate();
	}

    void OnCollisionEnter2D(Collision2D coll)
    {
        Debug.Log("PWET");
        OnCollision(coll);
    }

    protected abstract void OnUpdate();
    protected abstract void OnStart();
    protected abstract void OnCollision(Collision2D coll);
}

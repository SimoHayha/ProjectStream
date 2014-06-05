using UnityEngine;
using System.Collections;
using Assets.Scripts.BassWrapper;

public class Bands2 : MonoBehaviour {

    public int Bands;
    public GameObject Emitter;

    private BassManager _BassManager;

	void Start () {
        Vector3 pos = transform.position;

        for (int i = 0; i < Bands; ++i)
        {
            GameObject copy = Instantiate(Emitter, pos, Quaternion.identity) as GameObject;
            pos.x += 1.0f;
        }
	}
	
	void Update () {
	
	}
}

using UnityEngine;
using System.Collections;
using Assets.Scripts.BassWrapper;

public class DeathLazerBehavior : MonoBehaviour {

    private BassManager _BassManager;

	void Start () {
        _BassManager = GameObject.FindGameObjectWithTag("BassManager").GetComponent<BassManager>();
	}
	
	void Update () {
        if (Input.GetMouseButton(2))
        {
            Color color = renderer.material.color;
            color.a = 1.0f * _BassManager.GetIntensity();
            renderer.material.color = color;
            renderer.enabled = true;
        }
        else
            renderer.enabled = false;



        //Vector3 scale = transform.localScale;
        //scale.x = 5.0f + 2.5f * _BassManager.GetIntensity();
        //transform.localScale = scale;
	}
}

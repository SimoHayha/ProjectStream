using UnityEngine;
using System.Collections;
using Assets.Scripts.BassWrapper;

public class BackgroundBehavior : MonoBehaviour {

    private BassManager _Bass;

	void Start () {
	    try
        {
            _Bass = GameObject.FindGameObjectWithTag("BassManager").GetComponent<BassManager>();
        }
        catch
        {
            enabled = false;
        }
	}
	
	void LateUpdate () {
        float intensity = _Bass.GetIntensity();
        Color color = renderer.material.GetColor("_Color");

        color.a = Mathf.Clamp(intensity, 0.2f, 0.8f);

        renderer.material.SetColor("_Color", color);
	}
}

using UnityEngine;
using System.Collections;
using Assets.Scripts.BassWrapper;

public class PitchLightBehavior : MonoBehaviour {

    public enum PitchType
    {
        HighPitch,
        LowPitch
    }

    public PitchType Pitch;

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
        if (Pitch == PitchType.HighPitch)
        {

        }
        else
        {

        }

        float intensity = _Bass.GetIntensity();
        light.intensity = 8 * intensity * 2.0f;
	}
}

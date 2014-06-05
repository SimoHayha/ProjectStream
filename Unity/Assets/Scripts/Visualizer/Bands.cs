using UnityEngine;
using System.Collections;
using Assets.Scripts.BassWrapper;
using System;

public class Bands : MonoBehaviour {

    public int NumBand = 64;
    public GameObject Band;

    private BassManager _Bass;
    private GameObject[] _Bands;

	void Start () {
        //NumBand = (int)(Screen.width / 5);
        //NumBand = (int)(Screen.width / (Band.GetComponent<MeshRenderer>().bounds.size.x * 75));

	    try
        {
            _Bass = GameObject.Find("BassManager").GetComponent<BassManager>();
        }
        catch (NullReferenceException)
        {
            enabled = false;
        }

        _Bands = new GameObject[NumBand];
        float offsetx = -8.5f;
        for (int i = 0; i < NumBand; ++i)
        {
            Vector3 position = new Vector3();
            position.x = offsetx;
            position.y = -4.5f;
            position.z = 0.0f;

            _Bands[i] = Instantiate(Band, position, Quaternion.identity) as GameObject;

            offsetx += 0.2f;
        }
	}
	
	void Update () {
        float[] fft = _Bass.GetPeaks(NumBand);

        if (fft != null)
        {
            for (int i = 0; i < NumBand; ++i)
            {
                float peak = fft[i] * 10.0f;

                Vector3 scale = _Bands[i].transform.localScale;
                scale.y = Mathf.Clamp(peak, 0.1f, 5.0f);
                _Bands[i].transform.localScale = scale;
                _Bands[i].renderer.material.color = Color.Lerp(Color.blue, Color.red, peak);

                ParticleSystem particles = _Bands[i].GetComponentInChildren<ParticleSystem>();
                particles.startColor = _Bands[i].renderer.material.GetColor("_Color");
                if (peak > 0.2f)
                    particles.Emit(1);
                particles.startSpeed = Mathf.Lerp(1.0f, 10.0f, peak);
            }
            //float[] fft = _Bass.GetCurrentChannelData();

            //if (fft != null)
            //{
            //    int iteration = fft.Length / NumBand;

            //    int b0 = 0;
            //    for (int x = 0; x < NumBand; ++x)
            //    {
            //        float peak = 0.0f;
            //        int b1 = (int)Mathf.Pow(2, x * 10.0f / (NumBand - 1));
            //        if (b1 > 1023)
            //            b1 = 1023;
            //        if (b1 <= b0)
            //            b1 = b0 + 1;
            //        for (; b0 < b1; b0++)
            //            if (peak < fft[1 + b0])
            //                peak = fft[1 + b0];
            //        float y = Mathf.Sqrt(peak);
            //        if (y > 1.0f)
            //            y = 1.0f;

            //        Vector3 scale = _Bands[x].transform.localScale;
            //        scale.y = Mathf.Clamp(y * 2.0f, 0.05f, float.MaxValue);
            //        _Bands[x].transform.localScale = scale;
            //        _Bands[x].renderer.material.SetColor("_Color", Color.Lerp(Color.blue, Color.red, y));

            //        ParticleSystem particles = _Bands[x].GetComponentInChildren<ParticleSystem>();
            //        particles.startColor = _Bands[x].renderer.material.GetColor("_Color");
            //        if (y > 0.2f)
            //            particles.Emit(1);
            //        particles.startSpeed = Mathf.Lerp(1.0f, 10.0f, y);
            //    }
            //}
        }
        else
        {
            for (int i = 0; i < NumBand; ++i)
            {
                Vector3 scale = _Bands[i].transform.localScale;
                scale.y = 0.05f;
                _Bands[i].transform.localScale = scale;
                _Bands[i].renderer.material.SetColor("_Color", Color.yellow);
            }
        }
	}
}

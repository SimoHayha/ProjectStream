using UnityEngine;
using System.Collections;
using Assets.Scripts.BassWrapper;

public class RoundVisualizer : MonoBehaviour {

    public GameObject Obj;
    public int NumberOfElements = 64;

    private GameObject[] _Bars;
    private BassManager _BassManager;
    private float _GlobalOffset = 0.0f;
    private bool _IsExplode;

	void Start () {
        float offset = 360.0f / NumberOfElements;
        float current = 0.0f;

        _Bars = new GameObject[NumberOfElements];
	    for (int i = 0; i < NumberOfElements; ++i)
        {
            var pos = GetCircle(transform.position, 1.0f, current);
            var rot = Quaternion.FromToRotation(Vector3.forward, transform.position - pos);
            _Bars[i] = Instantiate(Obj, pos, rot) as GameObject;
            _Bars[i].transform.parent = gameObject.transform;
            current += offset;
        }

        int div = NumberOfElements / 3;
        for (int i = 0; i < div; ++i)
            _Bars[i].renderer.material.color = Color.Lerp(Color.red, Color.green, (float)i / (float)div);
        for (int i = 0; i < div; ++i)
            _Bars[i + div].renderer.material.color = Color.Lerp(Color.green, Color.blue, (float)i / (float)div);
        for (int i = 0; i < div; ++i)
            _Bars[i + div * 2].renderer.material.color = Color.Lerp(Color.blue, Color.red, (float)i / (float)div);

            //for (int i = 0; i < _Bars.Length; ++i)
            //    _Bars[i].renderer.material.color = Color.Lerp(Color.red, Color.blue, (float)i / (float)_Bars.Length);

        _BassManager = GameObject.FindGameObjectWithTag("BassManager").GetComponent<BassManager>();
        _IsExplode = false;
	}
	
	void Update () {
        if (!_IsExplode)
        {
            float offset = 360.0f / NumberOfElements;
            float current = _GlobalOffset;
            float[] fft = _BassManager.GetPeaks(NumberOfElements);

            if (fft != null)
            {
                for (int i = 0; i < NumberOfElements; ++i)
                {
                    if (fft[i] >= 0.15f)
                        _Bars[i].light.enabled = true;
                    else
                        _Bars[i].light.enabled = false;

                    float clampedValue = Mathf.Clamp(fft[i], 0.1f, 1.0f);
                    _Bars[i].transform.localScale = new Vector3(clampedValue, clampedValue, clampedValue);
                    _Bars[i].transform.position = GetCircle(transform.position, 1.0f + fft[i] * 2.0f, current);
                    _Bars[i].transform.rotation = Quaternion.FromToRotation(Vector3.forward, transform.position - _Bars[i].transform.position);
                    current += offset;
                }
            }

            _GlobalOffset += _BassManager.GetIntensity();
            if (_GlobalOffset >= 360.0f)
                _GlobalOffset = 0.0f;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var bar in _Bars)
            {
                bar.rigidbody2D.velocity = Vector2.zero;
                bar.rigidbody2D.AddForce((bar.transform.position - transform.position) * 200.0f);
            }

            _IsExplode = !_IsExplode;
        }
	}

    private Vector3 GetCircle(Vector3 center, float radius, float angle)
    {
        Vector3 pos = new Vector3();
        pos.x = center.x + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
        pos.z = center.z;
        return pos;
    }
}

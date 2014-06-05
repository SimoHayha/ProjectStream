using UnityEngine;
using System.Collections;
using Assets.Scripts.BassWrapper;

public class PointCloud : MonoBehaviour {

    public int x;
    public int y;
    public GameObject Prefab;

    private GameObject[] _Points;
    private BassManager _BassManager;

	void Start () {
        _Points = new GameObject[x * y];

        for (int i = 0; i < x; ++i)
        {
            for (int j = 0; j < y; ++j)
            {
                Vector3 position = transform.position + new Vector3(j / 1.0f, i / 1.35f, 0.0f);
                _Points[i * x + j] = Instantiate(Prefab, position, Quaternion.identity) as GameObject;
                _Points[i * x + j].transform.parent = transform;
            }
        }

        _BassManager = GameObject.FindGameObjectWithTag("BassManager").GetComponent<BassManager>();
	}
	
	void Update () {
        float[] fft = _BassManager.GetPeaks(_Points.Length);

        if (fft != null)
        {
            for (int i = 0; i < _Points.Length; ++i)
                _Points[i].transform.localScale = new Vector3(fft[i], fft[i], fft[i]);
        }
	}
}

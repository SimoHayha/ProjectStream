using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class StreamAPI : MonoBehaviour {

    [DllImport("Stream")]
    private static extern bool Initialize();

	void Start () {
        DontDestroyOnLoad(gameObject);

        Debug.Log("Hello");
        if (Initialize())
            Debug.Log("StreamAPI loaded");
        else
            Debug.Log("StreamAPI error");
	}
	
	void Update () {
	
	}
}

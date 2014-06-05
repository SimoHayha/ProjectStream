using UnityEngine;
using System.Collections;
using Assets.Scripts.BassWrapper;

public class StardustBehavior : MonoBehaviour {

    private BassManager _Bass;
    int _NbParticles;

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

    void LateUpdate()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[1000];
        int nbParticles = particleSystem.GetParticles(particles);
        float intensity = _Bass.GetIntensity();

        for (int i = 0; i < nbParticles; ++i)
            particles[i].size = Mathf.Clamp(intensity + Random.Range(-0.1f, 0.1f), 0.1f, 1.0f);

        particleSystem.SetParticles(particles, nbParticles);
    }
}

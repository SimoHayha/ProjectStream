using UnityEngine;
using System.Collections;
using Assets.Scripts.BassWrapper;
using System;
using System.Runtime.InteropServices;

[RequireComponent(typeof(CharacterController))]
public class PlayerShip : MonoBehaviour {

    public float Speed = 5.0f;
    public GameObject Bullet;
    public GameObject Missile;

    private CharacterController _CharacterController;
    private BassManager _Bass;
    private bool _AllowFire;
    private bool _AllowMissile;
    private GameObject[] _Cannons;
    private int _IndexInCannons;
    private PropulsorBehavior _Propulsor;
    private PerkManager _PerkManager;

    void Start()
    {
        _PerkManager = GameObject.FindGameObjectWithTag("PerkManager").GetComponent<PerkManager>();
        _AllowFire = true;
        _AllowMissile = true;
        _CharacterController = GetComponent<CharacterController>();
        _Cannons = GameObject.FindGameObjectsWithTag("PlayerCannon");
        _Propulsor = GetComponentInChildren<PropulsorBehavior>();
        try
        {
            _Bass = GameObject.Find("BassManager").GetComponent<BassManager>();
        }
        catch (NullReferenceException)
        {
            enabled = false;
        }
    }

    void Update()
    {
        HandleInput();
        HandleFire();
    }

    private void HandleInput()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        HandlePropulsion(horizontal, vertical);

        _CharacterController.Move(Vector3.right * horizontal * Time.deltaTime * Speed);
        _CharacterController.Move(Vector3.up * vertical * Time.deltaTime * Speed);

        var mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        Quaternion rotation = Quaternion.LookRotation(transform.position - mousePosition, Vector3.forward);
        transform.rotation = rotation;
        transform.eulerAngles = new Vector3(0.0f, 0.0f, transform.eulerAngles.z + 90.0f);
    }

    private void HandlePropulsion(float horizontal, float vertical)
    {
        if (horizontal != 0.0f || vertical != 0.0f)
            _Propulsor.SetPropulsorType(PropulsorBehavior.PropulsorType.SpeedUp);
        else
            _Propulsor.SetPropulsorType(PropulsorBehavior.PropulsorType.Idle);
    }

    private void HandleFire()
    {
        if (Input.GetAxis("Fire1") != 0.0f && _AllowFire)
        {
            StartCoroutine("Fire");
        }
        if (Input.GetAxis("Fire2") != 0.0f && _AllowMissile)
        {
            StartCoroutine("FireMissile");
            if (_PerkManager.ToggleDoubleMissile)
                StartCoroutine("FireMissile");
        }
    }

    private IEnumerator FireMissile()
    {
        _AllowMissile = false;

        FireMissileFrom(_Cannons[_IndexInCannons].transform.position);
        _IndexInCannons++;
        if (_IndexInCannons >= _Cannons.Length)
            _IndexInCannons = 0;

        yield return new WaitForSeconds(0.25f);
        _AllowMissile = true;
    }

    private IEnumerator Fire()
    {
        _AllowFire = false;

        FireBulletFrom(_Cannons[_IndexInCannons].transform.position);
        _IndexInCannons++;
        if (_IndexInCannons >= _Cannons.Length)
            _IndexInCannons = 0;

        float wait = Mathf.Clamp(1 - _Bass.GetPeakFrequency() - 0.8f, 0.0f, 1.0f);
        yield return new WaitForSeconds(wait);
        _AllowFire = true;
    }

    private void FireMissileFrom(Vector3 startingPoint)
    {
        Instantiate(Missile, startingPoint, transform.rotation);
    }

    private void FireBulletFrom(Vector3 startingPoint)
    {
        var mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        var direction = transform.position - mousePosition;
        Quaternion rotation = Quaternion.LookRotation(transform.position - mousePosition, Vector3.forward);
        var bullet = Instantiate(Bullet, startingPoint, rotation) as GameObject;
        bullet.transform.eulerAngles = new Vector3(0.0f, 0.0f, bullet.transform.eulerAngles.z + 90.0f);
        bullet.GetComponent<BulletBehavior>().Direction = direction.normalized;
        if (_PerkManager.ToggleFocus)
            bullet.GetComponent<BulletBehavior>().Dispersion = 0.05f;
        else
            bullet.GetComponent<BulletBehavior>().Dispersion = 0.1f;

        float intensity = Mathf.Pow(_Bass.GetPeakFrequency() + 1.0f, 2.0f);
        //float intensity = _Bass.GetIntensity() + 0.5f;
        bullet.transform.localScale = new Vector3(intensity, intensity, intensity);
    }
}

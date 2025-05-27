using System;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] private Gun CurrentGun;

    private float currentFireRate;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    // Update is called once per frame
    void Update()
    {
        GunFireRateCalc();
        TryFire();
    }

    private void TryFire()
    {
        if (Input.GetButton("Fire1") && currentFireRate <= 0)
        {
            Fire();
        }
    }

    private void Fire()
    {
        currentFireRate = CurrentGun.fireRate;
        Shoot();
    }

    private void Shoot()
    {
        PlaySE(CurrentGun.fire_Sound);
        CurrentGun.muzzleFlash.Play();
        Debug.Log("ÃÑ¾Ë °Ý¹ß");
    }
    private void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }

    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
            currentFireRate -= Time.deltaTime;
    }
}

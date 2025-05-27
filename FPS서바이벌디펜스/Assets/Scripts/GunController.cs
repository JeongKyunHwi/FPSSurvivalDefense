using System;
using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] private Gun CurrentGun;

    private float currentFireRate;
    private bool isReload = false;
    [HideInInspector]
    public bool isfineSightMode;

    [SerializeField] private Vector3 originPos;

    private AudioSource audioSource;

    private RaycastHit hitInfo;

    [SerializeField] private Camera theCam;

    [SerializeField] private GameObject hit_effect_prefab;

    private void Start()
    {
        originPos = Vector3.zero;
        audioSource = GetComponent<AudioSource>();

    }
    // Update is called once per frame
    void Update()
    {
        GunFireRateCalc();
        TryFire();
        TryReload();
        TryFineSight();
    }

    private void TryFineSight()
    {
        if (Input.GetButtonDown("Fire2") && !isReload)
        {
            FineSight();
        }
    }

    public void CancelFineSight()
    {
        if (isfineSightMode)
            FineSight();
    }

    private void FineSight()
    {
        isfineSightMode = !isfineSightMode;
        CurrentGun.anim.SetBool("FineSightMode", isfineSightMode);

        if (isfineSightMode)
        {
            StopAllCoroutines();
            StartCoroutine(FineSightActivateCoroutine());
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FineSightDeactivateCoroutine());
        }
    }

    IEnumerator FineSightActivateCoroutine()
    {
        while (CurrentGun.transform.localPosition != CurrentGun.fineSightOriginPos)
        {
            CurrentGun.transform.localPosition = Vector3.Lerp(CurrentGun.transform.localPosition, CurrentGun.fineSightOriginPos, 0.2f);
            yield return null;
        }
    }
    IEnumerator FineSightDeactivateCoroutine()
    {
        while (CurrentGun.transform.localPosition != originPos)
        {
            CurrentGun.transform.localPosition = Vector3.Lerp(CurrentGun.transform.localPosition, originPos, 0.2f);
            yield return null;
        }
    }

    private void TryReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReload && CurrentGun.currentBulletCount < CurrentGun.reloadBulletCount)
        {
            CancelFineSight();
            StartCoroutine(ReloadCoroutine());
        }
    }

    private void TryFire()
    {
        if (Input.GetButton("Fire1") && currentFireRate <= 0 && !isReload)
        {
            Fire();
        }
    }

    private void Fire()
    {
        if (!isReload)
        {
            if (CurrentGun.currentBulletCount > 0)
                Shoot();
            else
            {
                CancelFineSight();
                StartCoroutine(ReloadCoroutine());
            }

        }
    }

    IEnumerator ReloadCoroutine()
    {
        if (CurrentGun.carryBulletCount > 0)
        {
            isReload = true;
            CurrentGun.anim.SetTrigger("Reload");

            CurrentGun.carryBulletCount += CurrentGun.currentBulletCount;
            CurrentGun.currentBulletCount = 0;

            yield return new WaitForSeconds(CurrentGun.reloadTime);

            if (CurrentGun.carryBulletCount >= CurrentGun.reloadBulletCount)
            {
                CurrentGun.currentBulletCount = CurrentGun.reloadBulletCount;
                CurrentGun.carryBulletCount -= CurrentGun.reloadBulletCount;
            }
            else
            {
                CurrentGun.currentBulletCount = CurrentGun.carryBulletCount;
                CurrentGun.carryBulletCount = 0;
            }
            isReload = false;
        }
        else
        {
            Debug.Log("소유한 총알이 없음");
        }
    }

    private void Shoot()
    {
        CurrentGun.currentBulletCount--;
        currentFireRate = CurrentGun.fireRate; // 연사속도 재계산
        PlaySE(CurrentGun.fire_Sound);
        CurrentGun.muzzleFlash.Play();

        Hit();
        //총기 반동 코루틴
        StopAllCoroutines();
        StartCoroutine(RetroActionCoroutine());

    }

    private void Hit()
    {
        if(Physics.Raycast(theCam.transform.position, theCam.transform.forward, out hitInfo, CurrentGun.range))
        {
            GameObject clone = Instantiate(hit_effect_prefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            Destroy(clone, 2f);
        }
    }
    IEnumerator RetroActionCoroutine()
    {
        Vector3 recoilBack = new Vector3(CurrentGun.retroActionForce, originPos.y, originPos.z);
        Vector3 retroActionRecoilBack = new Vector3(CurrentGun.retroActionFineSightForce, CurrentGun.fineSightOriginPos.y, CurrentGun.fineSightOriginPos.z);

        if (!isfineSightMode)
        {
            CurrentGun.transform.localPosition = originPos;
            //반동 시작
            while(CurrentGun.transform.localPosition.x <= CurrentGun.retroActionForce - 0.02f)
            {
                CurrentGun.transform.localPosition = Vector3.Lerp(CurrentGun.transform.localPosition, recoilBack, 0.4f);
                yield return null;
            }
            while(CurrentGun.transform.localPosition != originPos)
            {
                CurrentGun.transform.localPosition = Vector3.Lerp(CurrentGun.transform.localPosition, originPos, 0.1f);
                yield return null;
            }
        }
        else
        {
            CurrentGun.transform.localPosition = CurrentGun.fineSightOriginPos;
            //반동 시작
            while (CurrentGun.transform.localPosition.x <= CurrentGun.retroActionFineSightForce - 0.02f)
            {
                CurrentGun.transform.localPosition = Vector3.Lerp(CurrentGun.transform.localPosition, retroActionRecoilBack, 0.4f);
                yield return null;
            }
            while (CurrentGun.transform.localPosition != CurrentGun.fineSightOriginPos)
            {
                CurrentGun.transform.localPosition = Vector3.Lerp(CurrentGun.transform.localPosition, CurrentGun.fineSightOriginPos, 0.1f);
                yield return null;
            }
        }
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

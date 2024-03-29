﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Shoot : Photon.MonoBehaviour, IShoot
{
    [Header("'Shoot' Reference")]
    [Space]
    [SerializeField]
    protected PhotonView myPV;
    [SerializeField]
    protected Transform startFirePoint;    //Miejsce z kąd wylatuje pocisk
    [SerializeField]
    protected Transform maxFirePoint;  //Miejsce gdzie jest maxymalny zasięg lotu pocisku 
    [SerializeField]
    protected GameObject shootSoundEffect; //Prefab odtwarzacza dzwięku wystrzału
    [SerializeField]
    protected GameObject BulletTrailPrefab; //Prefab pocisku

    [Header("'Shoot' Details")]
    [Space]
    public float shootDistance = 7.75f;
    protected float timeToFire = 0;
    public float realReloadTime;
    protected int tempMaxAmmo;
    public int TempMaxAmmo { get { return tempMaxAmmo; } }
    public bool isReloadnig = false;
    public LayerMask whatToHit;

    protected int maxAmmo;
    public abstract int MaxAmmo { get; set; }

    protected float reloadTime;
    public abstract float ReloadTime { get; set; }

    protected float reloadMagazieTime;
    public abstract float ReloadMagazieTime { get; set; }

    protected float damage;
    public abstract float Damage { get; set; }

    protected float damageLotery;
    public abstract float DamageLotery { get; set; }
    


    public virtual void Shooting()
    {
        tempMaxAmmo--;
        if(shootSoundEffect != null)
            Instantiate(shootSoundEffect, transform.position, transform.rotation);
        myPV.RPC("RpcDoShootEffect", PhotonTargets.All, startFirePoint.position, startFirePoint.rotation);
    }

    protected bool ICanShoot = false;
    public virtual void CheckShooting()
    {
        if (isReloadnig)
        {
            ICanShoot = false;
            return;
        }
        if (tempMaxAmmo <= 0)    //If you put in a whole magazine
        {
            StartCoroutine(Reload());   //start loading magazine
            StartCoroutine(ReloadEffect()); //show loading effect
            ICanShoot = false;
            return;
        }
        ICanShoot = true;
    }

    public virtual IEnumerator Reload()
    {
        isReloadnig = true;
        realReloadTime = ReloadTime - 0.1f;

        yield return new WaitForSecondsRealtime(ReloadTime);

        tempMaxAmmo = MaxAmmo;
        isReloadnig = false;
    }


    public virtual IEnumerator ReloadEffect()
    {
        for (int i = 0; i <= ReloadTime * 10; i++)
            if (realReloadTime >= 0f)
            {
                yield return new WaitForSecondsRealtime(0.1f);
                realReloadTime -= 0.1f;
                realReloadTime = Mathf.Round(realReloadTime * 100f) / 100f;
            }
    }


    protected RaycastHit2D MakeRaycastHit2D ()
    {
        Vector2 mousePosition = new Vector2(maxFirePoint.position.x, maxFirePoint.position.y);
        Vector2 firePointPosition = new Vector2(startFirePoint.position.x, startFirePoint.position.y);
        Debug.DrawLine(firePointPosition, mousePosition, Color.cyan);
        return Physics2D.Raycast(firePointPosition, mousePosition - firePointPosition, shootDistance, whatToHit);
    }

    /// <summary>
    /// Jestem lokalnym graczem i trafiam BOTa
    /// </summary>
    /// <param name="hit"></param>
    protected void HitBotHowPlayer(RaycastHit2D hit, float damage)
    {
        if (hit.collider.tag == Tag.BOT)
        {
            hit.collider.GetComponent<BOTHealt>().myPV.RPC("AdBotDamage", PhotonTargets.All, damage); //niech bot sprawdza jako czołg ma gracz
            hit.collider.GetComponent<BOTHealt>().SetLastShooter(myPV.GetComponent<PlayerGO>().myPlayer);
            hit.collider.GetComponent<BOTHealt>().SyncHP(hit.collider.GetComponent<BOTHealt>().healtPoint);
        }
    }

    /// <summary>
    /// Jestem botem i trafiam gracza lokalnego jak i zdalnego
    /// </summary>
    /// <param name="hit"></param>
    /// <param name="damage"></param>
    protected void HitPlayerHowBot(RaycastHit2D hit, float damage)
    {
        if (hit.collider.tag == Tag.LOCALPLAYERBODY || hit.collider.tag == Tag.REMOTEPLAYERBODY)
        {
            if (hit.collider.GetComponent<TankObject>().Player.hp > 0)
                GameManager.Instance.photonView.RPC("OdbierzHpGraczowiJakoBotRPC", PhotonTargets.MasterClient,
                    hit.collider.GetComponent<TankObject>().Player.pp, damage);

            SetCameraTargetOnMeBOT(hit,myPV.viewID);

            PlayHitAudioPlayer(hit);
        }
    }

    /// <summary>
    /// Jestem lokalnym graczem i trafiam zdalnego
    /// </summary>
    /// <param name="hit"></param>
    protected void HitPlayerHowPlayer(RaycastHit2D hit, float damage)
    {
        if (hit.collider.tag == Tag.REMOTEPLAYERBODY)
        {
            if (hit.collider.GetComponent<TankObject>().Player.hp > 0)
                GameManager.Instance.photonView.RPC("OdbierzHpGraczowiRPC", PhotonTargets.MasterClient, 
                    hit.collider.GetComponent<TankObject>().Player.pp, damage);

            SetCameraTargetOnMePlayer(hit);

            PlayHitAudioPlayer(hit);
        }
    }

    protected void HitPlayerHowAutoTurretPlayer(RaycastHit2D hit, float damage)
    {
        HitPlayerHowBot(hit, damage);
    }


    protected void PlayHitAudioPlayer(RaycastHit2D hit)
    {
        hit.collider.GetComponent<TankObject>().PlayerGO.GetComponent<TankRPC>().myPV.RPC("PlayAudioHitRPC",
                hit.collider.GetComponent<TankObject>().Player.pp, null);
    }

    protected void SetCameraTargetOnMePlayer(RaycastHit2D hit)
    {
        hit.collider.GetComponent<TankObject>().PlayerGO.GetComponent<TankRPC>().myPV.RPC("SetCameraDeathRPC",
                hit.collider.GetComponent<TankObject>().Player.pp, null);
    }

    protected void SetCameraTargetOnMeBOT(RaycastHit2D hit, int ID)
    {
        hit.collider.GetComponent<TankObject>().PlayerGO.GetComponent<TankRPC>().myPV.RPC("SetCameraDeathHowBotRPC",
                hit.collider.GetComponent<TankObject>().Player.pp, ID);
    }


    [PunRPC]
    protected abstract void RpcDoShootEffect(Vector3 pos, Quaternion rot, PhotonMessageInfo pmi);
}

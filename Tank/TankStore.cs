﻿using UnityEngine;

/// <summary>
/// Scritp responsoible for player progres in game. 
/// Don't turn off this script in PlayerSetup
/// </summary>
public class TankStore : Photon.MonoBehaviour
{
    [Header("Inne skrypty:")]
    public Camera2DFollow cameraFollow;
    public PlayerSetup playerSetup;
    public TriggerSth triggerSth;
 
    public GameObject stan;
    public Camera tankCamera;
    public GameObject gameOver;

    //Do śmierci gracza
    [Header("Do śmierci gracza:")]
    public GameObject camOryginalTarget;
    public GameObject camDeadTarget;
    public GameObject[] respawns;
    public GameObject[] objectToDisableOnDeath;
    public Behaviour[] componentToDisableOnDeath;

    public delegate void ShakeEnemyCamera();
    public event ShakeEnemyCamera onPlayerDead;
    public TankPeriscope tankPeriscope;



    /// <summary>
    /// Śmierć gracza (tylko lokalna i nie pełna)
    /// </summary>
    public void OnDead()
    {
        for (int i = 0; i < objectToDisableOnDeath.Length; i++)
        {
            objectToDisableOnDeath[i].SetActive(false);
        }
        for (int i = 0; i < componentToDisableOnDeath.Length; i++)
        {
            componentToDisableOnDeath[i].enabled = false;
        }
        GetComponent<TankEvolution>().TurretGameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        ShopManager.Instance.ResetUpdate();
        cameraFollow.target = camDeadTarget.transform;
        GetComponent<TankRPC>().OnDeathRPC(true);
        gameOver.SetActive(true);

        if (onPlayerDead != null)
            onPlayerDead();
    }

    /// <summary>
    /// Kontynuacja rozgrywki lokalnego gracza
    /// </summary>
    public void ContinueGame()
    {
        //Debug.Log("Kontynujuje rozgrywke");
        for (int i = 0; i < objectToDisableOnDeath.Length; i++)
        {
            objectToDisableOnDeath[i].SetActive(true);
        }
        for (int i = 0; i < componentToDisableOnDeath.Length; i++)
        {
            componentToDisableOnDeath[i].enabled = true;
        }
        GetComponent<TankEvolution>().TurretGameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        cameraFollow.target = camOryginalTarget.transform;
        UstawPozycje();
        GetComponent<TankRPC>().OnDeathRPC(false);
        gameOver.SetActive(false);
        GetComponent<TankShoot>().isReloadnig = false;
        GetComponent<TankShoot>().realReloadTime = 0f;
        GetComponent<TankShoot>().SetShootingOpportunity(true);
        GetComponent<PlayerGO>().myPlayer.hp = 600f;
        GetComponent<TankEvolution>().SetStartTank();
        HUDManager.Instance.StartRefresh();
        TankWaterCollision.Instance.Setup();
        TechTree.Instance.TankSwitchTierButton(1);
        GameManager.Instance.UpdateMyPlayer(photonView.viewID);
        //playerSetup.AktualizujMojeDane();
    }

    /// <summary>
    /// Looks for player respawns and moves the player in a random of them
    /// </summary>
    void UstawPozycje()
    {
        respawns = GameObject.FindGameObjectsWithTag("PlayerRespawn");
        int ii = Random.Range(0, respawns.Length);

        GetComponent<TankEvolution>().TankGameObject.transform.position = respawns[ii].transform.position;
    }

    public void ShakeCamera()
    {
        photonView.RPC("ShakeMyCameraRPC", GetComponent<PlayerGO>().myPlayer.pp, null);
    }

    [PunRPC]
    public void ShakeMyCameraRPC()
    {
        tankCamera.GetComponent<Shake>().CamShake();
    }
}
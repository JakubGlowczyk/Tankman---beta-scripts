﻿using UnityEngine;

public class TankRPC : Photon.MonoBehaviour
{
    public PhotonView myPV;
    public TankStore tankStore;
    public PlayerGO playerGO;

    public GameObject explosion;
    public GameObject hitSoundPrefab;

    public Material deathMat;
    public Material defaultMat;

    public SpriteRenderer body;
    public SpriteRenderer head;




    public void ZebralemScore()
    {
        GetComponent<PhotonView>().RPC("ZebralemScoreRPC",playerGO.myPlayer.pp,50);
        //GetComponent<PhotonView>().RPC("Zniszcz", PhotonTargets., null);
    }

    [PunRPC]
    void ZebralemScoreRPC(int SCORE)
    {
        GetComponent<PlayerGO>().myPlayer.score += SCORE;
    }

    public void OnDeathRPC(bool deadOrResurrection)
    {
        photonView.RPC("DeathRPC",PhotonTargets.All, deadOrResurrection);
    }

    [PunRPC]
    void DeathRPC(bool deadOrResurrection, PhotonMessageInfo pmi)
    {
        if(deadOrResurrection)
        {
            Player.FindPlayer(pmi.sender).score = Player.FindPlayer(pmi.sender).score/7;
            Instantiate(explosion, body.transform.position, body.transform.rotation);
            tankStore.stan.SetActive(false);
            body.material = deathMat;
            head.material = deathMat;
            Player.FindPlayer(pmi.sender).gameObject.GetComponent<TankEvolution>().HullGameObject.tag = Tag.STATICGAMEOBJECT;
        }
        else
        {
            tankStore.stan.SetActive(true);
            body.material = defaultMat;
            head.material = defaultMat;
            Player.FindPlayer(pmi.sender).gameObject.GetComponent<PlayerSetup>().SetTag();
        }
    }

    [PunRPC]
    void RpcAddPlayerScoreforKillBot()
    {
        GetComponent<PlayerGO>().myPlayer.score += 500;
    }

    [PunRPC]
    void SetCameraDeathRPC(PhotonMessageInfo pmi)
    {
        Debug.Log("Ustawiam u siebie CAMERE!!!!!!!!!!");
        Debug.Log(Player.FindPlayer(pmi.sender).gameObject.name);
        tankStore.camDeadTarget = Player.FindPlayer(pmi.sender).gameObject.GetComponent<TankEvolution>().HullGameObject;    //TO DO: wysłać to przez RPC
    }

    [PunRPC]
    void SetCameraDeathHowBotRPC(int ID)
    {
        Debug.Log("Ustawiam u siebie CAMERE!!!!!!!!!!");
        Debug.Log(PhotonView.Find(ID).gameObject);
        tankStore.camDeadTarget = PhotonView.Find(ID).gameObject;    //TO DO: wysłać to przez RPC
    }

    [PunRPC]
    void PlayAudioHitRPC(PhotonMessageInfo pmi)
    {
        Instantiate(hitSoundPrefab);
    }

    [PunRPC]
    public void SetCamouflage(MoroButton.Camouflage myCamouflage)
    {
        Material newMat = defaultMat;
        switch (myCamouflage)
        {
            case MoroButton.Camouflage.Default:
                newMat = defaultMat;
                break;
            case MoroButton.Camouflage.ERDL:
                newMat = ShopManager.Instance.erdlMat;
                break;
            case MoroButton.Camouflage.Marpat:
                newMat = ShopManager.Instance.marpatMat;
                break;
            case MoroButton.Camouflage.Erbsenmuster:
                newMat = ShopManager.Instance.erbseMat;
                break;
            case MoroButton.Camouflage.Puma:
                newMat = ShopManager.Instance.pumaMat;
                break;
            case MoroButton.Camouflage.Tigerstripe:
                newMat = ShopManager.Instance.tigerstripeMat;
                break;
            case MoroButton.Camouflage.DPM:
                newMat = ShopManager.Instance.dpmMat;
                break;
        }

        GetComponent<TankEvolution>().HullGameObject.GetComponent<SpriteRenderer>().material = newMat;
        GetComponent<TankEvolution>().TurretGameObject.GetComponent<SpriteRenderer>().material = newMat;
        GetComponent<TankEvolution>().TurretCapGameObject.GetComponent<SpriteRenderer>().material = newMat;
    }

    [PunRPC]
    void SetItemPositionRPC(int ID, Vector3 pos)
    {
        PhotonView.Find(ID).gameObject.transform.position = pos;
    }
}

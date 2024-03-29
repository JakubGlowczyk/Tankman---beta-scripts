﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ###################################
 * #        by Jakub Główczyk        #
 * #            [#][ ][ ]            #
 * ###################################
 */

/// <summary>
/// Ustawia gracza lokalnego, zdalnego i server(czyli gracza MasterClient)
/// </summary>
public class PlayerSetup : Photon.MonoBehaviour
{
    [SerializeField]
    Behaviour[] componentToDisable;

    [SerializeField]
    GameObject[] objectsToDisable;

    private GameObject[] itemDetectors;
    private GameObject[] tempItemDetectors;
    private PhotonView myPV;
    private bool startServerSync = true;

    [SerializeField]
    private GameObject Hull;
    [SerializeField]
    private GameObject Turret;
    [SerializeField]
    private GameObject TurretCap;


    /* 
     *  ##############################################################################
     *  #                                                                            #
     *  # Gra ruszyła... trzeba ustawić gracza lokalniego i zdalnego!                #
     *  #                                                                            #
     *  # Trzeba uzgodnić kto jest serverem i czy wykonał należące do niego zadania. #
     *  #                                                                            #
     *  # Czy gra się dopiero zaczyna czy ja tylko do niej dołączyłem?!              #
     *  #                                                                            #
     *  ##############################################################################
     */


    void Awake()
    {
        myPV = GetComponent<PhotonView>();
    }


    void Start()
    {
        //Na początek ustawię tagi graczy w swojej kopi gry. Każdy gracz w mojej kopi gry ma 
        // włączony ten skrypt więc każdy to wykona :P
        SetTag();

        if (photonView.isMine) 
        {
            SetGameScene();

            //Chej! jestem tu nowy więc rozkażę każdemu botowi aby wysłał mi swoje dane!
            UpdateBot();
        }
        else
        {
            //Jestem zdalnną kopią czołgu gracza który gra na innym kompie więc
            // niech tak pozostanie!
            DisableComponents();
        }
    }


    /// <summary>
    /// Ustawia scene roboczą dla gracza
    /// </summary>
    void SetGameScene()
    {
        GameObject Camera = GameObject.FindGameObjectWithTag("SceneCamera");
        Camera.SetActive(false);
    }

    void Update()
    {
        //Sprawdzam czy przypadkiem nie zostałem serverem ub czy nim od początku nie byłem!
        if (PhotonNetwork.isMasterClient && startServerSync && myPV.isMine)
        {
            //No trzeba się pochwalić że ma się władzę...
            Debug.Log("<color=red>ZOSTAŁEM SERVEREM!!!</color>");

            /*
             *  nie ważne jak do tego doszło ale jestem serverem ;)
             *  więc muszę ustawić całą rozgrywkę, przechowywać dane itp
             */

            //Niech każdy gracz zdalny ma włączony detektor itemów
            StartCoroutine(SetOtherPlayerColliderScore());

            //To ja widzę czy gracz zebrał item czy nie więc każę innym to synchronizować
            StartCoroutine(UpdateScorePlayers());
            itemDetectors = GameObject.FindGameObjectsWithTag("ItemDetector");
            for (int i = 0; i < itemDetectors.Length; i++)
            {
                itemDetectors[i].GetComponent<ItemDetector>().enabled = true;
            }
            tempItemDetectors = itemDetectors;

            //BOTy są lokalne tylko i wyłącznie na serverze więc niech takie będą
            GameObject[] bots = GameObject.FindGameObjectsWithTag("BOTID");
            for (int ii = 0; ii < bots.Length; ii++)
            {
                if (bots != null)
                    bots[ii].GetComponent<BOTSetup>().SetBotComponents();
            }

            startServerSync = false;
        }
    }



    /// <summary>
    /// Ustawia tag TEGO gracza a dokładnie tag GameObject'u gdzie jest collider czołgu -
    ///  czy jest to "LocalBody" czy "RemoteBody" 
    /// </summary>
    public void SetTag()
    {
        if(photonView.isMine)
        {
            gameObject.tag = Tag.LOCALPLAYER;
            Hull.gameObject.tag = Tag.LOCALPLAYERBODY;
            Hull.gameObject.layer = 10;
        }
        else
        {
            gameObject.tag = Tag.REMOTEPLAYER;
            Hull.gameObject.tag = Tag.REMOTEPLAYERBODY;
            Hull.gameObject.layer = 11;
        }
    }

    /// <summary>
    /// Co sekundę dba o to aby każdy zdalny gracz (w tej kopi gry) miał
    ///  włączony detektor "serverowy" który wysyła dane detektora do lokalnych graczy przez RPC
    /// </summary>
    /// <returns></returns>
    IEnumerator SetOtherPlayerColliderScore()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1);
            if (tempItemDetectors.Length != itemDetectors.Length)
            {
                Debug.Log("Rozpoczynam Włączanie colliderów czołgu u master Clienta! bo dołączył nowy gracz!!!");
                itemDetectors = GameObject.FindGameObjectsWithTag("ItemDetector");
                for (int i = 0; i < itemDetectors.Length; i++)
                {
                    if (itemDetectors != null)
                    {
                        itemDetectors[i].GetComponent<ItemDetector>().enabled = true;
                    }
                }
                itemDetectors = GameObject.FindGameObjectsWithTag("ItemDetector");
            }
            tempItemDetectors = GameObject.FindGameObjectsWithTag("ItemDetector");
        }
    }


    /// <summary>
    /// Co sekundę pobiera od każdego zdalnego gracza (w tej kopii gry)
    ///  jego itemy i wysyła je odpowiedni do lokaknych graczy przez RPC
    ///  itemy: score, coin, dynamit, naprawiarka, zasoby
    /// </summary>
    /// <returns></returns>
    IEnumerator UpdateScorePlayers()
    {
        while (true)
        {
            //Debug.Log("UpdateScorePlayers");
            yield return new WaitForSecondsRealtime(1f);

            GameObject[] players = GameObject.FindGameObjectsWithTag("RemotePlayer");
            for (int i = 0; i < players.Length; i++)
            {
                if (players.Length > 0)
                {
                    if (players[i].GetComponent<PlayerGO>().myPlayer == null)
                        continue;

                    Player player = players[i].GetComponent<PlayerGO>().myPlayer;
                    players[i].GetComponent<PlayerSetup>().UpdateScore
                        (
                            player.score,
                            player.coin,
                            player.Dynamit,
                            player.Naprawiarka,
                            player.Zasoby
                        );
                }
            }
        }
    }

    private static void UpdateBot()
    {
        GameObject[] bots = GameObject.FindGameObjectsWithTag("BOTID");
        for (int i = 0; i < bots.Length; i++)
        {
            if (bots.Length > 0)
                bots[i].GetComponent<BOTSetup>().Hull.GetComponent<BOTHealt>().DownloadHP();
        }
    }

    /* 
     *  ######################################################################################################
     *  #                                                                                                    #
     *  # Serwer przechowuje prawdziwe score gracza więc ta funkcja wykonuje się                             #
     *  # TYLKO w lokalnej kopi gry servera, on każe ustawić każdemu graczu w swojej kopii gry (co sekunde)  #
     *  # score które server przechowywuje po to aby kiedy serwer opuści grę to serwerem zostanie inny gracz #
     *  # i ten inny gracz będzie miał od razu score wszystkich innych graczy ustawione w swojej kopi gry.   #
     *  #                                                                                                    #
     *  ######################################################################################################
     */

    void UpdateScore(int SCORE, int COIN, int DYNAMIT, int NAPRAWIARKA, int ZASOBY)
    {
        //Każdy inny gracz w tej kopii gry(kopii masterClienta)
        //wysyła swoje score(czyli score które przechwuje masterClient)
        //swoim sobowtórom we wszystkich innych kopiach gry
        myPV.RPC("RpcUpdateScore", PhotonTargets.All,SCORE, COIN, DYNAMIT, NAPRAWIARKA, ZASOBY);
    }

    [PunRPC]
    void RpcUpdateScore(int SCORE, int COIN, int DYNAMIT, int NAPRAWIARKA, int ZASOBY)
    {
        if (GetComponent<PlayerGO>().myPlayer == null)
            return;

        //Każdy gracz w swojej kopi gry ustawia sobie
        //SCORE które wysłał mu server
        GetComponent<PlayerGO>().myPlayer.score = SCORE;
        GetComponent<PlayerGO>().myPlayer.coin = COIN;
        GetComponent<PlayerGO>().myPlayer.Dynamit = DYNAMIT;
        GetComponent<PlayerGO>().myPlayer.Naprawiarka = NAPRAWIARKA;
        GetComponent<PlayerGO>().myPlayer.Zasoby = ZASOBY;

    }

    public void DisableComponents()
    {
        for (int i = 0; i < componentToDisable.Length; i++)
        {
            componentToDisable[i].enabled = false;
        }

        for (int i = 0; i < objectsToDisable.Length; i++)
        {
            objectsToDisable[i].SetActive(false);
        }
    }
}

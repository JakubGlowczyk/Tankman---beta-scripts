﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * ###################################
 * #        by Jakub Główczyk        #
 * #            [#][#][ ]            #
 * ###################################
 */

/// <summary>
/// ConnectionManager służy do połączenia się gracza z grą, poinformowania go
/// gdzie jest w grze, ile ma FPS'ów, PING'u
/// </summary>
public class ConnectionManager : Photon.MonoBehaviour
{
    private GameManager gameManager;
    private static short roomIndex = 0;

    /// <summary>
    /// Dołącz do gry, tylko dołącz
    /// </summary>
    public void Connect()
    {
        PhotonNetwork.sendRate = 30;
        PhotonNetwork.sendRateOnSerialize = 30;
        PhotonNetwork.ConnectUsingSettings(GameManager.gameVersion);
    }

    /// <summary>
    /// Jeśli stworzysz pokój (czyli jesteś serverem) to w scenie roboczej
    /// Spawnisz mape i boty dla wszystkich graczy 
    /// oraz w tym mejscu informujesz przyszłych graczy że dołączyłeś
    /// </summary>
    void OnCreatedRoom()
    {
        GameManager.InstantianeSceneObject();
        photonView.RPC("InitPlayer", PhotonTargets.All, PhotonNetwork.player); //Tak naprawdę wysyłasz to tylko sobie
    }

    /// <summary>
    /// Kiedy stracisz połączenie z grą (możliwe, że dobrowolne)
    /// </summary>
    void OnDisconnectedFromPhoton()
    {
        Debug.Log("Gracz zerwał połączene");
        //Player.players.Clear();
        SceneManager.LoadScene(1);
    }

    #region GUI w lewym górnym rogu (FPS, PING, ActualGameStat)

    //zmienne dla GUI
    private string stan;
    private string PING;
    private string FPStext;
    private string serverText;
    private float deltaTime = 0.0f;

    //Stan gry w lewym górnym rogu - PING FPC ROOMSTAT
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(5, 5, 200, 200), FPStext, style);
        GUI.Label(new Rect(65, 5, 200, 200), PING, style);
        GUI.Label(new Rect(140, 5, 200, 200), stan, style);
        GUI.Label(new Rect(215, 5, 200, 200), serverText, style);
    }


    void Start()
    {
        gameManager = GetComponent<GameManager>();
        StartCoroutine(FPSRefreez());
    }

    void Update()
    {
        //Game State
        //If Player join to the room to instead writing "Joined" write:
        if (PhotonNetwork.connectionStateDetailed.ToString() != "Joined")
        {
            stan = PhotonNetwork.connectionStateDetailed.ToString();
        }
        else
        {
            stan = "Online: " + PhotonNetwork.room.PlayerCount;
        }
        if (PhotonNetwork.isMasterClient)
            serverText = "<color=red>Server permissions!</color>";
        else
            serverText = "";
        if (Input.GetKeyDown(KeyCode.L))
            Player.DebugListyGraczy();

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        int ping = PhotonNetwork.networkingPeer.RoundTripTime;
        PING = "PING: " + ping.ToString();
    }


    IEnumerator FPSRefreez()
    {
        while(true)
        {
            float msec = deltaTime * 1000.0f;
            float FPS = 1.0f / deltaTime;
            FPStext = string.Format("FPS: {1:0.}", msec, FPS);
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    #endregion  

    #region Proces ładowania sceny roboczej (kiedy dołączysz do gry)

    /// <summary>
    /// Jeśli dołączysz do gry to odrazu dołączysz do lobby (jeśli jest włączone Auto joined lobby)
    /// to wtedy to się wykona czyli zaczniesz ładować scenę roboczą gry
    /// </summary>
    void OnJoinedLobby()
    {
        SceneManager.LoadSceneAsync("GameScene");
    }

    void OnEnable()
    {
        //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    #endregion

    #region Proces dołączania do właściwego pokoju (po załadowaniu sceny roboczej)

    /*
     * Nowy gracz chce dołączyć do jakiegokolwiek pokoju
     * mamy dwa człony pokoju - GameMode + Index np: FFA_0 lub BLITZKRIEG_4...
     * gracz już wybrał tryb gry (FFA, BLITZKRIEG...) i automatycznie prubuje dołączyć do pokoju 0
     * czyli do FFA_0
     * Jeśli mu się nie uda (bo pokój jest pełny lub nie istnieje (jeśli nie istnieje to próbuje go stworzyć
     * a jeśli nie uda się stworzyć to znaczy że istnieje i zapewne jest pełny)) to dołączyć do FFA_1
     * i koło się zamyka
     */

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != 1 && scene.buildIndex != 0)
        {
            //Debug.Log("Prubuję dołączyć do pokoju: "+ gameManager.gameMode + "_" + roomIndex);
            PhotonNetwork.JoinOrCreateRoom(GameManager.myMode.ToString() + "_" + roomIndex, roomOptions: new RoomOptions() /*{ maxPlayers = 1 }*/, typedLobby: TypedLobby.Default);
            //PhotonNetwork.JoinOrCreateRoom(gameManager.gameMode, new RoomOptions(), TypedLobby.Default);
        }
    }

    void OnPhotonCreateRoomFailed()
    {
        //Debug.Log("nie udało się stworzyć pokoju: " + gameManager.gameMode + "_" + roomIndex);
        roomIndex++;
        //Debug.Log("Prubuję dołączyć do pokoju: " + gameManager.gameMode + "_" + roomIndex);
        PhotonNetwork.JoinOrCreateRoom(GameManager.myMode.ToString() + "_" + roomIndex, roomOptions: new RoomOptions() /*{ maxPlayers = 1 }*/, typedLobby: TypedLobby.Default);
    }

    #endregion

    #region Rejestrowanie (dodawanie i usuwanie) graczy

    /// <summary>
    /// Jeśli nowy gracz dołączy do gry to to wykonują wszyscy gracze prócz niego samego
    /// (serwer tego nie wykona bo sam jest w pokoju (prucz niego samego...))
    /// </summary>
    /// <param name="NewPlayerPP">PhotonPlayer nowego gracza</param>
    void OnPhotonPlayerConnected(PhotonPlayer NewPlayerPP)
    {
        //Aha! tylko server niech o tym informuje innych
        if (PhotonNetwork.isMasterClient)
        {
            //###
            //Tu się zaczyna cała przygoda nowego gracza
            //###

            //przywitajcie gracza wielkimi brawami
            photonView.RPC("InitPlayer", PhotonTargets.All, NewPlayerPP);
        }
    }

    /// <summary>
    /// Jeśli gracz opuści grę to to wykonują wszyscy gracze prócz niego samego
    /// (skoro opuszcza grę to ki *uj go to obchodzi)     
    /// </summary>
    /// <param name="OldPlayerPP">PhotonPlayer starego gracza</param>
    void OnPhotonPlayerDisconnected(PhotonPlayer OldPlayerPP)
    {
        //Każdy indywidualnie usuwa go ze swojej listy graczy 
        var tmpPlayer = Player.FindPlayer(OldPlayerPP);
        if (tmpPlayer != null)
        {
            Player.players.Remove(tmpPlayer);
        }
    }

    /// <summary>
    /// Gracze!, jakiś gracz chce dołączyć więc go powitajmy!
    /// dodajmy go do listy graczy a resztę niech on nam powie
    /// </summary>
    /// <param name="pp">newPlayer</param>
    /// <param name="pmi">default</param>
    [PunRPC]
    void InitPlayer(PhotonPlayer pp,PhotonMessageInfo pmi)
    {
        //No elo wszystkim, powiedzcie soś o sobie.
        if (pp == PhotonNetwork.player)
            gameManager.SetRemotePlayer();

        //Cześć graczu! każdy z nas doda Cię do listy graczy, tak jak i ty sam!
        Player player = new Player();
        Player.players.Add(player);
        player.nick = pp.NickName;
        player.pp = pp;

        //Dobra dzięki, jeszcze tylko się zespawnie i powiem wam wszystkim gdzie 
        if (pp == PhotonNetwork.player)
            gameManager.SpawnPlayer();
    }

    #endregion
}
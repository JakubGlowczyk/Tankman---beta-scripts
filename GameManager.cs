using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * ###################################
 * #        by Jakub Główczyk        #
 * #            [#][#][ ]            #
 * ###################################
 */

/// <summary>
/// He is responsible for the overall operation of the game
/// </summary>
public class GameManager : Photon.MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    #endregion

    public static Player LocalPlayer
    {
        get
        {
            if (Player.FindPlayer(PhotonNetwork.player) != null)
                return Player.FindPlayer(PhotonNetwork.player);
            else
            {
                Debug.LogError("Don't find localPlayer! Mayby he isn't on scene?!");
                return null;
            }
        }
    }

    [SerializeField]
    private TanksData tanksData;   //only setup static list 
    public static ModeManager.Mode myMode;
    public static NationManager.Nation myNation = NationManager.Nation.IIIRZESZA;
    public const string gameVersion = "2.1";  //You need change when you add new things for game
    public static string biomName;     //the name of the biomass that the server has drawn

    //Nazwy prefabów biomów w Resources //TODO: Zrobić enum
    static string[] biomsPrefab = new string[] { "PIXEL", "PIXELo" };
    //liczby botów
    static int howMuchO_I = 0;
    static int howMuchSTRV = 0;
    static int howMuchAMX = 0;
    //Numer każdego bota (potrzebny do uniknięcia 
    //problemu ze spawnem 2 botów w tym samym miejscu)
    static int OI_ID = 0;
    static int AMX_ID = 0;

    //Granice do wbicia tieru 
    public const int One_level = 1000;
    public const int Two_level = 3000;
    public const int Tree_level = 6000;



    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            tanksData.Setup();
            SceneManager.LoadScene(1);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Jako nowy gracz spawnie się i każę reszcie połączyć sobie gracza z listy z tym spawnem
    /// </summary>
    public void SpawnPlayer()
    {
        GameObject[] respawns = GameObject.FindGameObjectsWithTag("PlayerRespawn");
        int ii = Random.Range(0, respawns.Length);

        GameObject myPlayerGO = PhotonNetwork.Instantiate("Player", respawns[ii].transform.position, respawns[ii].transform.rotation, 0);

        UpdateMyPlayer(myPlayerGO.GetComponent<PhotonView>().viewID);
    }

    public void UpdateMyPlayer(int viewID)
    {
        photonView.RPC("SendPlayerRPC", PhotonTargets.All,
            LocalPlayer.pp,
            viewID,
            NationManager.myNation,
            TanksData.FindTankData(NationManager.ReturnStartTank(NationManager.myNation)).maxHp,
            NationManager.ReturnStartTank(NationManager.myNation),
            true); //Player gracza wysyłamy jeśli target ma go na liście graczy
    }

    /// <summary>
    /// Wykonuje ją tylko i wyłącznie MasterClient,
    /// server spawni losową mapę dla wszystkich, spawni FoodSpawner
    /// oraz spawni boty zależnie od wcześniej zespawnowanego biomu
    /// </summary>
    public static void InstantianeSceneObject()
    {
        if (PhotonNetwork.isMasterClient)
        {
            int loteryBiom = Random.Range(0, biomsPrefab.Length);
            loteryBiom = 0; //do testów
            Instance.GetComponent<PhotonView>().RPC("UstawAktualnyBiomWszystkim", PhotonTargets.AllBuffered, biomsPrefab[loteryBiom]);
            PhotonNetwork.Instantiate("Food_Spawner", new Vector3(0, 0, 0), Quaternion.identity, 0, null);

            switch (biomsPrefab[loteryBiom])
            {
                case "PIXEL":
                    howMuchAMX = 0;
                    howMuchO_I = 1;
                    howMuchSTRV = 0;
                    break;
                case "PIXELo":
                    howMuchAMX = 0;
                    howMuchO_I = 1;
                    howMuchSTRV = 0;
                    break;
            }
            PhotonNetwork.InstantiateSceneObject(biomsPrefab[loteryBiom], new Vector3(0, 0, 0), Quaternion.identity, 0, null);

            for (int i = 0; i < howMuchO_I; i++)
            {
                GameObject OI = PhotonNetwork.InstantiateSceneObject("BOT_Tiger", new Vector3(0, 0, 0), Quaternion.identity, 0, null);
                Instance.photonView.RPC("SetBotIDRPC", PhotonTargets.AllBuffered, OI.GetComponent<PhotonView>().viewID, OI_ID);
                OI_ID++;
            }

            for (int i = 0; i < howMuchSTRV; i++)
            {
                PhotonNetwork.InstantiateSceneObject("BOT_PZI", new Vector3(0, 0, 0f), Quaternion.identity, 0, null);
            }

            for (int i = 0; i < howMuchAMX; i++)
            {
                GameObject AMX = PhotonNetwork.InstantiateSceneObject("BOT_PZI", new Vector3(0, 0, 0f), Quaternion.identity, 0, null);
                AMX.GetComponent<BOTSetup>().ID = AMX_ID;
                AMX_ID++;
            }
        }
    }

    /// <summary>
    /// Metoda którą wykonuje server. spawnuje podany obiekt(nazwaObiektu) w podanej pozycji i rotacji.
    /// Każdy gracz może poprosić server aby zespawnił obiekt, serwer sprawdza czy tak może być 
    /// i spawni go u wszystkich graczy
    /// </summary>
    /// <param name="nazwaObiektu">Nazwa prefabu, który znajduje się w Resources. On będzie spawniony w grze</param>
    /// <param name="pos">pozycja w której chcesz zespawnić obiekt</param>
    /// <param name="rot">chyba wiadomo ;)</param>
    [PunRPC]
    void SpawnSceneObjectRPC(string nazwaObiektu, Vector3 pos, Quaternion rot, PhotonMessageInfo pmi)
    {
        if (!PhotonNetwork.isMasterClient)  //jeśli nie jestem serwerem
            return;

        if (Player.FindPlayer(pmi.sender).Zasoby <= 0)  //Jeśli gracz który chce wstawiać nie ma narzędzi
            return;

        PhotonNetwork.InstantiateSceneObject(nazwaObiektu, pos, rot, 0, null);
    }

    [PunRPC]
    void SetBotIDRPC(int photonViewID, int botID)
    {
        PhotonView.Find(photonViewID).gameObject.GetComponent<BOTSetup>().ID = botID;
    }

    [PunRPC]
    void SpawnujGraczaRPC(int pvID, PhotonMessageInfo pmi)
    {
        GameObject newPlayerGO = PhotonView.Find(pvID).gameObject;
        Player player;

        if (Player.FindPlayer(pmi.sender) == null)
        {
            Debug.LogError("Błąd krytyczny!");
            return;
        }
        else
        {
            player = Player.FindPlayer(pmi.sender);
        }

        player.gameObject = newPlayerGO;    //Tu jest problem
        newPlayerGO.GetComponent<PlayerGO>().myPlayer = player;
        newPlayerGO.GetComponent<PlayerGO>().myPlayer.nation = myNation;
        newPlayerGO.name = "Player_" + player.nick; //+nick
    }

    [PunRPC]
    void UstawAktualnyBiomWszystkim(string BIOMNAME)
    {
        biomName = BIOMNAME;
    }

    #region Mordowanie gracza (lub tyklo bicie ;)

    [PunRPC]
    void OdbierzHpGraczowiRPC(PhotonPlayer ofiaraPP,float currentDamage, PhotonMessageInfo pmi)
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        float DAMAGE = TanksData.FindTankData(Player.FindPlayer(pmi.sender).tank).damage;
        float DAMAGELOTERY = TanksData.FindTankData(Player.FindPlayer(pmi.sender).tank).damageLotery;
        float tempDamage = Mathf.Round(Random.Range(DAMAGE - DAMAGELOTERY, DAMAGE + DAMAGELOTERY));

        if (Player.FindPlayer(pmi.sender).tank == DostempneCzolgi.O_I ||
            Player.FindPlayer(pmi.sender).tank == DostempneCzolgi.IS7)
            tempDamage = currentDamage;

        Player ofiara = Player.FindPlayer(ofiaraPP);
        if (ofiara.hp <= tempDamage)
        {
            GetComponent<PhotonView>().RPC("ZabiJOfiareRPC", ofiaraPP, ofiaraPP);
            int reward = TanksData.FindTankData(Player.FindPlayer(pmi.sender).gameObject.GetComponent<PlayerGO>().myPlayer.tank).level * 200;
            Player.FindPlayer(pmi.sender).gameObject.GetComponent<PlayerGO>().myPlayer.score += reward;
        }
        else
        {
            GetComponent<PhotonView>().RPC("OdbierzHpOfiaraRPC", PhotonTargets.All, ofiaraPP, tempDamage);
        }
    }

    [PunRPC]
    void OdbierzHpGraczowiJakoBotRPC(PhotonPlayer ofiaraPP, float DAMAGE, PhotonMessageInfo pmi)
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        Player ofiara = Player.FindPlayer(ofiaraPP);

        if (ofiara.hp <= DAMAGE)
            GetComponent<PhotonView>().RPC("ZabiJOfiareRPC", ofiaraPP, ofiaraPP);
        else
            GetComponent<PhotonView>().RPC("OdbierzHpOfiaraRPC", PhotonTargets.All, ofiaraPP, DAMAGE);
    }

    [PunRPC]
    void OdbierzHpOfiaraRPC(PhotonPlayer ofiaraPP, float damage)
    {
        //Debug.Log("Gracz " + Player.FindPlayer(ofiaraPP).nick + " stracił " + damage + " punktów!");
        Player.FindPlayer(ofiaraPP).hp -= damage;
    }

    [PunRPC]
    void ZabiJOfiareRPC(PhotonPlayer ofiaraPP)
    {
        Player.FindPlayer(ofiaraPP).gameObject.GetComponent<TankStore>().OnDead();
    }

    #endregion

    /// <summary>
    /// Ustawia wszystkich graczy zdalnych w mojej kopii gry
    /// (tylko to co potrzebuje zwykły szary gracz)
    /// </summary>
    public void SetRemotePlayer()
    {
    #region Proces proszenia server o dane, ustawianie ich dla gracza proszącego
        if (!PhotonNetwork.isMasterClient)
            photonView.RPC("ServerSetPlayersForMeRPC", PhotonTargets.MasterClient, null);
    }

    /// <summary>
    /// Wysyłam (ja server) dane wszystkich graczy proszącemu o to 
    /// </summary>
    /// <param name="pmi"></param>
    [PunRPC]
    void ServerSetPlayersForMeRPC(PhotonMessageInfo pmi)
    {
        if (!PhotonNetwork.isMasterClient)
            return;
        for (int i = 0; i < Player.players.Count; i++)
        {
            if (Player.players[i].pp != pmi.sender)
                photonView.RPC("SendPlayerRPC", pmi.sender, 
                    Player.players[i].pp, 
                    Player.players[i].gameObject.GetComponent<PhotonView>().viewID, 
                    Player.players[i].nation,
                    Player.players[i].gameObject.GetComponent<PlayerGO>().myPlayer.hp,
                    Player.players[i].gameObject.GetComponent<PlayerGO>().myPlayer.tank, 
                    false); //true wysyłamy jeśli target ma gracza go na liście graczy
        }
    }

    /// <summary>
    /// To wykonuje gracz proszący o dane reszty graczy,
    ///  ustawia podstawowe dane o tych graczach
    /// </summary>
    /// <param name="remotePlayerPP"></param>
    /// <param name="remotePlayerIndex"></param>
    /// <param name="remotePlayerID"></param>
    /// <param name="remoteNation"></param>
    /// <param name="remoteHP"></param>
    /// <param name="remoteTank"></param>
    [PunRPC]
    void SendPlayerRPC(PhotonPlayer remotePlayerPP, int remotePlayerID, NationManager.Nation remoteNation,
                        float remoteHP, DostempneCzolgi remoteTank, bool remotePlayer = false)
    {
        //Tworzę gracza i dodaje go do mojej listy graczy 
        Player player = new Player();
        if (remotePlayer)
        {
            player = Player.FindPlayer(remotePlayerPP);
        }
        else
            Player.players.Add(player);

        //Ustawiam podstawowe dane tego gracza 
        player.nick = remotePlayerPP.NickName;
        player.pp = remotePlayerPP;
        player.nation = remoteNation;
        player.hp = remoteHP;
        player.tank = remoteTank;

        //Ustawiam odwołanie gracza z listy i właściwego obiektu
        GameObject newPlayerGO = PhotonView.Find(remotePlayerID).gameObject;
        player.gameObject = newPlayerGO;    //Tu jest problem
        newPlayerGO.GetComponent<PlayerGO>().myPlayer = player;

        //Taki bajer
        newPlayerGO.name = "Player_" + player.nick; //+nick

        //Ustawiam dane widoczne dla gracza proszącego (ustawiam czołg, sliderHP i nick) 
        newPlayerGO.GetComponent<PlayerGO>().myPlayer.hp = player.hp;
        newPlayerGO.GetComponent<PlayerGO>().myPlayer.nick = player.nick;
        newPlayerGO.GetComponent<Nick>().nick.text = player.nick;
        newPlayerGO.GetComponent<PlayerGO>().myPlayer.nation = player.nation;
        newPlayerGO.GetComponent<PlayerGO>().myPlayer.tank = player.tank;
        newPlayerGO.GetComponent<TankEvolution>().SetStartTankHowNewPlayer(player.tank);
    }
    #endregion 
}

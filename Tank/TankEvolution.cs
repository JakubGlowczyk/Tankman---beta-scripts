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
/// Odpowiada za ewolucje gracza, jego "level up". Zawiera także odniesienia do podzespołów czołgu,
/// wszystkie zmienne czołgu (speed, reload, HP) do których odwołują się inne skrypty.  
/// </summary>
public class TankEvolution : Photon.MonoBehaviour
{
    //Jeśli chodzi o gracza zdalnego nie wolno korzystać z Singletonu!
    public static TankEvolution Instance { get; private set; }

    [Header("Reference:")]
    [Space]
    public ShadowEffect myShadow;


    [Header("Tank public components:")]
    [Space]
    [SerializeField]
    private GameObject tank;
    public GameObject TankGameObject { get { return tank;} }

    [SerializeField]
    private GameObject hull;
    public GameObject HullGameObject { get { return hull; } }

    [SerializeField]
    private GameObject turret;
    public GameObject TurretGameObject { get { return turret; } }

    [SerializeField]
    private GameObject turretCap;
    public GameObject TurretCapGameObject { get { return turretCap; } }

    [SerializeField]
    private GameObject barrelEndPoint;
    public GameObject BarrelEndPoint { get { return barrelEndPoint; } }

    [Header("Tank private components:")]
    [Space]
    [SerializeField]
    private GameObject turretKeep;
    [SerializeField]
    private GameObject[] turretsOI;
    [SerializeField]
    private GameObject turretIS;
    [SerializeField]
    private BoxCollider2D[] tankColliders;

    [Header("Start tank Buttons:")]
    [Space]
    [SerializeField]
    private TankButton PZIGOButton;
    [SerializeField]
    private TankButton T38Button;
    [SerializeField]
    private TankButton KHA_GOButton;

    public TankData PZITank;
    public TankData KHAGOTank;
    public TankData T38Tank;


    #region TankParametrs

    [Header("Tank Details")]
    [Space]
    [SerializeField]
    private float maxHp;
    public float MaxHp { get { return maxHp; } }

    [SerializeField]
    private float reload;
    public float Reload { get { return reload; } }

    [SerializeField]
    private float damage;
    public float Damage { get { return damage; } }

    [SerializeField]
    private float speed;
    public float Speed { get { return speed; } }

    [SerializeField]
    private float turnSpeed;
    public float TurnSpeed { get { return turnSpeed; } }

    [SerializeField]
    private float headTurnSpeed;
    public float HeadTurnSpeed { get { return headTurnSpeed; } }

    [SerializeField]
    private int magazynek;
    public int Magazynek { get { return magazynek; } }

    [SerializeField]
    private float damageLotery;
    public float DamageLotery { get { return damageLotery; } }

    [SerializeField]
    private float reloadBetweenMagazine;
    public float ReloadBetweenMagazine { get { return reloadBetweenMagazine; } }

    [SerializeField]
    private float cameraOrthographicSize;
    public float CameraOrthographicSize { get { return cameraOrthographicSize; } }

    #endregion

    #region TankTexture

    [Header("Tanks texture:")]
    [Space]
    //PZI
    [SerializeField] public Sprite PZIHullTexture;
    [SerializeField] public Sprite PZITurretTexture;

    //PZIV
    [SerializeField] public Sprite PZIVHullTexture;
    [SerializeField] public Sprite PZIVTurretTexture;

    //PZVI
    [SerializeField] public Sprite PZVIHullTexture;
    [SerializeField] public Sprite PZVITurretTexture;

    //PZVIII
    [SerializeField] public Sprite PZVIIIHullTexture;
    [SerializeField] public Sprite PZVIIITurretTexture;

    //STUGIV
    [SerializeField] public Sprite STUGIVHullTexture;
    [SerializeField] public Sprite STUGIVTurretTexture;
    [SerializeField] public Sprite STUGIVTurretCapTexture;

    //JAGDTIGER
    [SerializeField] public Sprite JAGDTIGERHullTexture;
    [SerializeField] public Sprite JAGDTIGERTurretTexture;
    [SerializeField] public Sprite JAGDTIGERTurretCapTexture;

    //T38
    [SerializeField] public Sprite T38HullTexture;
    [SerializeField] public Sprite T38TurretTexture;

    //T3475
    [SerializeField] public Sprite T3475HullTexture;
    [SerializeField] public Sprite T3475TurretTexture;

    //IS2
    [SerializeField] public Sprite IS2HullTexture;
    [SerializeField] public Sprite IS2TurretTexture;

    //IS7
    [SerializeField] public Sprite IS7HullTexture;
    [SerializeField] public Sprite IS7TurretTexture;

    //SU76
    [SerializeField] public Sprite SU76HullTexture;
    [SerializeField] public Sprite SU76TurretTexture;
    [SerializeField] public Sprite SU76TurretCapTexture;

    //SU85
    [SerializeField] public Sprite SU85HullTexture;
    [SerializeField] public Sprite SU85TurretTexture;
    [SerializeField] public Sprite SU85TurretCapTexture;


    //KHA-GO
    [SerializeField] public Sprite KHA_GOHullTexture;
    [SerializeField] public Sprite KHA_GOTurretTexture;

    //HO-NI
    [SerializeField] public Sprite HO_NIHullTexture;
    [SerializeField] public Sprite HO_NITurretTexture;

    //HO-NI_III
    [SerializeField] public Sprite HO_NI_IIIHullTexture;
    [SerializeField] public Sprite HO_NI_IIITurretTexture;

    //HO-RO
    [SerializeField] public Sprite HO_ROHullTexture;
    [SerializeField] public Sprite HO_ROTurretTexture;

    //HA-TO_300MM
    [SerializeField] public Sprite HA_TO_300MMHullTexture;

    //SINKHOTO
    [SerializeField] public Sprite SINKHOTOHullTexture;
    [SerializeField] public Sprite SINKHOTOTurretTexture;

    //HT-NO_VI
    [SerializeField] public Sprite HT_NO_VIHullTexture;
    [SerializeField] public Sprite HT_NO_VITurretTexture;

    //O-I
    [SerializeField] public Sprite O_IHullTexture;
    [SerializeField] public Sprite O_ITurretTexture;

    //TYPE_I_CHI-KHE
    [SerializeField] public Sprite TYPE_I_CHI_KHEHullTexture;
    [SerializeField] public Sprite TYPE_I_CHI_KHETurretTexture;

    //TYPE_III_HI-NU
    [SerializeField] public Sprite TYPE_III_HI_NUHullTexture;
    [SerializeField] public Sprite TYPE_III_HI_NUTurretTexture;

    #endregion

    public void Awake()
    {
        if (!photonView.isMine)
            enabled = false;
        else if (Instance == null)
            Instance = this;
        else
            enabled = false;
    }

    void Start()
    {
        SetStartTank();
    }

    public void Update()
    {
        /// Dodaje score, coin itp.
        if (Input.GetKeyDown(KeyCode.P))
        {
            GetComponent<PlayerGO>().myPlayer.score += 500;
            GetComponent<PlayerGO>().myPlayer.Dynamit += 1;
            GetComponent<PlayerGO>().myPlayer.Naprawiarka += 1;
            GetComponent<PlayerGO>().myPlayer.Zasoby += 1;
            GetComponent<PlayerGO>().myPlayer.coin += 10;
        }
        /// Debuguje liste graczy i ich zmienne
        if (Input.GetKeyDown(KeyCode.O))
        {
            for (int i = 0; i < Player.players.Count; i++)
            {
                string text = "<color=red>" + Player.players[i].nick + "</color>   " + "~" + Player.players[i].nation + "~\n" +
                "tank: " + Player.players[i].tank + "\n" +
                "score: " + Player.players[i].score + "\n" +
                "zasoby: " + Player.players[i].Zasoby + "\n" +
                "PhotonPlayer: " + Player.players[i].pp + "\n"; //+
                //"ID: " + Player.players[i].gameObject.GetComponent<PhotonView>().viewID;

                Debug.Log(text);
            }
        }
        /// Zrywa połączenie z grą
        if (Input.GetKeyDown(KeyCode.I))
        {
            PhotonNetwork.Disconnect();
        }
    }


    /// <summary>
    /// Ustawia początkowy czołg w zależności od wybranej nacji
    /// </summary>
    public void SetStartTank()
    {
        switch (GetComponent<PlayerGO>().myPlayer.nation)
        {
            case NationManager.Nation.ZSRR:
                T38Button.OnClick();
                SetTankObject(T38Tank);
                break;

            case NationManager.Nation.IIIRZESZA:
                PZIGOButton.OnClick();
                SetTankObject(PZITank);
                break;

            case NationManager.Nation.JAPONIA:
                KHA_GOButton.OnClick();
                SetTankObject(KHAGOTank);
                break;
        }
    }


    void SetTankObject(TankData myTank)
    {
        //Ustawiam parametry czołgu
        SetTankParametrs(myTank);

        //Ustawiam textury kadłuba, wieży i (jeśli jest) przykrycia lufy 
        SetTankTexture(myTank.hullTexture, myTank.turretTexture, myTank.turretCapTexture);

        //Ustawiam pozycję wieży
        SetGameObjectPosition(myTank.turretPos.x, myTank.turretPos.y, turretKeep);

        //Ustawiam pozycję przykrycia lufy
        SetGameObjectPosition(myTank.turretCapPos.x, myTank.turretCapPos.y, turretCap);

        //Ustawiam pozycję wylotu lufy
        SetGameObjectPosition(myTank.barrlelEndPos.x, myTank.barrlelEndPos.y, barrelEndPoint);

        //Ustawiam collider czołgu oraz collider zbierania score
        ResetPolygon(myTank.offset.x, myTank.offset.y, myTank.size.x, myTank.size.y);

        //Jeśli mam przykrycie lufy to włączam ten GameObject, jeśli nie mam to wyłączam
        turretCap.SetActive(myTank.iHaveTurretCap);

        //Jeśli wieża ma ograniczoną rotację to ją muszę wpierw wyprostować
        if (!myTank.turretCan360)
            turret.transform.localRotation = Quaternion.identity;

        //Dla graczy lokalnych
        if(photonView.isMine)
        {
            MainTurret.Instance.OgraniczonaRotacja = !myTank.turretCan360;

            TechTree.Instance.TankSwitchTierButton(myTank.level);

            TechTree.Instance.FindTankButton(TanksData.FindTankEnum(myTank)).OnClick();

            cameraOrthographicSize = myTank.cameraSize;
        }

        #region Wyjątki czołgów 

        //OI'emu musimy włączyć działka lub je wyłączyć (jeśli zginął i np. KHA-GO będzie przez to przechodzić)
        if (myTank.tank == DostempneCzolgi.O_I)
        {
            for (int i = 0; i < turretsOI.Length; i++)
            {
                turretsOI[i].SetActive(true);
                turretsOI[i].GetComponent<AutoShoot>().Reset();
            }
        }
        else
        {
            for (int i = 0; i < turretsOI.Length; i++)
            {
                turretsOI[i].SetActive(false);
            }
        }

        //IS7 również posiada wieżyczkę na wieży 
        if (myTank.tank == DostempneCzolgi.IS7)
        {
            turretIS.SetActive(true);
            turretIS.GetComponent<AutoShoot>().Reset();
        }
        else
        {
            turretIS.SetActive(false);
        }

        // natomiast HA-TO ma nierychomą wieżyczkę no ale trzeba ją wyprostować (prędkość obrotu ma = 0)  
        if (myTank.tank == DostempneCzolgi.HA_TO_300MM)
        {
            turret.transform.localRotation = Quaternion.identity;
        }
        #endregion
    }


    /// <summary>
    /// Ustawia wszystkie parametry czołgu.
    /// Wystarczy, że podasz enum czołgu jakiego parametry chcesz ustawić
    /// </summary>
    /// <param name="myTank">enum czołgu którego parametry chcesz przypisać maszynie</param>
    void SetTankParametrs(TankData myTank)
    {
        GetComponent<PlayerGO>().myPlayer.tank = TanksData.FindTankEnum(myTank);
        magazynek = myTank.maxAmmo;
        maxHp = myTank.maxHp;
        damage = myTank.damage;
        reload = myTank.reload;
        speed = myTank.speed;
        turnSpeed = myTank.turnSpeed;
        headTurnSpeed = myTank.headTurnSpeed;
        damageLotery = myTank.damageLotery;
        reloadBetweenMagazine = myTank.reloadBetweenMagazine;
        GetComponent<TankShoot>().ResetMagazine(myTank.maxAmmo);
        if (photonView.isMine)
            GetComponent<PlayerGO>().myPlayer.hp = myTank.maxHp;
    }


    /// <summary>
    /// Ustawia tekstury czołgu, jeśli nie podasz TankCapTexture to znaczy że dany czołg go nieposiada
    /// </summary>
    /// <param name="hullTexture">textura kadłuba czołgu</param>
    /// <param name="turretTexture">textura wieży lub lufy czołgu</param>
    /// <param name="turretCapTexture">textura zakrycia połączenia lufy z kadłubem u niszczycieli</param>
    /// <param name="hullLayer">order in layer hullTexture</param>
    /// <param name="turretLayer">order in layer turretTexture</param>
    /// <param name="turretCapLayer">order in layer turretCapTexture</param>
    void SetTankTexture( Sprite hullTexture, Sprite turretTexture = null, Sprite turretCapTexture = null,
        int hullLayer = 12, int turretLayer = 14, int turretCapLayer = 15)
    {
        hull.GetComponent<SpriteRenderer>().sprite = hullTexture;
        hull.GetComponent<SpriteRenderer>().sortingOrder = hullLayer;
        if (turretTexture != null)
        {
            turret.GetComponent<SpriteRenderer>().sprite = turretTexture;
            turret.GetComponent<SpriteRenderer>().sortingOrder = turretLayer;

        }
        if (turretCapTexture != null)
        {
            turretCap.GetComponent<SpriteRenderer>().sprite = turretCapTexture;
            turretCap.GetComponent<SpriteRenderer>().sortingOrder = turretCapLayer;

        }
        myShadow.RestartShadow();
    }


    /// <summary>
    /// Ustawia pozycje podanego GameObjectu w podanym vector3(x,y,z),
    /// przydatne do ustawiania pozycji wieży i startFirePoint
    /// </summary>
    /// <param name="x">objects.transform.position.x</param>
    /// <param name="y">objects.transform.position.y</param>
    /// <param name="z">objects.transform.position.z</param>
    /// <param name="objects">Ustawia pozycje podanego GameObjectu w podanym vector3(x,y,z)</param>
    void SetGameObjectPosition(float x, float y, GameObject objects)
    {
        objects.transform.localPosition = new Vector2(x, y);
    }


    /// <summary>
    /// Ustawia komponent BoxCollider2D czołgu
    /// </summary>
    /// <param name="posX">BoxCollider2D position(offset) x</param>
    /// <param name="posY">BoxCollider2D position(offset) y</param>
    /// <param name="sizeX">BoxCollider2D size x</param>
    /// <param name="sizeY">BoxCollider2D size y</param>
    void ResetPolygon(float posX, float posY, float sizeX, float sizeY)
    {
        for (int i = 0; i < tankColliders.Length; i++)
        {
            tankColliders[i].offset = new Vector2(posX, posY);
            tankColliders[i].size = new Vector2(sizeX, sizeY);
        }
    }


    /// <summary>
    /// Ustawia dane czołgu który gracz posiada
    /// </summary>
    public void UpdateTank()
    {
        //UstawCzolg(myTank);
        SetTankObject(TanksData.FindTankData(GetComponent<PlayerGO>().myPlayer.tank));
    }

    /// <summary>
    /// Bardo niebezpieczna metoda no ale cóż, kiedyś się poprawi ;) TODO: 0_0
    /// </summary>
    /// <param name="tank"></param>
    public void SetStartTankHowNewPlayer(DostempneCzolgi tank)
    {
        SetTankObject(TanksData.FindTankData(tank));
    }

    ///////////////////////////////////////////////////////////////
    // [*] dla viodów dla każdego czołgu
    ///////////////////////////////////////////////////////////////

    ///////////////////////////////////////////////////////////////
    #region Do procesu ulepszenia czołgu
    ///////////////////////////////////////////////////////////////



    /*  
     * ######################################################################################
     * #  Wszystko zaczyna się kiedy gracz klikie kikolwiek czołg z drzewka  void Button()  #
     * #      (Za to czy przyciski jest aktywny czy nieaktywny odpowiada HUDManagera)       #
     * #                                                                                    #
     * #  Następnie informacja o kliknięciu jest przekazywana do servera i to ON sprawdza:  #
     * #     - ile gracz ma zebrane score (score zebrane w kopi gry servera)                #
     * #     - jaki poziom ma gracz                                                         #
     * #     - czy ulepszanie czołgu wykonuje server                                        #
     * #                                                                                    #
     * #                 Jeśli wszystko jest ok czołg zostaje ulepszony :D                  #
     * ######################################################################################
     */


    public void TankButton(TankData newTank)
    {
        UpdateTank(newTank);
        //GetComponent<TankStore>().techTree.TankSwitchTierButton(newTank.level-1);
        //tb.OnClick();
    }

    /// <summary>
    /// Wysyła zapytanie do servera czy może kupić sobie podany czołg.
    /// Jeśli może to go kupuje i wysyła tą wiadomość do wszystkich
    /// </summary>
    /// <param name="newTank">Tank whtch I wont to have</param>
    public void UpdateTank(TankData newTank)
    {
        photonView.RPC("PleaseServerForUpdateRPC", PhotonTargets.MasterClient, newTank.tank);
    }


    [PunRPC]
    void PleaseServerForUpdateRPC(DostempneCzolgi newTank, PhotonMessageInfo pmi)
    {
        #region Security
        int playerOldTankLevel = TanksData.FindTankData(Player.FindPlayer(pmi.sender).tank).level;  //(int)newTank - 1;// TankManager.czolgii[newTank].level - 1;//Player.FindPlayer(pmi.sender).gameObject.GetComponent<TankEvolution>().myTank.level;
        int requiredScore = TankEvolution.UstawGranice(playerOldTankLevel);

        if (!PhotonNetwork.isMasterClient)
        {
            Debug.LogError("Tylko server ma prawo ulepszać czołgi graczy!"); //to ma sens ;)
            return;
        }
        if (TanksData.FindTankData(newTank).level != playerOldTankLevel + 1)
        {
            Debug.LogError("Niewłaściwy level 'starego' czołgu gracza! ~" +
            TanksData.FindTankData(newTank).level + " <= " + (playerOldTankLevel + 1).ToString()
            ); //może być tylko o jeden mniejszy od nowego albo ten sam
            return;
        }
        if (Player.FindPlayer(pmi.sender).score < requiredScore)
        {
            Debug.LogError("Gracz ma za mało score!"); //Przynajmniej w kopii gry servera
            return;
        }
        #endregion
        {
            TankData goodTankParametrs = TanksData.FindTankData(newTank);
            Player.FindPlayer(pmi.sender).tank = newTank;   //Server przechowuje i tylko on sobie ustawia właściwy typ i u siebie trzyma
            photonView.RPC("UstawCzolgRPC", PhotonTargets.All, newTank,
                goodTankParametrs.cameraSize,
                goodTankParametrs.damage,
                goodTankParametrs.damageLotery,
                goodTankParametrs.maxHp,
                goodTankParametrs.reload,
                goodTankParametrs.speed,
                goodTankParametrs.maxAmmo
                );
        }//    /
    }//        \ <- upośledzona strzałka ;)
    //         /
    //         \ Po prostu server wysyła dane czołgu ze swojej kopii gry a 
    //         / gracz je podmienia i odrazu ustawia aktualizując czołg.
    //         \
    [PunRPC]// \/
    void UstawCzolgRPC(DostempneCzolgi newTank,
                float cameraSize,
                float damage,
                float damageLotery,
                float maxHp,
                float reload,
                float speed,
                int maxAmmo )
    {
        TankData mySuspectTank = TanksData.FindTankData(newTank);
        mySuspectTank.cameraSize = cameraSize;
        mySuspectTank.damage = damage;
        mySuspectTank.damageLotery = damageLotery;
        mySuspectTank.maxHp = maxHp;
        mySuspectTank.reload = reload;
        mySuspectTank.speed = speed;
        mySuspectTank.maxAmmo = maxAmmo;

        SetTankObject(mySuspectTank);
    }


    public static int UstawGranice(int poziomCzolguGracza)
    {
        int coZwrocic = 0;
        switch (poziomCzolguGracza)
        {
            case 1: coZwrocic = GameManager.One_level; break;
            case 2: coZwrocic = GameManager.Two_level; break;
            case 3: coZwrocic = GameManager.Tree_level; break;
            default:
                break;
        }
        return coZwrocic;
    }

    ///////////////////////////////////////////////////////////////
    #endregion
    ///////////////////////////////////////////////////////////////
}
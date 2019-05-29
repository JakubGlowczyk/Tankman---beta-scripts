using UnityEngine;

/*
 * ###################################
 * #            by Unity             #
 * #            [modifed]            #
 * ###################################
 */

public class TankEngine : Engine, ICanMove, ICanTurn
{
    public static TankEngine Instance { get; private set; }

    public TankStore tankStore; //dla Zapór 
    [SerializeField]
    private PhotonView myPV;

    public override float MoveSpeed { get { return TankEvolution.Instance.Speed; }}

    public override float TurnSpeed{ get { return TankEvolution.Instance.TurnSpeed; } }

    public float SpeedValue { get; private set; }
    public float TurnValue { get; private set; }

    public bool cofanie { get; private set; }


    void Awake()
    {
        if (!myPV.isMine)
            enabled = false;
        else if (Instance == false)
            Instance = this;
        else
            enabled = false;
    }

    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        EngineAudio(SpeedValue, TurnValue);

        SpeedValue = Input.GetAxis("Vertical1");
        TurnValue = Input.GetAxis("Horizontal1");

        if (Input.GetKey(KeyCode.S))
            //cofanie
            cofanie = true;
        else if (Input.GetKey(KeyCode.W))
            //jazda!
            cofanie = false;
    }

    private void FixedUpdate()
    {
                                      float tempSpeed = (cofanie) 
                                                    ?
                (TankWaterCollision.Instance.ISwim || TankWaterCollision.Instance.ISink)
                /*jeśli cofam w wodzie*/            ?     /*jeśli cofam 'nie' w wodzie*/
                MoveSpeed * 0.75f * 0.65f           :                 MoveSpeed * 0.75f 

                :
                
                (TankWaterCollision.Instance.ISwim || TankWaterCollision.Instance.ISink)
                /*jeśli jadę normalnie przez wode*/ ?           /*jeśli jadę normalnie*/
                MoveSpeed * 0.65f                   :                      MoveSpeed;

        Move(tempSpeed, SpeedValue);
        TurnForValue(TurnSpeed, TurnValue);
    }
}
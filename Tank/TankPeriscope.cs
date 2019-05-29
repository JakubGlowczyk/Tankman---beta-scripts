using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Peryskop czołgu, widzi graczy w pobliżu
/// </summary>
public class TankPeriscope : MonoBehaviour
{
    public List<Player> PlayersInNear { get; private set; }

    [SerializeField]
    private TankStore tankStore;
    [SerializeField]
    private Shake cameraShake;



    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == Tag.REMOTEPLAYERBODY)
        {
            tankStore.onPlayerDead += collision.GetComponent<TankObject>().PlayerGO.GetComponent<TankStore>().tankPeriscope.CameraShake;
            //PlayersInNear.Add(collision.GetComponent<Keeper>().keep.GetComponent<PlayerGO>().myPlayer);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == Tag.REMOTEPLAYERBODY)
        {
            tankStore.onPlayerDead -= collision.GetComponent<TankObject>().PlayerGO.GetComponent<TankStore>().tankPeriscope.CameraShake;
            //PlayersInNear.Remove(collision.GetComponent<Keeper>().keep.GetComponent<PlayerGO>().myPlayer);
        }
    }

    void CameraShake()
    {
        Debug.Log("Potrząsam kamerą!");
        tankStore.ShakeCamera();
    }
}

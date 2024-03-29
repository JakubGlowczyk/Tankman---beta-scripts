﻿using UnityEngine;  
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/*
 * ###################################
 * #        by Jakub Główczyk        #
 * #            [#][ ][ ]            #
 * ###################################
 */

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    public static int tempGranicaWbicjaLewla;

    public PlayerGO playerGO;
    public TechTree techTree;

    public GameObject[] zebatki;
    public GameObject zasobyMenu;

    public Slider SliderHp;
    public Slider SliderHpOnTank;
    public Slider SliderExp;
    public Slider SliderAmmo;
    public Slider SliderReload;
    public Slider SliderRegeneration;
    public Color fullExpFillColor;
    public Color normalExpFillColor;

    public Text meltTime;
    public Text hpMaxHp;
    public Text expMaxExpText;
    public Text coinText;
    public Text coinTextInShoop;
    public Text currentAmmoText;
    public static Text nickText;
    public Text _nickText;
    public static Text consoll;
    public Text _consoll;
    public Text dynamitText;
    public Text naprawiarkaText;
    public Text zasobyText;
    public Image dynamit;
    public GameObject dynamitTlo;
    public Image naprawiarka;
    public GameObject naprawiarkaTlo;
    public Image zasoby;

    [SerializeField]
    private PhotonView myPV;


    public void Awake()
    {
        if (!myPV.isMine)
            enabled = false;
        else if (Instance == null)
            Instance = this;
        else
            enabled = false;
    }

    void Start ()
    {
        consoll = _consoll;
        nickText = _nickText;
        nickText.text = playerGO.myPlayer.nick;
        StartRefresh();
	}


    public void StartRefresh()
    {
        StartCoroutine(SetHUDOnUpdate());
    }

    IEnumerator SetHUDOnUpdate()
    {
        while(true)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            ChcekUpdate();
        }
    }

	
	void ChcekUpdate ()
    {
        SliderHp.value = CalculateHealth();
        //SliderHpOnTank.value = SliderHp.value; to jest ustawiane w PlayerRegenerationHP
        SliderAmmo.value = (float)TankShoot.Instance.TempMaxAmmo / (float)TankShoot.Instance.MaxAmmo;
        SliderReload.value = ((TankShoot.Instance.realReloadTime * -1f) + TankShoot.Instance.ReloadTime) / TankShoot.Instance.ReloadTime;
        if (playerGO.myPlayer.hp != TankHealth.Instance.MaxHP)
            SliderRegeneration.value = ((TankHealth.Instance.tempTime * -1f) + TankHealth.Instance.CzasDoRozpoczeciaRegeneracji) / TankHealth.Instance.CzasDoRozpoczeciaRegeneracji;
        else
            SliderRegeneration.value = 1;

        if (SliderExp.value == 1)
            SliderExp.fillRect.GetComponent<Image>().color = fullExpFillColor;
        else
            SliderExp.fillRect.GetComponent<Image>().color = normalExpFillColor;

        hpMaxHp.text = playerGO.myPlayer.hp + "/" + TankHealth.Instance.MaxHP;
        if(GameManager.LocalPlayer.tankTier == TankTier.CzrawtyTier)
            expMaxExpText.text = playerGO.myPlayer.score + "/∞";
        else
            expMaxExpText.text = playerGO.myPlayer.score + "/" + tempGranicaWbicjaLewla.ToString();
        currentAmmoText.text = TankShoot.Instance.TempMaxAmmo.ToString();

        coinText.text = "   Coin: <color=yellow>"+ playerGO.myPlayer.coin.ToString()+"</color>";
        coinTextInShoop.text = coinText.text;

        zasobyText.text = "x" + playerGO.myPlayer.Zasoby.ToString();
        naprawiarkaText.text = "x" + playerGO.myPlayer.Naprawiarka.ToString();
        dynamitText.text = "x" + playerGO.myPlayer.Dynamit.ToString();

        switch (GameManager.LocalPlayer.tankTier)
        {
            case TankTier.PierwszyTier:
                PrzelaczZebatki(0);
                tempGranicaWbicjaLewla = GameManager.One_level;
                SliderExp.value = (float)playerGO.myPlayer.score / tempGranicaWbicjaLewla;
                break;

            case TankTier.DrugiTier:
                PrzelaczZebatki(1);
                tempGranicaWbicjaLewla = GameManager.Two_level;
                SliderExp.value = ((float)playerGO.myPlayer.score - GameManager.One_level) / (tempGranicaWbicjaLewla - GameManager.One_level);
                break;

            case TankTier.TrzeciTier:
                PrzelaczZebatki(2);
                tempGranicaWbicjaLewla = GameManager.Tree_level;
                SliderExp.value = ((float)playerGO.myPlayer.score - GameManager.Two_level) / (tempGranicaWbicjaLewla - GameManager.Two_level);
                break;

            case TankTier.CzrawtyTier:
                PrzelaczZebatki(3);
                tempGranicaWbicjaLewla = playerGO.myPlayer.score + 1501;
                SliderExp.value = 1;
                break;
        }
    }

    void PrzelaczZebatki(int aktywnaZebatka)
    {
        for (int i = 0; i < zebatki.Length; i++)
        {
            if (i == aktywnaZebatka)
                zebatki[i].SetActive(true);
            else if (i < aktywnaZebatka)
                zebatki[i].SetActive(true); 
            else
                zebatki[i].SetActive(false);
        }
    }


    public float CalculateHealth()
    {
        return playerGO.myPlayer.hp / TankHealth.Instance.MaxHP;
    }


    public enum TankTier
    {
        PierwszyTier,
        DrugiTier,
        TrzeciTier,
        CzrawtyTier
    }

    public static void OnEnterButton(string text)
    {
        consoll.text = text;
        nickText.gameObject.SetActive(false);
    }

    public void OnEnterButtonUI(string text)
    {
        consoll.text = text;
        nickText.gameObject.SetActive(false);
    }

    public void OnExitButton()
    {
        consoll.text = "";
        nickText.gameObject.SetActive(true);
    }


    public void CzyPokazacMenuZasobow(bool tak)
    {
        if (tak)
            zasobyMenu.SetActive(true);
        else
            zasobyMenu.SetActive(true); //TODO: inna opcja
    }
}

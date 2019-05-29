using UnityEngine;

/*
 * ###################################
 * #        by Jakub Główczyk        #
 * #            [#][#][#]            #
 * ###################################
 */

public class ShadowEffect : MonoBehaviour
{
    [Range(0.05f, 0.15f)]
    //[SerializeField] kurde, chcę aby to było edutowane w Editor więc musi być public
    public float objectHeight = 0.05f;

    [Range(0,255)]
    //[SerializeField]
    public int shadowIntensity = 100;

    //[SerializeField]
    public Vector3 shadowScale = new Vector3(1, 1, 1);

    public GameObject shadow;

    public delegate void CheckShadow();
    public event CheckShadow ShadowWasDestroyed;



    public ShadowEffect(int objectHeight, int shadowIntensity, Vector3 shadowScale)
    {
        this.objectHeight = objectHeight;
        this.shadowIntensity = shadowIntensity;
        this.shadowScale = shadowScale;
    }

    public void RestartShadow()
    {
        SpriteRenderer renderer = new SpriteRenderer();
        SpriteRenderer sr = new SpriteRenderer();

        if (shadow == null)
        {
            Debug.LogWarning("Nie znaleziono cienia!");
            return;
        }

        if(GetComponent<SpriteRenderer>() == null)
        {
            Debug.LogWarning("Nie ma z czego utworzyć cienia!");
            return;
        }
        else
        {
            renderer = GetComponent<SpriteRenderer>();
        }

        if (shadow.GetComponent<SpriteRenderer>() == null)
            sr = shadow.AddComponent<SpriteRenderer>();
        else
            sr = shadow.GetComponent<SpriteRenderer>();

        sr.sprite = GetComponent<SpriteRenderer>().sprite;
        sr.color = new Color32(0, 0, 0, (byte)shadowIntensity);

        sr.sortingLayerName = renderer.sortingLayerName;
        sr.sortingOrder = renderer.sortingOrder - 1;
    }

    public void CreateShadow()
    {
        if(shadow == null)
        {
            shadow = new GameObject("Shadow");
            shadow.AddComponent<Shadow>();
            shadow.GetComponent<Shadow>().myShadowEffect = this;
            ShadowWasDestroyed += shadow.GetComponent<Shadow>().DestroyShadow;
            shadow.transform.localScale = shadowScale;
            RestartShadow();
        }
    }

    public void SetPosition()
    {
        shadow.transform.rotation = transform.rotation;
        shadow.transform.position = new Vector3(transform.position.x + objectHeight, transform.position.y - objectHeight, 0);
    }


    void Awake()
    {
        CreateShadow();
    }

    public void Update()
    {
        SetPosition();
    }

    public void OnEnable()
    {
        if (shadow != null)
            shadow.SetActive(true);
    }

    public void OnDisable()
    {
        if (this == null)
            DestroyImmediate(shadow);

        if(shadow != null)
            shadow.SetActive(false);
    }

    public void OnDestroy()
    {
        SetDelegate();

        if (shadow != null)
            Destroy(shadow);
    }

    public void SetDelegate()
    {
        if(ShadowWasDestroyed != null)
            ShadowWasDestroyed();
    }
}
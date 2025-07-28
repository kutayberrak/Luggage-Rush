using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SettingsMenu : MonoBehaviour
{
    [Header("space between menu items")]
    [SerializeField] Vector2 spacing;

    [Space]
    [Header("Main button rotation")]
    [SerializeField] float rotationDuration;
    [SerializeField] Ease rotationEase;

    [Space]
    [Header("Animation")]
    [SerializeField] float expandDuration;
    [SerializeField] float collapseDuration;
    [SerializeField] Ease expandEase;
    [SerializeField] Ease collapseEase;

    [Space]
    [Header("Fading")]
    [SerializeField] float expandFadeDuration;
    [SerializeField] float collapseFadeDuration;

    [Space]
    [Header("Background")]
    [SerializeField] Image backgroundImage;
    [SerializeField] Color backgroundColor = new Color(0, 0, 0, 0.5f);
    [SerializeField] float initialHeight = 50f;
    [SerializeField] float expandedHeight = 300f;

    [SerializeField] Button mainButton;
    SettingsMenuItem[] menuItems;
    RectTransform backgroundRect;

    //is menu opened or not
    bool isExpanded = false;

    Vector2 mainButtonPosition;
    int itemsCount;

    void Start()
    {
        //add all the items to the menuItems array
        itemsCount = transform.childCount - 1;
        menuItems = new SettingsMenuItem[itemsCount];
        for (int i = 0; i < itemsCount; i++)
        {
            // +1 to ignore the main button
            menuItems[i] = transform.GetChild(i + 1).GetComponent<SettingsMenuItem>();
        }

        mainButton = transform.GetChild(0).GetComponent<Button>();
        mainButton.onClick.AddListener(ToggleMenu);
        //SetAsLastSibling () to make sure that the main button will be always at the top layer
        mainButton.transform.SetAsLastSibling();

        mainButtonPosition = mainButton.GetComponent<RectTransform>().anchoredPosition;

        //set all menu items position to mainButtonPosition
        ResetPositions();

        // Başlangıçta arka planı gizle
        if (backgroundImage != null)
        {
            backgroundRect = backgroundImage.GetComponent<RectTransform>();
            // backgroundImage.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0);

            // Arka planın başlangıç boyutunu ayarla
            backgroundRect.sizeDelta = new Vector2(backgroundRect.sizeDelta.x, initialHeight);
        }
    }

    void ResetPositions()
    {
        for (int i = 0; i < itemsCount; i++)
        {
            menuItems[i].rectTrans.anchoredPosition = mainButtonPosition;
        }
    }

    void ToggleMenu()
    {
        isExpanded = !isExpanded;

        if (isExpanded)
        {
            //menu opened
            for (int i = 0; i < itemsCount; i++)
            {
                menuItems[i].rectTrans.DOAnchorPos(mainButtonPosition + spacing * (i + 1), expandDuration).SetEase(expandEase);

                //Fade to alpha=1 starting from alpha=0 immediately
                //menuItems[i].img.DOFade(1f, expandFadeDuration).SetAutoKill(true);

                //menuItems[i].img.color = new Color(menuItems[i].img.color.r, menuItems[i].img.color.g, menuItems[i].img.color.b, 0f);
            }

            // Arka planı göster ve genişlet
            if (backgroundImage != null)
            {
                backgroundRect.DOSizeDelta(new Vector2(backgroundRect.sizeDelta.x, expandedHeight), expandDuration).SetEase(expandEase);
                //backgroundImage.DOFade(backgroundColor.a, expandFadeDuration);
            }
        }
        else
        {
            //menu closed
            for (int i = 0; i < itemsCount; i++)
            {
                menuItems[i].rectTrans.DOAnchorPos(mainButtonPosition, collapseDuration).SetEase(collapseEase);
                //Fade to alpha=0
                // menuItems[i].img.DOFade(0f, collapseFadeDuration);
            }

            // hide background and collapse
            if (backgroundImage != null)
            {
                backgroundRect.DOSizeDelta(new Vector2(backgroundRect.sizeDelta.x, initialHeight), collapseDuration).SetEase(collapseEase);
                // backgroundImage.DOFade(0f, collapseFadeDuration);
            }
        }


    }



    void OnDestroy()
    {
        //remove click listener to avoid memory leaks
        mainButton.onClick.RemoveListener(ToggleMenu);
    }
}

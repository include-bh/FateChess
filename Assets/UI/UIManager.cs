using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIManager : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler
{
    public static UIManager Instance { get; private set; }
    public static readonly int[] UIXpos = { -225, -75, 75, 225 }; 
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }
    [Header("玩家")]
    public Player curPlayer = null;
    public List<UIRenderer> CardSlot=new List<UIRenderer>();
    [Header("按键")]
    public Button useCardButton;
    public Button skipTurnButton;

    [Header("视觉效果")]
    public SpriteAtlas commandAtlas;
    public Image CommandCount;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardDescriptionText;
    public ScrollRect scrollRect;
    public Card curOnCard = null;

    public GameObject UICardPrefab;
    public GameObject UIPiecePrefab;
    public Transform canvas;
    
    void Start()
    {
    }

    public void UpdateCommandCount()
    {
        int x = curPlayer.CommandCount;
        if (x > 3) x = 3;
        CommandCount.sprite = commandAtlas.GetSprite($"Player{x}");
    }

    public void UpdateHandCard()
    {
        for (int i = 0; i < 4; i++)
        {
            CardSlot[i].data = curPlayer.hand[i];
            CardSlot[i].UpdateSprite();
        }
    }

    public void GenerateUIRenderer(int id, Card card)
    {
        if (card is Piece piece)
        {
            GameObject GO = Instantiate(UIPiecePrefab, canvas);
            UIPieceRenderer rend = GO.GetComponent<UIPieceRenderer>();
            rend.pos = id; rend.data = card; card.renderer = rend;
            CardSlot[id] = rend;
        }
        else
        {
            GameObject GO = Instantiate(UICardPrefab, canvas);
            UIRenderer rend = GO.GetComponent<UIRenderer>();
            rend.pos = id; rend.data = card; card.renderer = rend;
            CardSlot[id] = rend;
        }
        CardSlot[id].FlyIn();
    }
    
    public void UpdateDescription()
    {
        if (curOnCard == null)
        {
            cardNameText.text = "";
            cardDescriptionText.text = "";
        }
        else
        {
            cardNameText.text = curOnCard.cardName;
            cardDescriptionText.text = curOnCard.GetDescription();
        }
        scrollRect.verticalNormalizedPosition = 1f;
    }
}

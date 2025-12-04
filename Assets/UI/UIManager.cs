using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
    public GameObject curUI;

    [Header("常态UI")]
    public GameObject NormalUI;
    public Button RefreshCardButton;
    public Button EndTurnButton;
    public SpriteAtlas commandAtlas;
    public Image CommandCount;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardDescriptionText;
    public ScrollRect scrollRect;
    public Card curOnCard = null;

    [Header("选择中UI")]
    public GameObject SelectUI;
    public Button FinishButton;

    [Header("无懈可击UI")]
    public GameObject WuXieUI;
    public Button UseWuXieButton;
    public Button NotUseWuXieButton;

    [Header("死亡UI")]
    public GameObject DeathUI;
    public TextMeshProUGUI DeathText;

    [Header("预制体")]
    public GameObject UICardPrefab;
    public GameObject UIPiecePrefab;
    public Transform canvas;

    void Start()
    {
        CardSlot.Clear();
        for (int i = 0; i < 4; i++)
        {
            GameObject GO = Instantiate(UICardPrefab, canvas);
            UIRenderer rend = GO.GetComponent<UIRenderer>();
            rend.pos = i;
            CardSlot.Add(rend);
        }
    }

    public void OnEndTurnClicked()
    {
        curPlayer.TurnEndTcs?.TrySetResult();
    }
    public void OnRefreshCardClicked()
    {
        curPlayer.RefreshCard();
    }
    public event Action FinishSelect;
    public void OnFinishClicked()
    {
        FinishSelect?.Invoke();
        FinishSelect = null;
    }
    
    public void SwitchToSelectUI()
    {
        curUI.SetActive(false);
        curUI = SelectUI;
        curUI.SetActive(true);
    }
    public void SwitchToNormalUI()
    {
        curUI.SetActive(false);
        curUI = NormalUI;
        curUI.SetActive(true);
    }
    public void SwitchToWuXieUI()
    {
        curUI.SetActive(false);
        curUI = WuXieUI;
        curUI.SetActive(true);
    }
    public void SwitchToDeathUI()
    {
        curUI.SetActive(false);
        curUI = DeathUI;
        curUI.SetActive(true);
    }
    
    public void UpdateCommandCount()
    {
        int x = curPlayer.CommandCount;
        if (x > 3) x = 3;
        CommandCount.sprite = commandAtlas.GetSprite($"Command{x}");
    }

    public void UpdateHandCard()
    {
        for (int i = 0; i < 4; i++)
            UpdateUIRendererImmediately(i, curPlayer.hand[i]);
    }

    public void SetupUIRenderer(int id, Card card)
    {
        Destroy(CardSlot[id].gameObject);
        if (card is Piece piece)
        {
            GameObject GO = Instantiate(UIPiecePrefab, canvas);
            UIPieceRenderer rend = GO.GetComponent<UIPieceRenderer>();
            rend.pos = id; rend.data = piece; piece.renderer = rend;
            CardSlot[id] = rend;
        }
        else
        {
            GameObject GO = Instantiate(UICardPrefab, canvas);
            UIRenderer rend = GO.GetComponent<UIRenderer>();
            rend.pos = id; rend.data = card; card.renderer = rend;
            CardSlot[id] = rend;
        }
        CardSlot[id].InitSprite();
    }
    public async UniTask UpdateUIRenderer(int id, Card card)
    {
        SetupUIRenderer(id, card);
        await CardSlot[id].FlyIn();
    }
    public void UpdateUIRendererImmediately(int id,Card card)
    {
        SetupUIRenderer(id, card);
        CardSlot[id].gameObject.SetActive(true);
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

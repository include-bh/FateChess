using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
    public GameObject PlayerSwitchUI;
    public Image ShowCurPlayer;

    [Header("常态UI")]
    public GameObject NormalUI;
    public Button RefreshCardButton;
    public void OnRefreshCardClicked()
    {
        curPlayer.RefreshCard();
    }
    public Button EndTurnButton;
    public void OnEndTurnClicked()
    {
        curPlayer.TurnEndTcs?.TrySetResult();
    }
    public SpriteAtlas commandAtlas;
    public Image CommandCount;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardDescriptionText;
    public ScrollRect scrollRect;
    public Card curOnCard = null;

    [Header("选择中UI")]
    public GameObject SelectUI;
    public Button FinishButton;
    public event Action FinishSelect;
    public void OnFinishClicked()
    {
        FinishSelect?.Invoke();
    }
    public void ClearFinish()
    {
        FinishSelect = null;
    }

    [Header("无懈可击UI")]
    public GameObject WuXieUI;
    public Button UseWuXieButton;
    public event Action UseWuXiePress;
    public void OnUseClicked()
    {
        UseWuXiePress?.Invoke();
    }
    public Button NotUseWuXieButton;
    public event Action NotUseWuXiePress;
    public void OnNotUseClicked()
    {
        NotUseWuXiePress?.Invoke();
    }
    public TextMeshProUGUI WuXieTitle;

    [Header("死亡UI")]
    public GameObject DeathUI;
    public TextMeshProUGUI DeathTitle;
    public Button ReturnButton;
    public void DeathReturn()
    {
        SceneManager.LoadScene("StartScene");
    }

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


    public async UniTask ShowPlayerSwitch(Player player)
    {
        PlayerSwitchUI.SetActive(true);
        if (GameManager.Instance.playerAtlas != null)
        {
            string bgName = $"Player{player.id}";
            ShowCurPlayer.sprite = GameManager.Instance.playerAtlas.GetSprite(bgName);
        }
        await UniTask.Delay(1000);
        PlayerSwitchUI.SetActive(false);
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

    public void SetCurPlayer(Player cur)
    {
        curPlayer = cur;
        UpdateCommandCount();
        UpdateHandCard();
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
            cardNameText.text = " ";
            cardDescriptionText.text = " ";
        }
        else
        {
            cardNameText.text = curOnCard.cardName;
            cardDescriptionText.text = curOnCard.GetDescription() + "\n ";
        }

        cardDescriptionText.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }
    
}

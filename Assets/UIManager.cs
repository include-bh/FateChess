using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Buttons")]
    public Button useCardButton;
    public Button skipTurnButton;
    
    [Header("Timer Display")]
    public Text timerText;
    
    [Header("Visual Feedback")]
    public GameObject timeWarningEffect; // 时间不足时的警告效果
    
    private bool isTimerWarningActive = false;
    private const float WARNING_THRESHOLD = 5f;
    void Start()
    {
        //SetupUI();
        //SetupEvents();
    }

    void SetupUI()
    {
        // 设置手牌布局
        if (currentPlayer.handCardsParent.GetComponent<HorizontalLayoutGroup>() == null)
        {
            var layout = currentPlayer.handCardsParent.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.padding = new RectOffset(20, 20, 20, 20);
        }

        // 设置手牌容器的锚点（屏幕底部）
        RectTransform handRect = currentPlayer.handCardsParent.GetComponent<RectTransform>();
        handRect.anchorMin = new Vector2(0, 0);
        handRect.anchorMax = new Vector2(1, 0);
        handRect.pivot = new Vector2(0.5f, 0);
        handRect.anchoredPosition = new Vector2(0, 50);
    }
    
    void SetupEvents()
    {
        // 按钮事件
        useCardButton.onClick.AddListener(UseSelectedCards);
        skipTurnButton.onClick.AddListener(SkipTurn);

        // 回合事件
        TurnManager.Instance.onTurnStart.AddListener(OnTurnStart);
        TurnManager.Instance.onTurnEnd.AddListener(OnTurnEnd);
        TurnManager.Instance.onTimeUpdate.AddListener(OnTimeUpdate);

        // 玩家事件
        currentPlayer.onCardPlayed.AddListener(OnCardPlayed);
        currentPlayer.onCardDiscarded.AddListener(OnCardDiscarded);
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}

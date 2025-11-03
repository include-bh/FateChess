using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }
    [Header("玩家")]
    public Player curPlayer;
    [Header("按键")]
    public Button useCardButton;
    public Button skipTurnButton;

    [Header("视觉效果")]
    public TextMeshPro timer;
    public ImageSpriteRenderer CmdCnt;
    void Start()
    {
        SetupUI();
        SetupEvents();
    }

    void SetupUI()
    {
    }
    
    void SetupEvents()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    void UpdateCmdCnt()
    {
        if (curPlayer.CommandCount <= 3)
            CmdCnt.ChangeToSpriteByIndex(curPlayer.CommandCount);
        else CmdCnt.ChangeToSpriteByIndex(3);
    }
}

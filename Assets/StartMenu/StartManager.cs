using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartManager : MonoBehaviour
{
    public static readonly int[] dx = { 1, 0, -1, -1, 0, 1, 0 };
    public static readonly int[] dy = { 0, 1, 1, 0, -1, -1, 0 };
    public static StartManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        UpdateTeamDropdown();
        presetDropdown.ClearOptions();
        presetDropdown.AddOptions(playersPreset.Select(x => x.name).ToList());
        presetDropdown.SetValueWithoutNotify(0);
        presetDropdown.onValueChanged.RemoveAllListeners();
        presetDropdown.onValueChanged.AddListener(LoadPreset);
    }

    public List<PlayerConfig> players;
    public Transform canvas;
    public GameObject SlotPrefab;
    public TMP_Dropdown presetDropdown;

    public static readonly List<TeamPreset> playersPreset = new List<TeamPreset>()
    {
        new TeamPreset{name="选择预设", players=new List<int>{}},
        new TeamPreset{name="经典双人", players=new List<int>{1,2}},
        new TeamPreset{name="经典三人", players=new List<int>{1,2,3}},
        new TeamPreset{name="四人混战", players=new List<int>{1,2,3,4}},
        new TeamPreset{name="双排对战", players=new List<int>{1,2,1,2}},
    };

    public void LoadPreset(int index)
    {
        if (index < 0 || index >= playersPreset.Count) return;
        while (players.Count > 0) RemovePlayer(0);
        var preset = playersPreset[index];
        for (int i = 0; i < preset.players.Count; i++)
        {
            GameObject newSlotGO = Instantiate(SlotPrefab,canvas,false);

            PlayerConfig cfg = newSlotGO.GetComponent<PlayerConfig>();
            cfg.data.team = preset.players[i];
            cfg.data.type = (i == 0 ? PlayerType.Human : PlayerType.AIBalanced);
            cfg.init();
            players.Add(cfg);

            newSlotGO.SetActive(true);
        }
        UpdateTeamDropdown();
        UpdateUI();
    }

    public void AddPlayer()
    {
        if (players.Count >= 4) return;
        int defaultTeam = players.Count + 1;

        GameObject newSlotGO = Instantiate(SlotPrefab,canvas,false);

        PlayerConfig cfg = newSlotGO.GetComponent<PlayerConfig>();
        cfg.data.team = defaultTeam;
        cfg.data.type = PlayerType.Human;
        cfg.init();
        players.Add(cfg);

        newSlotGO.SetActive(true);

        UpdateTeamDropdown();
        UpdateUI();
    }
    public void RemovePlayer(int i)
    {
        if (0 <= i && i < players.Count)
        {
            Destroy(players[i].gameObject);
            players.RemoveAt(i);
            UpdateTeamDropdown();
            UpdateUI();
        }
    }
    public void SetPlayerTeam(int i,int j)
    {
        if (0 <= i && i < players.Count)
        {
            players[i].data.team = j;
            UpdateTeamDropdown();
            UpdateUI();
        }
    }
    public void SetPlayerType(int i,PlayerType j)
    {
        if (0 <= i && i < players.Count)
        {
            players[i].data.type = j;
            UpdateTeamDropdown();
            UpdateUI();
        }
    }

    public List<string> TeamOptions;
    public void UpdateTeamDropdown()
    {
        var existingTeams = new HashSet<int>();
        foreach (var cfg in players)
            existingTeams.Add(cfg.data.team);
        var teamList = new List<int>(existingTeams);
        teamList.Sort();

        var Map = new Dictionary<int, int>();
        for (int i = 0; i < teamList.Count; i++) Map[teamList[i]] = i + 1;
        foreach (var cfg in players)
            cfg.data.team = Map[cfg.data.team];


        TeamOptions.Clear();
        for (int i = 1; i <= teamList.Count; i++) 
            TeamOptions.Add($"队伍 {i}");
        if (TeamOptions.Count < 4)
        {
            int newTeamId = teamList.Count + 1;
            teamList.Add(newTeamId);
            TeamOptions.Add($"新建队伍 {newTeamId}");
        }

        foreach (var cfg in players)
            cfg.UpdateTeamDropdown();
    }

    public void UpdateUI()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].id = i;
            players[i].text.text = $"玩家{i+1}";
            RectTransform rect = players[i].gameObject.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, 260 - i * 150);
        }
    }

    public void StartGame()
    {
        if (players.Count < 2) return;
        if (TeamOptions.Count < 3) return;
        StartCoroutine(InitGameManager());
    }
    
    
    private Terrain GetRandType()
    {
        int x = UnityEngine.Random.Range(0, 10);
        if (x <= 0) return Terrain.Water;
        if (x <= 2) return Terrain.Hill;
        return Terrain.Plain;
    }

    public IEnumerator InitGameManager()
    {
        //随机种子
        int seed = System.DateTime.Now.GetHashCode() ^
            System.Guid.NewGuid().GetHashCode() ^
            (int)(Time.realtimeSinceStartup * 1000);
        UnityEngine.Random.InitState(seed);

        //场景切换
        List<PlayerConfigData> data = players.Select(x => x.data).ToList();
        SceneManager.LoadScene("GameScene");

        yield return new WaitForSeconds(1f);

        //初始化玩家
        foreach (var cfg in data)
        {
            if (cfg.type == PlayerType.Human)
                GameManager.Instance.players.Add(new Player(cfg.team));
            if (cfg.type == PlayerType.AIBalanced)
                GameManager.Instance.players.Add(new AIPlayer(cfg.team, 1, 1));
            if (cfg.type == PlayerType.AIAttack)
                GameManager.Instance.players.Add(new AIPlayer(cfg.team, 2, 1));
            if (cfg.type == PlayerType.AIDefence)
                GameManager.Instance.players.Add(new AIPlayer(cfg.team, 1, 2));
        }
        for (int i = 0; i < GameManager.Instance.players.Count; i++)
            GameManager.Instance.players[i].id = i;

        //生成牌堆
        GameManager.Instance.DiscardPile = FindObjectsOfType<Card>().ToList();
        GameManager.Instance.FlushCard();

        //生成棋盘
        for (int i = 0; i < 7; i++)
        {
            GameManager.Instance.AddTile(0 + dx[i], 0 + dy[i], GetRandType(), i == 6);
            GameManager.Instance.AddTile(2 + dx[i], 1 + dy[i], GetRandType(), i == 6);
            GameManager.Instance.AddTile(-1 + dx[i], 3 + dy[i], GetRandType(), i == 6);
            GameManager.Instance.AddTile(-3 + dx[i], 2 + dy[i], GetRandType(), i == 6);
            GameManager.Instance.AddTile(-2 + dx[i], -1 + dy[i], GetRandType(), i == 6);
            GameManager.Instance.AddTile(1 + dx[i], -3 + dy[i], GetRandType(), i == 6);
            GameManager.Instance.AddTile(3 + dx[i], -2 + dy[i], GetRandType(), i == 6);
        }
        foreach (var x in GameManager.Instance.tiles.Values)
            x.isEditable = false;
    }
}

[System.Serializable]
public class TeamPreset
{
    public string name;
    public List<int> players;
}
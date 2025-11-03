using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerConfig : MonoBehaviour
{
    public TMP_Dropdown typeDropdown;
    public TMP_Dropdown teamDropdown;
    public TMP_Text text;

    public PlayerConfigData data;
    public int id;
    private static readonly List<string> TypeOptions = new List<string>()
    {
        "人类",
        "均衡型AI",
        "进攻型AI",
        "防御型AI"
    };
    public void init()
    {
        typeDropdown.ClearOptions();
        typeDropdown.AddOptions(TypeOptions);
        typeDropdown.SetValueWithoutNotify((int)data.type);
        typeDropdown.onValueChanged.RemoveAllListeners();
        typeDropdown.onValueChanged.AddListener(OnTypeDropdownChanged);

        teamDropdown.ClearOptions();
        teamDropdown.AddOptions(StartManager.Instance.TeamOptions);
        teamDropdown.SetValueWithoutNotify((int)data.team-1);
        teamDropdown.onValueChanged.RemoveAllListeners();
        teamDropdown.onValueChanged.AddListener(OnTeamDropdownChanged);
    }

    public void UpdateTeamDropdown(){
        teamDropdown.ClearOptions();
        teamDropdown.AddOptions(StartManager.Instance.TeamOptions);
        teamDropdown.SetValueWithoutNotify((int)data.team-1);
        teamDropdown.onValueChanged.RemoveAllListeners();
        teamDropdown.onValueChanged.AddListener(OnTeamDropdownChanged);
    }

    void OnTypeDropdownChanged(int displayIndex)
    {
        StartManager.Instance.SetPlayerType
            (id, (PlayerType)displayIndex);
    }
    void OnTeamDropdownChanged(int displayIndex)
    {
        StartManager.Instance.SetPlayerTeam
            (id, 1 + displayIndex);
    }
    public void RemoveSelf()
    {
        StartManager.Instance.RemovePlayer(id);
    }
}


[System.Serializable]
public class PlayerConfigData
{
    public int team;
    public PlayerType type;
}
public enum PlayerType
{
    Human,
    AIBalanced,
    AIAttack,
    AIDefence,
}
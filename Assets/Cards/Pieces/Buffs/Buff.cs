using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff
{
    public int id;
    public int roundRemain;
    public string buffName;
    public string buffDescription;
    public Buff() { }
    public Buff(int i,int x, string s1, string s2)
    {
        id = i;
        roundRemain = x;
        buffName = s1;
        buffDescription = s2;
    }

    public Buff(Buff b)
    {
        this.id = b.id;
        this.roundRemain = b.roundRemain;
        this.buffDescription = b.buffDescription;
        this.buffName = b.buffName;
    }

    public string GetDescription()
    {
        string res = "\n【" + buffName + "】";
        if (roundRemain != -1) res += $"剩余{roundRemain}回合 ";
        return res + "\n" + buffDescription;
    }
}
public class Stun : Buff
{
    public Stun(int x=1) : base()
    {
        id = 1001;
        roundRemain = x;
        buffName = "禁锢";
        buffDescription = "无法行动。";
    }
}
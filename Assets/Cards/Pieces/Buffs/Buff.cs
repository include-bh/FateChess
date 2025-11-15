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
    public string GetDescription()
    {
        string res="\n【" + buffName + "】";
        if (roundRemain != -1) res += $"剩余{roundRemain}回合 ";
        return res+buffDescription;
    }
}
public class Stun : Buff
{
    public Stun() : base()
    {
        id = 1001;
        roundRemain = 1;
        buffName = "禁锢";
        buffDescription = "无法行动。";
    }
}
public class DealDamageModifier : Buff
{
    public int rate;
    public DealDamageModifier(int x) : base()
    {
        id = 1002;
        rate = x;
        roundRemain = -1;
        if (x > 0)
        {
            buffName = "造成伤害提高";
            buffDescription = $"造成伤害提高{x}。";
        }
        else
        {
            buffName = "造成伤害降低";
            buffDescription = $"造成伤害降低{-x}。";
        }
    }
    public DealDamageModifier() : this(0) { }
}
public class TakeDamageModifier : Buff
{
    public int rate;
    public TakeDamageModifier(int x) : base()
    {
        id = 1003;
        rate = x;
        roundRemain = -1;
        if (x > 0)
        {
            buffName = "受到伤害提高";
            buffDescription = $"受到伤害提高{x}。";
        }
        else
        {
            buffName = "受到伤害降低";
            buffDescription = $"受到伤害降低{-x}。";
        }
    }
    public TakeDamageModifier() : this(0) { }
}

public class StepModifier : Buff
{
    public int rate;
    public StepModifier(int x) : base()
    {
        id = 1004;
        rate = x;
        roundRemain = -1;
        if (x > 0)
        {
            buffName = "移动力提高";
            buffDescription = $"移动力提高{x}。";
        }
        else
        {
            buffName = "移动力降低";
            buffDescription = $"移动力降低{-x}。";
        }
    }
    public StepModifier() : this(0) { }
}
public class RangeModifier: Buff
{
    public int rate;
    public RangeModifier(int x) : base()
    {
        id = 1005;
        rate = x;
        roundRemain = -1;
        if (x > 0)
        {
            buffName = "攻击范围提高";
            buffDescription = $"攻击范围提高{x}。";
        }
        else
        {
            buffName = "攻击范围降低";
            buffDescription = $"攻击范围降低{-x}。";
        }
    }
    public RangeModifier():this (0){}
}
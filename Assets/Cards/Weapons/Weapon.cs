using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Card
{
    public Piece owner;
    public Weapon() : base()
    {
        owner = null;
    }
    public virtual void OnInstall()
    {

    }
    public virtual void OnRemove()
    {
        
    }
}

public class ExCalibur : Weapon
{
    public ExCalibur() : base()
    {
        cardName = "誓约胜利之剑";
        cardDescription = "*仅Saber装备时生效\n造成的伤害+2。";
        sprite = GameManager.Instance.weaponAtlas.GetSprite("ExCalibur");
    }
    public int modifier(int dmg, Piece atk, Piece def)
    {
        return dmg + 2;
    }
    public override void OnInstall()
    {
    }
    public override void OnRemove()
    {
    }
}

public class Avalon : Weapon
{
    public Avalon() : base()
    {
        cardName = "遥远的理想乡";
        cardDescription = "*仅Saber装备时生效\n己方单位受到伤害-1。";
        sprite = GameManager.Instance.weaponAtlas.GetSprite("Avalon");
    }
    public int modifier(int dmg, Piece atk, Piece def)
    {
        if (def.player == owner.player) --dmg;
        return dmg;
    }
    public override void OnInstall()
    {
        if (owner is Saber)
        {
        }
    }
    public override void OnRemove()
    {
        if (owner is Saber)
        {
        }
    }
}

public class UBW : Weapon
{
    public UBW() : base()
    {
        cardName = "无限剑制";
        cardDescription = "*仅Archer装备时生效\n己方单位造成伤害+1。";
        sprite = GameManager.Instance.weaponAtlas.GetSprite("UBW");
    }
    public int modifier(int dmg, Piece atk, Piece def)
    {
        if (atk.player == owner.player) ++dmg;
        return dmg;
    }
    public override void OnInstall()
    {
        if (owner is Archer)
        {
        }
    }
    public override void OnRemove()
    {
        if (owner is Archer)
        {
        }
    }
}

public class GaeBolg : Weapon
{
    public GaeBolg() : base()
    {
        cardName = "穿刺死棘之枪";
        cardDescription = "*仅Lancer装备时生效\n攻击范围+1，攻击非相邻目标时造成伤害+1。";
        sprite = GameManager.Instance.weaponAtlas.GetSprite("GaeBolg");
    }
    public int modifier(int dmg, Piece atk, Piece def)
    {
        return dmg;
    }
    public override void OnInstall()
    {
    }
    public override void OnRemove()
    {
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Servant : Piece,ICanOnLoad
{
    public Weapon equip;
    public Servant():base()
    {
        equip = null;
    }
    public override void OnDeath()
    {
        if (equip != null)
        {
            GameManager.Instance.DiscardCard(equip);
            equip = null;
        }
        base.OnDeath();
    }
    public override string GetDescription()
    {
        string res = base.GetDescription();
        if (equip != null) res += "\n" + equip.GetDescription();
        return res;
    }
    
}

public class Saber : Servant
{
    public Saber():base()
    {
        maxHP = HP = 7;
        AT = 3;
        maxDF = DF = 3;
        RA = 1;
        ST = 1;
        canClimb = false;canSwim = false;canBanMagic = true;canRide = false;
        cardName = "Saber";
        cardDescription = "能够承受大量伤害的从者。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Saber");
    }
}
public class Lancer : Servant
{
    public Lancer():base()
    {
        maxHP = HP = 6;
        AT = 4;
        maxDF = DF = 2;
        RA = 1;
        ST = 2;
        canClimb = false;canSwim = true;canBanMagic = true;canRide = false;
        cardName = "Lancer";
        cardDescription = "擅长近战单挑的从者。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Lancer");
    }
}
public class Archer : Servant
{
    public Archer():base()
    {
        maxHP = HP = 6;
        AT = 3;
        maxDF = DF = 2;
        RA = 3;
        ST = 2;
        canClimb = true;canSwim = false;canBanMagic = true;canRide = false;
        cardName = "Archer";
        cardDescription = "能够远程攻击的从者。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Archer");
    }
}
public class Caster : Servant
{
    public Caster():base()
    {
        maxHP = HP = 6;
        AT = 2;
        maxDF = DF = 2;
        RA = 2;
        ST = 1;
        canClimb = false;canSwim = false;canBanMagic = false;canRide = false;
        cardName = "Caster";
        cardDescription = "进行连锁法术攻击的从者。\n攻击时，可以指定至多3个攻击目标。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Caster");
    }
    public override async Task Attack()
    {
        canHit.Clear();
        vis.Clear();
        dfsHit(RA, xpos, ypos);
        for(int i = 0; i < 3; i++)
        {
            Piece e = await player.SelectTarget(new List<Piece>(canHit));
            if (e != null) DealDamage(e, AT);
            else break;
            if (e.canBanMagic) break;
            if (i != 2)
            {
                vis.Clear();
                dfsHit(RA, e.xpos, e.ypos);
            }
        }
    }
}
public class Rider : Servant
{
    public Rider():base()
    {
        maxHP = HP = 7;
        AT = 3;
        maxDF = DF = 2;
        RA = 1;
        ST = 3;
        canClimb = false;canSwim = true;canBanMagic = false;canRide = true;
        cardName = "Rider";
        cardDescription = "移动迅速，可以驾驶载具的从者。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Rider");
    }
}
public class Assassin : Servant
{
    public Assassin():base()
    {
        maxHP = HP = 5;
        AT = 4;
        maxDF = DF = 1;
        RA = 1;
        ST = 3;
        canClimb = true; canSwim = false; canBanMagic = false;canRide = false;
        cardName = "Assassin";
        cardDescription = "通过背刺造成大量伤害的从者。\n【刺杀】攻击时，若攻击目标后方，额外造成2点伤害。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Assassin");
    }
    public override void DealDamage(Piece e, int dmg, bool isPierce)
    {
        EventManager.TriggerOnAttack(this, e);

        if (((e.xpos - xpos == dx[e.facing]) && (e.ypos - ypos == dy[e.facing]))
         || ((e.xpos - xpos == dx[(e.facing + 1) % 6]) && (e.ypos - ypos == dy[(e.facing + 1) % 6])))
            e.TakeDamage(this, dmg + 2, isPierce);
        else e.TakeDamage(this, dmg, isPierce);
    }
}
public class Berserker : Servant
{
    public Berserker():base()
    {
        maxHP = HP = 5;
        AT = 5;
        maxDF = DF = 1;
        RA = 1;
        ST = 2;
        canClimb = false;canSwim = false;canBanMagic = false;canRide = false;
        cardName = "Berserker";
        cardDescription = "能够造成大量伤害，但生存能力较低的从者。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Berserker");
    }
}
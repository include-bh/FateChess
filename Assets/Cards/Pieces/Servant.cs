using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Servant : Piece,ICanOnLoad
{
    public Weapon equip;
    public Servant() : base()
    {
        equip = null;
    }
    public override void InitCard()
    {
        base.InitCard();
        equip = null;
    }
    public override async UniTask OnDeath()
    {
        if (equip != null)
        {
            equip.OnRemove();
            equip = null;
        }
        await base.OnDeath();
    }
    public override string GetDescription()
    {
        string res = base.GetDescription();
        if (equip != null) res += "\n宝具：【" + equip.cardName + "】\n" + equip.GetDescription();
        return res;
    }
    
}

public class Saber : Servant
{
    public Saber() : base()
    {
        maxHP = HP = 7;
        AT = 3;
        maxDF = DF = 3;
        RA = 1;
        ST = 1;
        canClimb = 0; canSwim = 0; canBanMagic = 1; canRide = 0;
        cardName = "Saber";
        cardDescription = "能够承受大量伤害的从者。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Saber");
    }
    public override void InitCard()
    {
        base.InitCard();
        maxHP = HP = 7;
        AT = 3;
        maxDF = DF = 3;
        RA = 1;
        ST = 1;
        canClimb = 0; canSwim = 0; canBanMagic = 1; canRide = 0;
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
        canClimb = 0;canSwim = 1;canBanMagic = 1;canRide = 0;
        cardName = "Lancer";
        cardDescription = "擅长近战单挑的从者。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Lancer");
    }
    public override void InitCard()
    {
        base.InitCard();
        maxHP = HP = 6;
        AT = 4;
        maxDF = DF = 2;
        RA = 1;
        ST = 2;
        canClimb = 0;canSwim = 1;canBanMagic = 1;canRide = 0;
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
        canClimb = 1;canSwim = 0;canBanMagic = 1;canRide = 0;
        cardName = "Archer";
        cardDescription = "能够远程攻击的从者。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Archer");
    }
    public override void InitCard()
    {
        base.InitCard();
        maxHP = HP = 6;
        AT = 3;
        maxDF = DF = 2;
        RA = 3;
        ST = 2;
        canClimb = 1;canSwim = 0;canBanMagic = 1;canRide = 0;
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
        canClimb = 0;canSwim = 0;canBanMagic = 0;canRide = 0;
        cardName = "Caster";
        cardDescription = "进行连锁法术攻击的从者。\n攻击时，可以指定至多3个攻击目标。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Caster");
    }
    public override void InitCard()
    {
        base.InitCard();
        maxHP = HP = 6;
        AT = 2;
        maxDF = DF = 2;
        RA = 2;
        ST = 1;
        canClimb = 0;canSwim = 0;canBanMagic = 0;canRide = 0;
    }
    public override async UniTask Attack()
    {
        canHit.Clear();
        getHit(RA, xpos, ypos);
        for(int i = 0; i < 3; i++)
        {
            Piece e = await player.SelectTarget(new List<Piece>(canHit));
            if (e != null)
            {
                DealDamage(e, AT);
                if (e.status == CardStatus.InPile) canHit.Remove(e);
            }
            else break;
            if (equip is not RuleBreaker && e.canBanMagic > 0) break;
            if (i != 2)
            {
                getHit(RA, e.xpos, e.ypos);
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
        canClimb = 0;canSwim = 1;canBanMagic = 0;canRide = 1;
        cardName = "Rider";
        cardDescription = "移动迅速，可以驾驶载具的从者。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Rider");
    }
    public override void InitCard()
    {
        base.InitCard();
        maxHP = HP = 7;
        AT = 3;
        maxDF = DF = 2;
        RA = 1;
        ST = 3;
        canClimb = 0;canSwim = 1;canBanMagic = 0;canRide = 1;
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
        canClimb = 1; canSwim = 0; canBanMagic = 0;canRide = 0;
        cardName = "Assassin";
        cardDescription = "通过背刺造成大量伤害的从者。\n【刺杀】攻击时，若攻击目标后方，额外造成2点伤害。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Assassin");
    }
    public override void InitCard()
    {
        base.InitCard();
        maxHP = HP = 5;
        AT = 4;
        maxDF = DF = 1;
        RA = 1;
        ST = 3;
        canClimb = 1; canSwim = 0; canBanMagic = 0;canRide = 0;
    }
    public override void DealDamage(Piece e, int dmg, bool isPierce)
    {
        EventManager.TriggerOnAttack(this, e);

        if (DealDamageModifier != null)
            foreach (DamageModifier modifier in DealDamageModifier.GetInvocationList())
                dmg = modifier(dmg, this, e);
            
        GameObject go = GameObject.Instantiate(GameManager.Instance.AttackEffectPrefab);
        AttackEffect ef = go.GetComponent<AttackEffect>();
        ef.start = GameManager.GetPosition(xpos, ypos);
        ef.end = GameManager.GetPosition(e.xpos, e.ypos);
        ef.PlayAnimation();

        if (e.load == null && (((e.xpos - xpos == dx[e.facing]) && (e.ypos - ypos == dy[e.facing]))
         || ((e.xpos - xpos == dx[(e.facing + 1) % 6]) && (e.ypos - ypos == dy[(e.facing + 1) % 6]))))
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
        canClimb = 0;canSwim = 0;canBanMagic = 0;canRide = 0;
        cardName = "Berserker";
        cardDescription = "能够造成大量伤害，但生存能力较低的从者。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Berserker");
    }
    public override void InitCard()
    {
        base.InitCard();
        maxHP = HP = 5;
        AT = 5;
        maxDF = DF = 1;
        RA = 1;
        ST = 2;
        canClimb = 0; canSwim = 0; canBanMagic = 0; canRide = 0;
    }

    public override void TakeDamage(Piece e, int dmg, bool isPierce = false)
    {
        if (status != CardStatus.OnBoard) return;
        if (e == null && GameManager.Instance.curPlayer == player) dmg /= 3;
        if(TakeDamageModifier!=null)
        foreach (DamageModifier modifier in TakeDamageModifier.GetInvocationList())
            dmg = modifier(dmg, e, this);
        if (dmg <= 0) return;


        if (renderer is BoardPieceRenderer bprend) bprend.TakeDamageAnimation();
        if (!isPierce)
        {
            if (DF > dmg)
            {
                DF -= dmg;
                return;
            }
            else
            {
                dmg -= DF;
                DF = 0;
                EventManager.TriggerOnBreak(e, this);
            }
        }
        if (HP > dmg)
        {
            HP -= dmg;
            pieceRenderer.UpdateData();
        }
        else
        {
            if (equip is GodHand equ && equ.state == 0)
            {
                HP = 1;
                equ.Trigger();
                pieceRenderer.UpdateData();
            }
            else
            {
                HP = 0;
                EventManager.TriggerOnKill(e, this);
                pieceRenderer.UpdateData();
                OnDeath();
            }
        }
    }
    
    public override void OnTurnBegin()
    {
        base.OnTurnBegin();
        if (equip is GodHand equ && equ.state == 1)
        {
            equip.OnRemove();
            equip = null;
        }
    }
}
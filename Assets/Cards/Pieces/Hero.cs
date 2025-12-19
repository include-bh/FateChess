using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Hero : Piece, ICanOnLoad, ICanBanMove
{
    public override async UniTask<bool> UseCard(Player usr)
    {
        player = usr;
        status = CardStatus.OnBoard;
        DealDamageModifier = usr.DealDamageModifier;
        TakeDamageModifier = usr.TakeDamageModifier;
        buffs = new List<Buff>(usr.buffs);
        usr.onBoardList.Add(this);
        
        GameObject go = GameObject.Instantiate(GameManager.Instance.BoardPiecePrefab);
        renderer = go.GetComponent<BoardPieceRenderer>();
        renderer.data = this;
        renderer.InitSprite();

        UpdateOnBoardPosition();

        return true;
    }
    public override async UniTask OnDeath()
    {
        if (tile != null) tile.onTile = null;
        if (load != null && this is ICanOnLoad ic) load.onLoad.Remove(ic);
        player.onBoardList.Remove(this);
        if (renderer != null && renderer is BoardRenderer brend)
            await brend.LeaveAnimation();
        if (renderer != null)
        {
            GameObject.Destroy(renderer.gameObject);
            renderer = null;
        }
    }
}
public class Arthuria : Hero
{
    public Arthuria() : base()
    {
        maxHP = HP = 12;
        AT = 7;
        maxDF = DF = 4;
        RA = 1;
        ST = 1;
        canClimb = 0; canSwim = 0; canBanMagic = 1; canRide = 1;
        cardName = "阿尔托莉雅·潘德拉贡";
        cardDescription = "古不列颠传说中的亚瑟王，在圣杯战争中曾多次毁掉圣杯。\n在场时，己方单位受到伤害-1。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Arthuria");
    }
    public override void InitCard()
    {
        base.InitCard();
        maxHP = HP = 12;
        AT = 7;
        maxDF = DF = 4;
        RA = 1;
        ST = 1;
        canClimb = 0; canSwim = 0; canBanMagic = 1; canRide = 1;
    }
    
    Buff buff = new Buff(3001,-1, "阿尔托莉雅·潘德拉贡", "受到伤害-1。");
    public int modifier(int dmg, Piece atk, Piece def)
    {
        if (def.player == this.player) --dmg;
        return dmg;
    }
    public override async UniTask<bool> UseCard(Player usr)
    {
        await base.UseCard(usr);
        player.TakeDamageModifier += modifier;
        player.buffs.Add(new Buff(buff));
        foreach (Piece x in player.onBoardList)
        {
            x.TakeDamageModifier += modifier;
            x.buffs.Add(new Buff(buff));
        }
        return true;
    }
    public override async UniTask OnDeath()
    {
        player.TakeDamageModifier -= modifier;
        player.buffs.RemoveAll(buf => buf.id == buff.id);
        foreach (Piece x in player.onBoardList)
        {
            x.TakeDamageModifier -= modifier;
            x.buffs.RemoveAll(buf => buf.id == buff.id);
        }
        await base.OnDeath();
    }
}
public class Include : Hero
{
    public Include():base()
    {
        maxHP = HP = 10;
        AT = 6;
        maxDF = DF = 3;
        RA = 2;
        ST = 2;
        canClimb = 1;canSwim = 1;canBanMagic = 1;canRide = 0;
        cardName = "包涵·溯时之枪";
        cardDescription = "作者以男身被召唤的英灵，似乎拥有掌控时间的力量。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Include");
    }
    public override void InitCard()
    {
        base.InitCard();
        maxHP = HP = 10;
        AT = 6;
        maxDF = DF = 3;
        RA = 2;
        ST = 2;
        canClimb = 1; canSwim = 1; canBanMagic = 1; canRide = 0;
    }
    Buff buff = new Buff(2008,-1, "嗜血", "击败敌人后可再次行动，不可连续触发。");
    int state = 0;
    public void modifier(Piece atk, Piece def)
    {
        if (atk == this && state == 0)
        {
            state = 1;
            canAct = true;
            TakeAction();
            state = 0;
        }
    }
    
    public override async UniTask<bool> UseCard(Player usr)
    {
        await base.UseCard(usr);
        EventManager.OnKill += modifier;
        buffs.Add(new Buff(buff));
        return true;
    }
    public override async UniTask OnDeath()
    {
        EventManager.OnKill -= modifier;
        await base.OnDeath();
    }
}
public class Sunsettia : Hero
{
    public Sunsettia():base()
    {
        maxHP = HP = 10;
        AT = 5;
        maxDF = DF = 3;
        RA = 4;
        ST = 2;
        canClimb = 1;canSwim = 1;canBanMagic = 1;canRide = 0;
        cardName = "春迟日落·裂空之箭";
        cardDescription = "作者以女身被召唤的英灵，似乎拥有撕裂空间的力量。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Sunsettia");
    }
    public override void InitCard()
    {
        base.InitCard();
        maxHP = HP = 10;
        AT = 5;
        maxDF = DF = 3;
        RA = 4;
        ST = 2;
        canClimb = 1;canSwim = 1;canBanMagic = 1;canRide = 0;
    }
    Buff buff = new Buff(2008,-1, "嗜血", "击败敌人后可再次行动，不可连续触发。");
    int state = 0;
    public void modifier(Piece atk, Piece def)
    {
        if (atk == this && state == 0)
        {
            state = 1;
            canAct = true;
            TakeAction();
            state = 0;
        }
    }
    public override async UniTask<bool> UseCard(Player usr)
    {
        await base.UseCard(usr);
        EventManager.OnKill += modifier;
        buffs.Add(new Buff(buff));
        return true;
    }
    public override async UniTask OnDeath()
    {
        EventManager.OnKill -= modifier;
        await base.OnDeath();
    }
}

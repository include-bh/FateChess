using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Weapon : Card
{
    public Piece owner;
    public Weapon() : base()
    {
        owner = null;
    }
    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;

        var tar = await usr.SelectTarget(
            usr.onBoardList
            .Where(p => p is Servant)
            .ToList()
        ) as Servant;

        Debug.Log(tar);

        if (tar == null) return false;

        --usr.CommandCount;
        if (tar.equip != null)
        {
            tar.equip.OnRemove();
            tar.equip = null;
        }

        tar.equip = this;
        owner = tar;
        OnInstall();
        return true;
    }
    public virtual void OnInstall()
    {
        status = CardStatus.OnBoard;
    }
    public virtual void OnRemove()
    {
        owner = null;
        GameManager.Instance.DiscardCard(this);
    }
}

public class ExCalibur : Weapon
{
    public ExCalibur() : base()
    {
        cardName = "誓约胜利之剑";
        cardDescription = "*仅Saber装备时生效\n攻击力+2。";
        sprite = GameManager.Instance.weaponAtlas.GetSprite("ExCalibur");
    }
    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;

        var tar = await usr.SelectTarget(
            usr.onBoardList
            .Where(p => p is Servant)
            .ToList()
        ) as Servant;

        if (tar == null) return false;

        --usr.CommandCount;
        
        if (tar is Saber && tar.equip is Avalon)
        {
            GameManager.Instance.Upgrade(tar,new Arthuria());
            GameManager.Instance.DiscardCard(this);
            return true;
        }
        
        if (tar.equip != null)
        {
            tar.equip.OnRemove();
            tar.equip = null;
        }

        tar.equip = this;
        owner = tar;
        OnInstall();
        return true;
    }

    public override void OnInstall()
    {
        base.OnInstall();
        if (owner is Saber)
        {
            owner.AT += 2;
            owner.pieceRenderer.UpdateData();
        }
    }
    public override void OnRemove()
    {
        if (owner is Saber)
        {
            owner.AT -= 2;
            owner.pieceRenderer.UpdateData();
        }
        base.OnRemove();
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
    Buff buff = new Buff(2001, -1, "遥远的理想乡", "受到伤害-1。");

    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;

        var tar = await usr.SelectTarget(
            usr.onBoardList
            .Where(p => p is Servant)
            .ToList()
        ) as Servant;

        if (tar == null) return false;

        --usr.CommandCount;

        if (tar is Saber && tar.equip is ExCalibur)
        {
            GameManager.Instance.Upgrade(tar, new Arthuria());
            GameManager.Instance.DiscardCard(this);
            return true;
        }

        if (tar.equip != null)
        {
            tar.equip.OnRemove();
            tar.equip = null;
        }

        tar.equip = this;
        owner = tar;
        OnInstall();
        return true;
    }
    
    public override void OnInstall()
    {
        base.OnInstall();
        if (owner is Saber)
        {
            owner.player.TakeDamageModifier += modifier;
            owner.player.buffs.Add(new Buff(buff));
            foreach (Piece x in owner.player.onBoardList)
            {
                x.TakeDamageModifier += modifier;
                x.buffs.Add(new Buff(buff));
            }
        }
    }
    public override void OnRemove()
    {
        if (owner is Saber)
        {
            owner.player.TakeDamageModifier -= modifier;
            owner.player.buffs.RemoveAll(buf => buf.id == buff.id);
            foreach (Piece x in owner.player.onBoardList)
            {
                x.TakeDamageModifier -= modifier;
                x.buffs.RemoveAll(buf => buf.id == buff.id);
            }
        }
        base.OnRemove();
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
    Buff buff = new Buff(2002,-1, "无限剑制", "造成伤害+1。");
    public int modifier(int dmg, Piece atk, Piece def)
    {
        if (atk.player == owner.player) ++dmg;
        return dmg;
    }
    public override void OnInstall()
    {
        base.OnInstall();
        if (owner is Archer)
        {
            owner.player.DealDamageModifier += modifier;
            owner.player.buffs.Add(new Buff(buff));
            foreach (Piece x in owner.player.onBoardList)
            {
                x.DealDamageModifier += modifier;
                x.buffs.Add(new Buff(buff));
            }
        }
    }
    public override void OnRemove()
    {
        if (owner is Archer)
        {
            owner.player.DealDamageModifier -= modifier;
            owner.player.buffs.RemoveAll(buf => buf.id == buff.id);
            foreach (Piece x in owner.player.onBoardList)
            {
                x.DealDamageModifier -= modifier;
                x.buffs.RemoveAll(buf => buf.id == buff.id);
            }
        }
        base.OnRemove();
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
    Buff buff = new Buff(2003,-1, "穿刺死棘之枪", "攻击非相邻目标时造成伤害+1。");
    public int modifier(int dmg, Piece atk, Piece def)
    {
        for (int i = 0; i < 6; i++)
            if (atk.xpos + GameManager.dx[i] == def.xpos && atk.ypos + GameManager.dy[i] == def.ypos)
                return dmg;
        return dmg + 1;
    }
    
    public override void OnInstall()
    {
        base.OnInstall();
        if(owner is Lancer)
        {
            owner.DealDamageModifier += modifier;
            owner.RA += 1;
            owner.buffs.Add(new Buff(buff));
            owner.pieceRenderer.UpdateData();
        }
    }
    
    public override void OnRemove()
    {
        if(owner is Lancer)
        {
            owner.DealDamageModifier -= modifier;
            owner.RA -= 1;
            owner.buffs.RemoveAll(buf => buf.id == buff.id);
            owner.pieceRenderer.UpdateData();
        }
        base.OnRemove();
    }
}

public class BloodFort : Weapon
{
    public BloodFort() : base()
    {
        cardName = "鲜血神殿";
        cardDescription = "*仅Rider装备时生效\n己方单位击败敌人时，回复3点生命。";
        sprite = GameManager.Instance.weaponAtlas.GetSprite("BloodFort");
    }
    Buff buff = new Buff(2004,-1, "鲜血神殿", "击败敌人时回复3点生命。");
    public void modifier(Piece atk,Piece def)
    {
        if (atk.player == owner.player)
            atk.HP = Math.Min(atk.HP + 3, atk.maxHP);
    }
    public override void OnInstall()
    {
        base.OnInstall();
        if (owner is Rider)
        {
            EventManager.OnKill += modifier;
            owner.player.buffs.Add(new Buff(buff));
            foreach (Piece x in owner.player.onBoardList)
                x.buffs.Add(new Buff(buff));
        }
    }
    public override void OnRemove()
    {
        if(owner is Rider)
        {
            EventManager.OnKill -= modifier;
            owner.player.buffs.RemoveAll(buf => buf.id == buff.id);
            foreach (Piece x in owner.player.onBoardList)
                x.buffs.RemoveAll(buf => buf.id == buff.id);
        }
        base.OnRemove();
    }
}
public class RuleBreaker : Weapon
{
    public RuleBreaker() : base()
    {
        cardName = "破除万法之符";
        cardDescription = "*仅Caster装备时生效\n攻击时无视【对魔力】。";
        sprite = GameManager.Instance.weaponAtlas.GetSprite("RuleBreaker");
    }
    
    public override void OnInstall()
    {
        base.OnInstall();
    }
    
    public override void OnRemove()
    {
        base.OnRemove();
    }
}
public class GodHand : Weapon
{
    public GodHand() : base()
    {
        cardName = "十二试炼";
        cardDescription = "*仅Berserker装备时生效\n受到致命伤害时，保留1点生命，回合内不会受到伤害。";
        sprite = GameManager.Instance.weaponAtlas.GetSprite("GodHand");
    }

    public int state = 0;
    Buff buff = new Buff(2005,1, "十二试炼", "不会受到伤害。");
    
    public int modifier(int dmg, Piece atk, Piece def)
    {
        return 0;
    }
    public override void OnInstall()
    {
        base.OnInstall();
        state = 0;
    }
    public void Trigger()
    {
        if(owner is Berserker)
        {
            state = 1;
            owner.TakeDamageModifier += modifier;
            owner.buffs.Add(new Buff(buff));
        }
    }
    public override void OnRemove()
    {
        if(owner is Berserker)
        {
            owner.buffs.RemoveAll(buf => buf.id == buff.id);
            if (state == 1) owner.TakeDamageModifier -= modifier;
        }
        base.OnRemove();
    }
}
public class Zabaniya : Weapon
{
    public Zabaniya() : base()
    {
        cardName = "诅咒之手";
        cardDescription = "*仅Assassin装备时生效\n移动力+1。获得【嗜血】：击败敌人后可再次行动。";
        sprite = GameManager.Instance.weaponAtlas.GetSprite("Zabaniya");
    }

    Buff buff = new Buff(1002, -1, "嗜血", "击败敌人后可再次行动。");
    public void modifier(Piece atk, Piece def)
    {
        if (atk == owner)
            owner.canAct = true;
    }

    
    public override void OnInstall()
    {
        base.OnInstall();
        if(owner is Assassin)
        {
            EventManager.OnKill += modifier;
            owner.ST += 1;
            owner.buffs.Add(new Buff(buff));
            owner.pieceRenderer.UpdateData();
        }
    }
    
    public override void OnRemove()
    {
        if(owner is Assassin)
        {
            EventManager.OnKill -= modifier;
            owner.ST -= 1;
            owner.buffs.RemoveAll(buf => buf.id == buff.id);
            owner.pieceRenderer.UpdateData();
        }
        base.OnRemove();
    }
}
public class TrailBat : Weapon
{
    public TrailBat() : base()
    {
        cardName = "开拓者的球棒";
        cardDescription = "攻击力+1。击破敌人时，回复2生命。";
        sprite = GameManager.Instance.weaponAtlas.GetSprite("TrailBat");
    }
    Buff buff = new Buff(2006,-1, "开拓者的球棒", "击破敌人时，回复2生命。");
    public void modifier(Piece atk,Piece def)
    {
        if (atk == owner)
            atk.HP = Math.Min(atk.HP + 2, atk.maxHP);
    }
    
    
    public override void OnInstall()
    {
        base.OnInstall();
        EventManager.OnBreak += modifier;
        owner.buffs.Add(new Buff(buff));
        owner.AT += 1;
        owner.pieceRenderer.UpdateData();
    }
    
    public override void OnRemove()
    {
        EventManager.OnBreak -= modifier;
        owner.buffs.RemoveAll(buf => buf.id == buff.id);
        owner.AT -= 1;
        owner.pieceRenderer.UpdateData();
        base.OnRemove();
    }
}
public class PyroLance : Weapon
{
    public PyroLance() : base()
    {
        cardName = "筑城者的骑枪";
        cardDescription = "防御力上限+1。攻击时，回复1生命。";
        sprite = GameManager.Instance.weaponAtlas.GetSprite("PyroLance");
    }

    Buff buff = new Buff(2007,-1, "筑城者的骑枪", "攻击时，回复1生命。");
    
    public int modifier(int dmg,Piece atk,Piece def)
    {
        owner.HP = Math.Min(owner.HP + 1, owner.maxHP);
        return dmg;
    }
    
    public override void OnInstall()
    {
        base.OnInstall();
        owner.maxDF += 1;
        owner.DF += 1;
        owner.buffs.Add(new Buff(buff));
        owner.pieceRenderer.UpdateData();
        owner.DealDamageModifier += modifier;
    }
    
    public override void OnRemove()
    {
        owner.maxDF -= 1;
        owner.DF = Math.Min(owner.DF, owner.maxDF);
        owner.buffs.RemoveAll(buf => buf.id == buff.id);
        owner.pieceRenderer.UpdateData();
        owner.DealDamageModifier -= modifier;
        base.OnRemove();
    }
}
public class OverbreakHat : Weapon
{
    public OverbreakHat() : base()
    {
        cardName = "钟表匠的礼帽";
        cardDescription = "击破敌人时，敌人受到等同于其防御力上限点数的伤害。";
        sprite = GameManager.Instance.weaponAtlas.GetSprite("OverbreakHat");
    }

    public void modifier(Piece atk, Piece def)
    {
        if (atk == owner)
            def.TakeDamage(null,def.maxDF);
    }
    public override void OnInstall()
    {
        base.OnInstall();
        EventManager.OnBreak += modifier;
    }
    
    public override void OnRemove()
    {
        EventManager.OnBreak -= modifier;
        base.OnRemove();
    }
}
public class MemePen : Weapon
{
    public MemePen() : base()
    {
        cardName = "著者的羽毛笔";
        cardDescription = "造成的伤害为穿透伤害。";
        sprite = GameManager.Instance.weaponAtlas.GetSprite("MemePen");
    }

    public override void OnInstall()
    {
        base.OnInstall();
        owner.isPierce = true;
    }
    
    public override void OnRemove()
    {
        owner.isPierce = false;
        base.OnRemove();
    }
}
public class CludeSpear : Weapon
{
    public CludeSpear() : base()
    {
        cardName = "溯时之枪";
        cardDescription = "获得【嗜血】：击败敌人后可再次行动。";
        sprite = GameManager.Instance.weaponAtlas.GetSprite("CludeSpear");
    }
    Buff buff = new Buff(2008,-1, "嗜血", "击败敌人后可再次行动。");
    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;

        var tar = await usr.SelectTarget(
            usr.onBoardList
            .Where(p => p is Servant)
            .ToList()
        ) as Servant;

        if (tar == null) return false;

        --usr.CommandCount;
        Archer tar2;
        if (tar is Lancer && (tar2 = usr.onBoardList.OfType<Archer>().FirstOrDefault(s => s.equip is SunsetBow)) !=null)
        {
            GameManager.Instance.Upgrade(tar, new Include());
            GameManager.Instance.Upgrade(tar2, new Sunsettia());
            GameManager.Instance.DiscardCard(this);
            return true;
        }
        if (tar.equip != null)
        {
            tar.equip.OnRemove();
            tar.equip = null;
        }

        tar.equip = this;
        owner = tar;
        OnInstall();
        return true;
    }
    
    public void modifier(Piece atk, Piece def)
    {
        if (atk == owner)
            owner.canAct = true;
    }
    
    public override void OnInstall()
    {
        base.OnInstall();
        EventManager.OnKill += modifier;
        owner.buffs.Add(new Buff(buff));
        owner.pieceRenderer.UpdateData();
    }
    public override void OnRemove()
    {
        EventManager.OnKill -= modifier;
        owner.buffs.RemoveAll(buf => buf.id == buff.id);
        owner.pieceRenderer.UpdateData();
        base.OnInstall();
    }
}
public class SunsetBow : Weapon
{
    public SunsetBow() : base()
    {
        cardName = "裂空之箭";
        cardDescription = "攻击范围+1。获得【攀爬】【涉水】。";
        sprite = GameManager.Instance.weaponAtlas.GetSprite("SunsetBow");
    }
    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;

        var tar = await usr.SelectTarget(
            usr.onBoardList
            .Where(p => p is Servant)
            .ToList()
        ) as Servant;

        if (tar == null) return false;

        --usr.CommandCount;
        Lancer tar2;
        if (tar is Archer && (tar2 = usr.onBoardList.OfType<Lancer>().FirstOrDefault(s => s.equip is CludeSpear)) !=null)
        {
            GameManager.Instance.Upgrade(tar2, new Include());
            GameManager.Instance.Upgrade(tar, new Sunsettia());
            GameManager.Instance.DiscardCard(this);
            return true;
        }
        if (tar.equip != null)
        {
            tar.equip.OnRemove();
            tar.equip = null;
        }

        tar.equip = this;
        owner = tar;
        OnInstall();
        return true;
    }

    public override void OnInstall()
    {
        base.OnInstall();
        ++owner.canClimb;
        ++owner.canSwim;
        owner.RA += 1;
        owner.pieceRenderer.UpdateData();
    }
    
    public override void OnRemove()
    {
        --owner.canClimb;
        --owner.canSwim;
        owner.RA -= 1;
        owner.pieceRenderer.UpdateData();
        base.OnRemove();
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using static GameManager;

public class Skill : Card
{
    public virtual void ShowTarget()
    {
        
    }
    public virtual void RemoveShowTarget()
    {
        
    }
}


public class TuXi : Skill
{
    Piece tar;
    int tarx, tary, tarf;
    public TuXi() : base()
    {
        cardName = "奇兵突袭";
        cardDescription = "使任意己方非Master、傀儡单位移动至任意空格，并可再次行动。";
        sprite = GameManager.Instance.skillAtlas.GetSprite("TuXi");
    }

    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;
        List<Piece> buf1 = usr.onBoardList
            .Where(p => (p is not UnitBase))
            .ToList();
        if (buf1.Count == 0) return false;

        tar = await usr.SelectTarget(buf1);
        if (tar == null) return false;

        --usr.CommandCount;
    
        List<(int, int)> buf = GameManager.Instance.tiles
            .Where(pii => pii.Value.onTile == null)
            .Select(pii => pii.Key)
            .ToList();
        if (tar.tile.onTile == tar) buf.Add((tar.xpos, tar.ypos));

        (tarx, tary) = await usr.SelectPosition(buf);
        tarf = await player.SelectDirection(tarx,tary);

        if (!await GameManager.Instance.AskForWuXie(this,usr))
        {
            GameManager.Instance.DiscardCard(this);
            return true;
        }

        tar.xpos = tarx; tar.ypos = tary;
        tar.facing = tarf;
        tar.UpdateOnBoardPosition();
        tar.pieceRenderer.UpdatePosition();
        tar.pieceRenderer.UpdateRotation();

        tar.canAct = true;

        GameManager.Instance.DiscardCard(this);
        return true;
    }
}
public class ZhuanYi : Skill
{
    Piece tar;
    int tarx, tary, tarf;
    public ZhuanYi() : base()
    {
        cardName = "转移阵地";
        cardDescription = "使任意己方Master或傀儡移动至多3格，并可再次行动。";
        sprite = GameManager.Instance.skillAtlas.GetSprite("ZhuanYi");
    }
    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;

        List<Piece> buf1 = usr.onBoardList
            .Where(p => (p is UnitBase))
            .ToList();
        if (buf1.Count == 0) return false;

        tar = await usr.SelectTarget(buf1);
        if (tar == null) return false;
        
        --usr.CommandCount;

        tar.canGoTo.Clear();
        tar.getMove(3, tar.xpos, tar.ypos);

        (tarx,tary) = await usr.SelectPosition(new List<(int, int)>(tar.canGoTo));
        tarf = await usr.SelectDirection(tarx,tary);
        
        if (!await GameManager.Instance.AskForWuXie(this,usr))
        {
            GameManager.Instance.DiscardCard(this);
            return true;
        }
        
        tar.xpos = tarx;tar.ypos = tary;
        tar.facing = tarf;
        tar.UpdateOnBoardPosition();
        tar.pieceRenderer?.UpdatePosition();
        tar.pieceRenderer?.UpdateRotation();

        if (tar is LoadAble ve)
            foreach (Piece p in ve.onLoad.OfType<Piece>())
                (p.xpos, p.ypos) = (ve.xpos, ve.ypos);

        tar.canAct = true;

        GameManager.Instance.DiscardCard(this);
        return true;
    }
}
public class CeFan : Skill
{
    Piece tar;
    public CeFan() : base()
    {
        cardName = "策反";
        cardDescription = "使任意非己方单位视作己方单位，由己方操作行动一次。";
        sprite = GameManager.Instance.skillAtlas.GetSprite("CeFan");
    }
    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;

        List<Piece> buf = new List<Piece>();
        foreach (Tile t in GameManager.Instance.tiles.Values) if (t.onTile != null && t.onTile.player != usr)
        {
            if (t.onTile is Servant x) buf.Add(x);
            else if(t.onTile is LoadAble ve)
                foreach(Servant y in ve.onLoad.OfType<Servant>()) buf.Add(y);
        }
        tar = await usr.SelectTarget(buf);
        
        if (tar == null) return false;
        
        --usr.CommandCount;
        if (!await GameManager.Instance.AskForWuXie(this,usr))
        {
            GameManager.Instance.DiscardCard(this);
            return true;
        }

        Player ori = tar.player;
        tar.player = usr;
        tar.canAct = true;
        await tar.TakeAction();
        tar.player = ori;
        
        GameManager.Instance.DiscardCard(this);
        return true;
    }
}
public class JinGu : Skill
{
    Piece tar;
    public JinGu() : base()
    {
        cardName = "禁锢";
        cardDescription = "使任意单位无法行动，持续1回合。";
        sprite = GameManager.Instance.skillAtlas.GetSprite("JinGu");
    }
    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;

        List<Piece> buf = new List<Piece>();
        foreach (Tile t in GameManager.Instance.tiles.Values) if (t.onTile != null)
        {
            buf.Add(t.onTile);
            if(t.onTile is LoadAble ve)
                foreach(Piece y in ve.onLoad.OfType<Piece>()) buf.Add(y);
        }
        tar = await usr.SelectTarget(buf);

        if (tar == null) return false;

        --usr.CommandCount;
        if (!await GameManager.Instance.AskForWuXie(this, usr))
        {
            GameManager.Instance.DiscardCard(this);
            return true;
        }

        tar.buffs.Add(new Stun(1));

        GameManager.Instance.DiscardCard(this);
        return true;
    }
}

public class HuoQiu : Skill
{
    int tarx, tary;
    public HuoQiu() : base()
    {
        cardName = "火球";
        cardDescription = "对指定格及其相邻格造成6点伤害。\nMaster、傀儡、友军受到法术伤害会减少。";
        sprite = GameManager.Instance.skillAtlas.GetSprite("HuoQiu");
    }

    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;
        --usr.CommandCount;

        (tarx,tary) = await usr.SelectPosition(
            GameManager.Instance.tiles
            .Select(pii => pii.Key)
            .ToList()
        );

        if (!await GameManager.Instance.AskForWuXie(this,usr))
        {
            GameManager.Instance.DiscardCard(this);
            return true;
        }

        for (int i = 0; i < 6; i++)
        {
            int nx = tarx + dx[i], ny = tary + dy[i];
            Piece e = GameManager.Instance.GetTile(nx, ny)?.onTile;
            if (e != null)
            {
                e.TakeDamage(null, 6);
                if (e is LoadAble ve)
                    foreach (Piece p in ve.onLoad.OfType<Piece>())
                        p.TakeDamage(null, 6);
            }
        }
        Piece ee = GameManager.Instance.GetTile(tarx, tary)?.onTile;
        if (ee != null)
        {
            ee.TakeDamage(null, 6);
            if (ee is LoadAble ve)
                foreach (Piece p in ve.onLoad.OfType<Piece>())
                    p.TakeDamage(null, 6);
        }
        
        GameManager.Instance.DiscardCard(this);
        return true;
    }
}
public class GunMu : Skill
{
    int tarx, tary, tarf;
    public GunMu() : base()
    {
        cardName = "滚木";
        cardDescription = "对连续4格造成6点伤害，并击退1格。\nMaster、傀儡、友军受到法术伤害会减少。";
        sprite = GameManager.Instance.skillAtlas.GetSprite("GunMu");
    }
    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;
        --usr.CommandCount;
        
        HashSet<(int, int)> buf = new HashSet<(int, int)>();
        foreach(var (xx,yy) in GameManager.Instance.tiles.Keys)
        {
            buf.Add((xx, yy));
            for(int i = 0; i < 6; i++)
            {
                buf.Add((xx + dx[i], yy + dy[i]));
                buf.Add((xx + 2 * dx[i], yy + 2 * dy[i]));
                buf.Add((xx + 3 * dx[i], yy + 3 * dy[i]));
            }
        }
        (tarx,tary) = await usr.SelectPosition(buf.ToList());
        tarf = await usr.SelectDirection(tarx, tary, true);

        if (!await GameManager.Instance.AskForWuXie(this, usr))
        {
            GameManager.Instance.DiscardCard(this);
            return true;
        }

        for (int i = 3; i >= 0; i--)
        {
            Piece e = GameManager.Instance.GetTile(tarx + i * dx[tarf], tary + i * dy[tarf])?.onTile;
            if (e != null)
            {
                e.TakeDamage(null, 6);
                if (e is LoadAble ve)
                    foreach (Piece p in ve.onLoad.OfType<Piece>())
                        p.TakeDamage(null, 6);
                if (e is not ICanBanKnock) e.Knockback(tarf);
            }
        }
        
        GameManager.Instance.DiscardCard(this);
        return true;
    }

}
public class JuFeng : Skill
{
    int tarx, tary, tarf;
    public JuFeng() : base()
    {
        cardName = "飓风";
        cardDescription = "对连续4格造成3点穿透伤害，并驱离。\nMaster、傀儡、友军受到法术伤害会减少。";
        sprite = GameManager.Instance.skillAtlas.GetSprite("JuFeng");
    }
    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;
        --usr.CommandCount;
        
        HashSet<(int, int)> buf = new HashSet<(int, int)>();
        foreach(var (xx,yy) in GameManager.Instance.tiles.Keys)
        {
            buf.Add((xx, yy));
            for(int i = 0; i < 6; i++)
            {
                buf.Add((xx + dx[i], yy + dy[i]));
                buf.Add((xx + 2 * dx[i], yy + 2 * dy[i]));
                buf.Add((xx + 3 * dx[i], yy + 3 * dy[i]));
            }
        }
        (tarx,tary) = await usr.SelectPosition(buf.ToList());
        tarf = await usr.SelectDirection(tarx,tary, true);

        if (!await GameManager.Instance.AskForWuXie(this,usr))
        {
            GameManager.Instance.DiscardCard(this);
            return true;
        }

        for (int i = 3; i >= 0; i--)
        {
            Piece e = GameManager.Instance.GetTile(tarx + i * dx[tarf], tary + i * dy[tarf])?.onTile;
            if (e != null)
            {
                e.TakeDamage(null, 3, true);
                if (e is LoadAble ve)
                    foreach (Piece p in ve.onLoad.OfType<Piece>())
                        p.TakeDamage(null, 3, true);
                if (e is not ICanBanKnock) e.ForceMove(tarf);
            }
        }
        
        GameManager.Instance.DiscardCard(this);
        return true;
    }

}

public class WuZhong : Skill
{
    public WuZhong() : base()
    {
        cardName = "无中生有";
        cardDescription = "获得2划令咒。";
        sprite = GameManager.Instance.skillAtlas.GetSprite("WuZhong");
    }
    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;
        --usr.CommandCount;

        if (!await GameManager.Instance.AskForWuXie(this,usr))
        {
            GameManager.Instance.DiscardCard(this);
            return true;
        }

        usr.CommandCount += 2;
        
        GameManager.Instance.DiscardCard(this);
        return true;
    }
}

public class TianJia : Skill
{
    int tarx, tary;
    public TianJia() : base()
    {
        cardName = "添加棋盘块";
        cardDescription = "添加一个棋盘块。若场上已有70或以上个棋盘格，游戏将立即结束，Master生命值最高的玩家将获胜！\n新棋盘块必须与已有棋盘块连通。";
        sprite = GameManager.Instance.skillAtlas.GetSprite("TianJia");
    }
    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;
        --usr.CommandCount;

        if (!await GameManager.Instance.AskForWuXie(this,usr))
        {
            GameManager.Instance.DiscardCard(this);
            return true;
        }


        if (GameManager.Instance.tiles.Count >= 70)
        {
            int ma = 0;
            for (int i = 0; i < GameManager.Instance.players.Count; i++)
                if (!GameManager.Instance.players[i].dead && GameManager.Instance.players[i].master.HP > GameManager.Instance.players[ma].master.HP)
                    ma = i;
            GameManager.Instance.EndGame(GameManager.Instance.players[ma].team);

            GameManager.Instance.DiscardCard(this);
            return true;
        }
        else
        {
            HashSet<(int, int)> buf = new HashSet<(int, int)>();
            foreach (var (x, y) in GameManager.Instance.tiles.Keys)
            {
                buf.Add((x, y));
                for (int i = 0; i < 6; i++)
                {
                    buf.Add((x + dx[i], y + dy[i]));
                    buf.Add((x + 2 * dx[i], y + 2 * dy[i]));
                }
            }
            foreach (var (x, y) in GameManager.Instance.tiles.Keys)
            {
                buf.Remove((x, y));
                for (int i = 0; i < 6; i++)
                    buf.Remove((x + dx[i], y + dy[i]));
            }
            var (xx, yy) = await usr.SelectPosition(buf.ToList());

            for (int i = 0; i < 7; i++)
                if (GameManager.Instance.tiles.ContainsKey((xx + dx[i], yy + dy[i]))) return false;

            List<Tile> tiles = new List<Tile>();
            for (int i = 0; i < 7; i++)
                tiles.Add(GameManager.Instance.AddTile(xx + dx[i], yy + dy[i], Terrain.Plain, i == 6));


            GameManager.Instance.raycaster.eventMask = LayerMask.GetMask("Tile");
            UIManager.Instance.SwitchToSelectUI();
            UIManager.Instance.FinishButton.gameObject.SetActive(true);

            UniTaskCompletionSource tcs = new UniTaskCompletionSource();
            UIManager.Instance.FinishSelect += () => { tcs?.TrySetResult(); };
            await tcs.Task;
            UIManager.Instance.ClearFinish();

            GameManager.Instance.raycaster.eventMask = LayerMask.GetMask("Piece");
            UIManager.Instance.SwitchToNormalUI();
            foreach (Tile t in tiles) t.isEditable = false;
            GameManager.Instance.DiscardCard(this);
            return true;
        }
    }

}
public class YiDong : Skill
{
    int tarx, tary;
    public YiDong() : base()
    {
        cardName = "移动棋盘块";
        cardDescription = "将一个棋盘块移动至任意位置，或从棋盘上移除。\n移动后棋盘块必须与已有棋盘块连通。";
        sprite = GameManager.Instance.skillAtlas.GetSprite("YiDong");
    }

    bool CheckCanBanMove(Tile tile)
    {
        Piece p = tile.onTile;
        if (p is ICanBanMove) return true;
        if (p is LoadAble ve)
            foreach (var x in ve.onLoad) if (x is ICanBanMove) return true;
        return false;
    }

    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;
        --usr.CommandCount;

        List<(int, int)> buf1 = new List<(int, int)>();

        foreach(var pii in GameManager.Instance.tiles)
            if (pii.Value.isCenter)
            {
                bool ok = false;
                for (int i = 0; i < 7; i++)
                    if (CheckCanBanMove(GameManager.Instance.tiles[(pii.Value.xpos + dx[i], pii.Value.ypos + dy[i])]))
                    { ok = true; break; }
                if (!ok) buf1.Add(pii.Key);
            }
        (tarx, tary) = await usr.SelectPosition(buf1);

        if (!await GameManager.Instance.AskForWuXie(this, usr))
        {
            GameManager.Instance.DiscardCard(this);
            return true;
        }

        List<Tile> tilebuf = new List<Tile>();
        for(int i = 0; i < 7; i++)
        {
            tilebuf.Add(GameManager.Instance.tiles[(tarx + dx[i], tary + dy[i])]);
            GameManager.Instance.tiles.Remove((tarx + dx[i], tary + dy[i]));
        }


        HashSet<(int, int)> buf = new HashSet<(int, int)>();
        foreach(var (x,y) in GameManager.Instance.tiles.Keys)
        {
            buf.Add((x, y));
            for(int i = 0; i < 6; i++)
            {
                buf.Add((x + dx[i], y + dy[i]));
                buf.Add((x + 2 * dx[i], y + 2 * dy[i]));
            }
        }
        foreach (var (x, y) in GameManager.Instance.tiles.Keys)
        {
            buf.Remove((x, y));
            for (int i = 0; i < 6; i++)
                buf.Remove((x + dx[i], y + dy[i]));
        }

        UIManager.Instance.SwitchToSelectUI();
        UIManager.Instance.FinishButton.gameObject.SetActive(true);
        usr.PositionTcs = new UniTaskCompletionSource<(int, int)>();
        usr.TargetTcs = new UniTaskCompletionSource();
        List<(int, int)> PosSet = buf.ToList();
        List<SelectPositionTag> PositionTags=new List<SelectPositionTag>();
        foreach (var (x, y) in PosSet)
        {
            Vector2 pos = GameManager.GetPosition(x, y);
            GameObject go = GameObject.Instantiate(GameManager.Instance.SelectPositionTagPrefab, pos, Quaternion.identity);
            SelectPositionTag tag = go.GetComponent<SelectPositionTag>();
            tag.xpos = x; tag.ypos = y; tag.player = usr;
            PositionTags.Add(tag);
            go.SetActive(true);
        }
        GameManager.Instance.raycaster.eventMask = LayerMask.GetMask("SelectTag");
        UIManager.Instance.FinishSelect += () => { usr.TargetTcs?.TrySetResult(); };
        var tasks = new[] { usr.PositionTcs.Task, usr.TargetTcs.Task };
        int win = await UniTask.WhenAny(tasks);
        UIManager.Instance.ClearFinish();
        foreach (var tag in PositionTags)
            GameObject.Destroy(tag.gameObject);
        PositionTags.Clear();
        GameManager.Instance.raycaster.eventMask = LayerMask.GetMask("Piece");
        UIManager.Instance.SwitchToNormalUI();

        if (win == 1)
        {
            foreach(Tile t in tilebuf)
            {
                Piece x = t.onTile;
                if (x != null)
                {
                    if (x is LoadAble ve)
                        foreach (Piece p in ve.onLoad.OfType<Piece>()) p.OnDeath();
                    x.OnDeath();
                }
                GameObject.Destroy(t.gameObject,0.01f);
            }
        }
        else
        {
            (tarx, tary) = await usr.PositionTcs.Task;
            int tarf = await usr.SelectDirection(tarx, tary);
            for(int i = 0; i < 7; i++)
            {
                int x = tarx + dx[(i + tarf) % 6], y = tary + dy[(i + tarf) % 6];
                if(i==6){ x = tarx; y = tary; }
                GameManager.Instance.tiles[(x, y)] = tilebuf[i];
                tilebuf[i].xpos = x;
                tilebuf[i].ypos = y;
                if (tilebuf[i].onTile != null)
                {
                    Piece p = tilebuf[i].onTile;
                    p.xpos = x;p.ypos = y;
                    p.facing = (p.facing + tarf) % 6;
                    if(p.renderer is BoardPieceRenderer bp)
                    {
                        bp.InitPosition(x, y);
                        bp.InitRotation(p.facing);
                    }
                }
                tilebuf[i].UpdatePosition();
            }
        }

        GameManager.Instance.DiscardCard(this);
        return true;
    }

}
public class XiuGai : Skill
{
    int tarx, tary;
    public XiuGai() : base()
    {
        cardName = "修改地形";
        cardDescription = "修改一个空格的地形。";
        sprite = GameManager.Instance.skillAtlas.GetSprite("XiuGai");
    }
    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;
        --usr.CommandCount;

        (tarx,tary) = await usr.SelectPosition(
            GameManager.Instance.tiles
            .Where(pii => pii.Value.onTile == null)
            .Select(pii => pii.Key)
            .ToList()
        );

        if (!await GameManager.Instance.AskForWuXie(this, usr))
        {
            GameManager.Instance.DiscardCard(this);
            return true;
        }

        Tile t = GameManager.Instance.GetTile(tarx, tary);
        t.isEditable = true;
        
        GameManager.Instance.raycaster.eventMask = LayerMask.GetMask("Tile");
        UIManager.Instance.SwitchToSelectUI();
        UIManager.Instance.FinishButton.gameObject.SetActive(true);

        UniTaskCompletionSource tcs = new UniTaskCompletionSource();
        UIManager.Instance.FinishSelect += () => { tcs?.TrySetResult(); };
        await tcs.Task;

        GameManager.Instance.raycaster.eventMask = LayerMask.GetMask("Piece");
        UIManager.Instance.SwitchToNormalUI();
        t.isEditable = false;
        
        GameManager.Instance.DiscardCard(this);
        return true;
    }

}

public class WuXie : Skill
{
    public WuXie() : base()
    {
        cardName = "无懈可击";
        cardDescription = "抵消一个技能的效果。";
        sprite = GameManager.Instance.skillAtlas.GetSprite("WuXie");
    }
    public override async UniTask<bool> UseCard(Player usr)
    {
        return false;
    }

}
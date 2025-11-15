using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Skill : Card
{

}
public class TuXi : Skill
{
    public TuXi() : base()
    {
        cardName = "奇兵突袭";
        sprite = GameManager.Instance.skillAtlas.GetSprite("TuXi");
    }
    public override async UniTask UseCard(Player usr)
    {
        List<Piece> buf = usr.onBoardList
            .Where(p => (p is not UnitBase))
            .ToList();
        Piece tar = await usr.SelectTarget(buf);
        var (x, y) = await usr.SelectPosition(
            GameManager.Instance.tiles
            .Where(pii => pii.Value.onTile == null)
            .Select(pii => pii.Key)
            .ToList()
        );
        tar.xpos = x;tar.ypos = y;
        tar.UpdateOnBoardPosition();
        tar.canAct = true;
    }
}

public class JinGu : Skill
{

}
public class CeFan : Skill
{

}
public class WuZhong : Skill
{

}
public class ZhuanYi : Skill
{

}
public class TianJia : Skill
{

}
public class YiDong : Skill
{

}
public class XiuGai : Skill
{

}
public class HuoQiu : Skill
{

}
public class GunMu : Skill
{

}
public class JuFeng : Skill
{
    
}
public class WuXie : Skill
{

}
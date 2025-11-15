using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : Piece,ICanOnLoad,ICanBanMove
{
    public List<Weapon> equip;
}
public class Arthuria : Hero
{
    public Arthuria():base()
    {
        maxHP = HP = 12;
        AT = 7;
        maxDF = DF = 4;
        RA = 1;
        ST = 1;
        canClimb = false;canSwim = false;canBanMagic = true;canRide = true;
        cardName = "阿尔托莉雅·潘德拉贡";
        cardDescription = "古不列颠传说中的亚瑟王，在圣杯战争中曾多次毁掉圣杯。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Arthuria");
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
        canClimb = true;canSwim = true;canBanMagic = true;canRide = false;
        cardName = "包涵·溯时之枪";
        cardDescription = "作者以男身被召唤的英灵，似乎拥有掌控时间的力量。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Include");
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
        canClimb = true;canSwim = true;canBanMagic = true;canRide = false;
        cardName = "春迟日落·裂空之箭";
        cardDescription = "作者以女身被召唤的英灵，似乎拥有撕裂空间的力量。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Sunsettia");
    }
}

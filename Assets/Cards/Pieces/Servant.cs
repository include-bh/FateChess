using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Servant : Piece
{
    public Weapon equip;
    public override void OnDeath()
    {
        if (equip != null)
        {
            equip.gameObject.SetActive(false);
            GameManager.Instance.DiscardCard(equip);
            equip = null;
        }
        base.OnDeath();
    }

}
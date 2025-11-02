using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action<Piece, Piece> OnKill;
    public static void TriggerOnKill(Piece a,Piece b){OnKill?.Invoke(a,b);}
    public static event Action<Piece, Piece> OnBreak;
    public static void TriggerOnBreak(Piece a,Piece b){OnBreak?.Invoke(a,b);}
    public static event Action<Piece, Piece> OnAttack;
    public static void TriggerOnAttack(Piece a,Piece b){OnAttack?.Invoke(a,b);}

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

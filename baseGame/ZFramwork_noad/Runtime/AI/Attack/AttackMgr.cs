using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AttackMgr
{
    public static List<AttackInstance> _allAttacks = new List<AttackInstance>();

    public static void Clear()
    {
        _allAttacks.Clear();
    }

    public static int GetCount()
    {
        return _allAttacks.Count;
    }

    public static void AddAttackInstance(AttackInstance attack)
    {
        if (!_allAttacks.Contains(attack))
        {
            _allAttacks.Add(attack);
        }

    }

    public static void RemoveAttackInstance(AttackInstance attack)
    {
        if (_allAttacks.Contains(attack))
        {
            _allAttacks.Remove(attack);
        }
    }

   
}

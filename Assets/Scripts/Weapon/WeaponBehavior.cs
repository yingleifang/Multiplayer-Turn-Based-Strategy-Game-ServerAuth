using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBehavior : MonoBehaviour
{
    public AnimatorOverrideController overrideController;

    public int attack = 5;

    public int attackRange = 1;

    public float AttackAnimationLength = 0.8f;

    public bool isLeftHand = false;

    protected UnitFeature owner;
    public virtual IEnumerator AttackBehavior(UnitFeature target = default, UnitFeature attaker = default)
    {
        return null;
    }

    public virtual void setOwner(UnitFeature temp)
    {
        owner = temp;
    }
}

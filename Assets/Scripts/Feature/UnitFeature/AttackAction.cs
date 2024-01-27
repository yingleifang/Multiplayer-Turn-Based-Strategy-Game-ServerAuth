using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAction : BaseAction
{
    public void DoAttack(UnitFeature target)
    {
        unit.canAttack = false;
        StartBlockingCoroutine(Hitting(target));
    }

    public IEnumerator Hitting(UnitFeature target)
    {
        yield return unit.TurnTo(target.location.Position);
        unit.unitAnimation.UnitAnimation_Attack();
        yield return new WaitForSeconds(unit.getHitDelay());
        yield return unit.weaponInstance.AttackBehavior(target, unit);
        target.TakeDamage(unit.GetUnitDamage());
        yield return new WaitForSeconds(1f);
    }
}

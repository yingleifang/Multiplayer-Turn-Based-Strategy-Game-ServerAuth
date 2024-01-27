using UnityEngine;
using System.Collections;

/// <summary>
/// Component representing a unit that occupies a cell of the hex map.
/// </summary>
public class HexUnit : UnitFeature
{
	[SerializeField]
	CardDatabase.DamageType DamageType;

    float DeathDelay = 2f;

	MoveAction moveAction;
	AttackAction attackAction;

	public float rotationSpeed = 180f;
	public float travelSpeed = 4f;
	[SerializeField]
	int attackDamage = 3;
	[SerializeField]
	int AttackRange = 1;

	public static HexUnit unitPrefab;

	public HexCell currentTravelLocation;

	/// <summary>
	/// Speed of the unit, in cells per turn.
	/// </summary>
	public int MovementRange => 3;
	public bool canMove = true;
	public bool canAttack = true;

	public UnitAnimation unitAnimation;

	[SerializeField]
	public WeaponSlotManager myWeaponSlotManager;

	[SerializeField]
	public WeaponBehavior weaponInstance;

	public float attackActionDelay;
	protected override void Awake()
	{
		base.Awake();
		moveAction = GetComponent<MoveAction>();
		attackAction = GetComponent<AttackAction>();
    }

	public CardDatabase.DamageType GetDamageType()
	{
		return DamageType;
	}
    public float getHitDelay()
    {
		return attackActionDelay;
    }
	public void reFillActions()
    {
		canMove = true;
		canAttack = true;
    }

	/// <summary>
	/// Checl whether a cell is a valid destination for the unit.
	/// </summary>
	/// <param name="cell">Cell to check.</param>
	/// <returns>Whether the unit could occupy the cell.</returns>
	public bool IsValidDestination (HexCell cell) => !cell.IsUnderwater && !cell.unitFeature;

	/// <summary>
	/// Get the movement cost of moving from one cell to another.
	/// </summary>
	/// <param name="fromCell">Cell to move from.</param>
	/// <param name="toCell">Cell to move to.</param>
	/// <param name="direction">Movement direction.</param>
	/// <returns></returns>
	public int GetMoveCost (
		HexCell fromCell, HexCell toCell, HexDirection direction)
	{
		if (!IsValidDestination(toCell)) {
			return -1;
		}
		HexEdgeType edgeType = fromCell.GetEdgeType(toCell);
		if (edgeType == HexEdgeType.Cliff) {
			return -1;
		}
		//int moveCost;
		//if (fromCell.HasRoadThroughEdge(direction)) {
		//	moveCost = 1;
		//}
		if (fromCell.Walled != toCell.Walled) {
			return -1;
		}
		return 1;
		//else {
		//	moveCost = edgeType == HexEdgeType.Flat ? 5 : 10;
		//	moveCost +=
		//		toCell.UrbanLevel + toCell.FarmLevel + toCell.PlantLevel;
		//}
		//return moveCost;
	}

	/// <summary>
	/// Terminate the unit.
	/// </summary>
	public void Die () {
		location.unitFeature = null;
		Destroy(gameObject);
	}

	void OnEnable () {
		if (location) {
			transform.localPosition = location.Position;
			if (currentTravelLocation) {
				currentTravelLocation = null;
			}
		}
	}
	public MoveAction GetMoveAction()
	{
		return moveAction;
	}

	public AttackAction GetAttackAction()
	{
		return attackAction;
	}
	public override void TakeDamage(int damage)
	{
		base.TakeDamage(damage);
			if (UnitCurHealth <= 0)
			{
				StartCoroutine(Death());
				DisableUnit();
				myPlayer.myUnits.Remove(this);
			}
			else
			{
				unitAnimation.UnitAnimation_GetHit();
			}
	}
	public IEnumerator Death()
	{
		unitAnimation.UnitAnimation_Death();
		yield return new WaitForSeconds(DeathDelay);
		Destroy(gameObject);
	}

	public IEnumerator TurnTo(Vector3 point)
	{
		point.y = transform.localPosition.y;
		Quaternion fromRotation = transform.localRotation;
		Quaternion toRotation =
			Quaternion.LookRotation(point - transform.localPosition);
		float angle = Quaternion.Angle(fromRotation, toRotation);

		if (angle > 0f)
		{
			float speed = rotationSpeed / angle;
			for (
				float t = Time.deltaTime * speed;
				t < 1f;
				t += Time.deltaTime * speed
			)
			{
				transform.localRotation =
					Quaternion.Slerp(fromRotation, toRotation, t);
				yield return null;
			}
		}
		orientation = transform.localRotation.eulerAngles.y;
	}
	public void DisableUnit()
	{
		canMove = false;
		canAttack = false;
	}
	
	public int GetUnitDamage()
    {
		int totalDamage = attackDamage;
		if (weaponInstance)
        {
			totalDamage += weaponInstance.attack;
		}
		return totalDamage;
    }

	public int GetUnitAttackRange()
	{
		int totalAttackRange = AttackRange;
		if (weaponInstance)
		{
			totalAttackRange += weaponInstance.attackRange;
		}
		return totalAttackRange;
	}


	public void SetWeapon(WeaponBehavior weapon)
	{
        weaponInstance = weapon;
        weaponInstance.setOwner(this);
        unitAnimation.unitAnimator.runtimeAnimatorController = weaponInstance.overrideController;
        attackActionDelay = weaponInstance.AttackAnimationLength * 2 / 3;
        myWeaponSlotManager.LoadWeaponOnSlot(weaponInstance, weapon.isLeftHand);
    }
}
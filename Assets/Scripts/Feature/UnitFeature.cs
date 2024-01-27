using System;
using UnityEngine;

public class UnitFeature : Feature
{
    public int UnitCurHealth { get; protected set; } = 30;
	public int UnitTotalHealth { get; protected set; } = 30;

	public event EventHandler OnDamaged;

    public float meshHeight;

protected virtual void Awake()
    {
		meshHeight = GetComponent<CapsuleCollider>().height;
    }
    public override HexCell Location
	{
		get => location;
		set
		{
			if (location)
			{
				location.unitFeature = null;
			}
			location = value;
			value.unitFeature = this;
			transform.localPosition = value.Position;
			HexGrid.Instance.MakeChildOfColumn(transform, value.ColumnIndex);
		}
	}
    public virtual void TakeDamage(int damage)
    {
		UnitCurHealth -= damage;
		OnDamaged?.Invoke(this, EventArgs.Empty);
		GetHitVisual(damage);
	}
    protected virtual void GetHitVisual(int damage)
    {
		DamagePopUp.Create(transform.position, damage, false);

	}
	public float GetHealthNormalized()
	{
		return (float)UnitCurHealth / UnitTotalHealth;
	}
}
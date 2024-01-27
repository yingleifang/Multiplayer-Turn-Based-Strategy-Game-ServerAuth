using UnityEngine;
using System;
using static FeatureScriptbleObject;

public class Feature : MonoBehaviour
{
	public BaseLogic.PlayerFaction playerID;
	public float orientation;

	public HexCell location;

	public FeatureSelectedVisuals featureSelectedVisuals;

	public event EventHandler FeatureSelected;

	public event EventHandler FeatureDeSelected;

	public Faction myPlayer;

	[SerializeField]
	public GameObject PotentialTargetIndicator;
	private void Awake()
    {
		featureSelectedVisuals = GetComponentInChildren<FeatureSelectedVisuals>();
	}

    /// <summary>
    /// Cell that the unit occupies.
    /// </summary>
    public virtual HexCell Location
	{ get; set; }

	/// <summary>
	/// Orientation that the unit is facing.
	/// </summary>
	public float Orientation
	{
		get => orientation;
		set
		{
			orientation = value;
			transform.localRotation = Quaternion.Euler(0f, value, 0f);
		}
	}

	/// <summary>
	/// Validate the position of the unit.
	/// </summary>
	public void ValidateLocation() => transform.localPosition = location.Position;

	//load feature from file
	public static Feature Load(FeatureData featureData, HexGrid grid, Feature feature)
	{
		Feature spawnedFeature = Instantiate(feature);
		grid.AddFeatureBeforeGame(
            spawnedFeature, grid.GetCell(featureData.coordinate), featureData.orientation
		);
		return spawnedFeature;
	}
    public void RaiseFeatureSelectedEvent()
	{
		FeatureSelected?.Invoke(this, EventArgs.Empty);
	}

	public void RaiseFeatureDeSelectedEvent()
	{
		FeatureDeSelected?.Invoke(this, EventArgs.Empty);
	}
}

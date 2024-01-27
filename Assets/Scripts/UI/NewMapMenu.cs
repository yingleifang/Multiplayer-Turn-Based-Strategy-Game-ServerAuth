﻿using UnityEngine;

/// <summary>
/// Component that applies actions from the new map menu UI to the hex map.
/// Public methods are hooked up to the in-game UI.
/// </summary>
public class NewMapMenu : MonoBehaviour {

	[SerializeField]
	HexGrid hexGrid;

	[SerializeField]
	HexMapGenerator mapGenerator;

	bool generateMaps = true;

	public void ToggleMapGeneration (bool toggle) => generateMaps = toggle;

	public void Open () {
		gameObject.SetActive(true);
		HexMapCamera.Locked = true;
	}

	public void Close () {
		gameObject.SetActive(false);
		HexMapCamera.Locked = false;
	}

	public void CreateSmallMap () => CreateMap(20, 15);

	public void CreateMediumMap () => CreateMap(35, 30);

	public void CreateLargeMap () => CreateMap(80, 60);

	void CreateMap (int x, int z) {
		if (generateMaps) {
			mapGenerator.GenerateMap(x, z);
		}
		else {
			hexGrid.CreateMap(x, z);
		}
		Close();
	}
}
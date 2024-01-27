using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using Unity.Netcode;
using static Map;
using static FeatureScriptbleObject;

/// <summary>
/// Component that represents an entire hexagon map.
/// </summary>
public class HexGrid : MonoBehaviour
{
    [SerializeField]
    Material terrainMaterial;

    public Map curMap;
	public FeatureScriptbleObject curFeature;
    public static HexGrid Instance { get; private set; }

	public List<HexUnit> units = new List<HexUnit>();
	public List<Base> bases = new List<Base>();
	public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
	
	public bool BlockActions = false;

    public string mapSuffix = "map.asset";
    public string featureSuffix = "feature.asset";
    [SerializeField]
	public Base player1BasePrefab;
	[SerializeField]
	public Base player2BasePrefab;

	[SerializeField]
	public SpawnPoint player1SpawnPointPrefab;
	[SerializeField]
	public SpawnPoint player2SpawnPointPrefab;

	[SerializeField]
	HexCell cellPrefab;

	[SerializeField]
	Text cellLabelPrefab;

    [SerializeField]
	HexGridChunk chunkPrefab;

	[SerializeField]
	Texture2D noiseSource;

	[SerializeField]
	int seed;

	Transform[] columns;

	/// <summary>
	/// Amount of cells in the X dimension.
	/// </summary>
	public int CellCountX
	{ get; private set; }

	/// <summary>
	/// Amount of cells in the Z dimension.
	/// </summary>
	public int CellCountZ
	{ get; private set; }

	public HexCell currentPathFrom, currentPathTo;

	public List<HexCell> curPath;

	/// <summary>
	/// Whether there currently exists a path that should be displayed.
	/// </summary>
	public bool HasPath => CurrentPathExists;

    public HexCell[] Cells { get; private set; }

    HexGridChunk[] chunks;

	int chunkCountX, chunkCountZ;

	HexCellPriorityQueue searchFrontier;

	int searchFrontierPhase;

	public bool CurrentPathExists { get; private set; }

	HexCellShaderData cellShaderData;

	Dictionary<Color, List<HexCell>> coloredCells = new()
	{
		{ Color.white, new() },
		{ Color.red, new() },
		{ Color.blue, new() },

	};

	void Awake ()
	{
		//GetComponent<NetworkObject>().Spawn();	
        terrainMaterial.DisableKeyword("_SHOW_GRID");
        Shader.EnableKeyword("_HEX_MAP_EDIT_MODE");
        if (Instance != null)
        {
            Debug.LogError("There's more than one TurnSystem!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CellCountX = 20;
		CellCountZ = 15;
		HexMetrics.noiseSource = noiseSource;
		HexMetrics.InitializeHashGrid(seed);
		cellShaderData = gameObject.AddComponent<HexCellShaderData>();
		CreateMap(CellCountX, CellCountZ);
	}
	public void AddFeatureBeforeGame(Feature feature, HexCell location, float orientation)
	{
		if (feature is Base baseFeature)
        {
			bases.Add(baseFeature);
        }
        else if (feature is SpawnPoint spawnPointFeature)
        {
			spawnPoints.Add(spawnPointFeature);
        }
		feature.Location = location;
        feature.Orientation = orientation;
    }

	public void AddUnit(Faction player, HexUnit unitPrefab, HexCell location)
	{
        HexUnit unit = Instantiate(unitPrefab);
        SetUnitAttributes(player, location, unit);
        player.myUnits.Add(unit);
    }

    public void SetUnitAttributes(Faction player, HexCell location, HexUnit unit)
    {
        unit.Location = location;
        //unit.Orientation = player.selectedFeature.orientation;
        location.unitFeature = unit;
        units.Add(unit);
        unit.myPlayer = player;
    }

    /// <summary>
    /// Remove a unit from the map.
    /// </summary>
    /// <param name="unit">The unit to remove.</param>
    public void RemoveUnit (HexUnit unit)
	{
		units.Remove(unit);
		unit.Die();
	}

	/// <summary>
	/// Make a game object a child of a map column.
	/// </summary>
	/// <param name="child"><see cref="Transform"/> of the child game object.</param>
	/// <param name="columnIndex">Index of the parent column.</param>
	public void MakeChildOfColumn(Transform child, int columnIndex)
    {
        child.SetParent(columns[columnIndex], false);
	}

	/// <summary>
	/// Create a new map.
	/// </summary>
	/// <param name="x">X size of the map.</param>
	/// <param name="z">Z size of the map.</param>
	/// <param name="wrapping">Whether the map wraps east-west.</param>
	/// <returns>Whether the map was successfully created. It fails if the X or Z size
	/// is not a multiple of the respective chunk size.</returns>
	public bool CreateMap (int x, int z)
	{
		if (
			x <= 0 || x % HexMetrics.chunkSizeX != 0 ||
			z <= 0 || z % HexMetrics.chunkSizeZ != 0
		)
		{
			Debug.LogError("Unsupported map size.");
			return false;
		}

		//ClearPath();
		ClearUnits();
		if (columns != null)
		{
			for (int i = 0; i < columns.Length; i++)
			{
				Destroy(columns[i].gameObject);
			}
		}

		CellCountX = x;
		CellCountZ = z;
		chunkCountX = CellCountX / HexMetrics.chunkSizeX;
		chunkCountZ = CellCountZ / HexMetrics.chunkSizeZ;
		cellShaderData.Initialize(CellCountX, CellCountZ);
		CreateChunks();
		CreateCells();
		return true;
	}

	void CreateChunks()
	{
		columns = new Transform[chunkCountX];
		for (int x = 0; x < chunkCountX; x++)
		{
            columns[x] = new GameObject("Column").transform;
            columns[x].SetParent(transform, false);
		}
		chunks = new HexGridChunk[chunkCountX * chunkCountZ];
		for (int z = 0, i = 0; z < chunkCountZ; z++)
		{
			for (int x = 0; x < chunkCountX; x++)
			{
				HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.transform.SetParent(columns[x], false);
			}
		}
	}

	void CreateCells ()
	{
		Cells = new HexCell[CellCountZ * CellCountX];

		for (int z = 0, i = 0; z < CellCountZ; z++)
		{
			for (int x = 0; x < CellCountX; x++)
			{
				CreateCell(x, z, i++);
			}
		}
	}

	void ClearUnits ()
	{
		for (int i = 0; i < units.Count; i++)
		{
			units[i].Die();
		}
		units.Clear();
	}

	void OnEnable ()
	{
		if (!HexMetrics.noiseSource)
		{
			HexMetrics.noiseSource = noiseSource;
			HexMetrics.InitializeHashGrid(seed);
		}
	}

	/// <summary>
	/// Get a cell given a <see cref="Ray"/>.
	/// </summary>
	/// <param name="ray"><see cref="Ray"/> used to perform a raycast.</param>
	/// <returns>The hit cell, if any.</returns>
	public HexCell GetCell (Ray ray)
	{
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			return GetCell(hit.point);
		}
		return null;
	}

	/// <summary>
	/// Get the cell that contains a position.
	/// </summary>
	/// <param name="position">Position to check.</param>
	/// <returns>The cell containing the position, if it exists.</returns>
	public HexCell GetCell (Vector3 position)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		return GetCell(coordinates);
	}

	/// <summary>
	/// Get the cell with specific <see cref="HexCoordinates"/>.
	/// </summary>
	/// <param name="coordinates"><see cref="HexCoordinates"/> of the cell.</param>
	/// <returns>The cell with the given coordinates, if it exists.</returns>
	public HexCell GetCell (HexCoordinates coordinates)
	{
		int z = coordinates.Z;
		int x = coordinates.X + z / 2;
		if (z < 0 || z >= CellCountZ || x < 0 || x >= CellCountX)
		{
			return null;
		}
		return Cells[x + z * CellCountX];
	}

	/// <summary>
	/// Try to get the cell with specific <see cref="HexCoordinates"/>.
	/// </summary>
	/// <param name="coordinates"><see cref="HexCoordinates"/> of the cell.</param>
	/// <param name="cell">The cell, if it exists.</param>
	/// <returns>Whether the cell exists.</returns>
	public bool TryGetCell (HexCoordinates coordinates, out HexCell cell)
	{
		int z = coordinates.Z;
		int x = coordinates.X + z / 2;
		if (z < 0 || z >= CellCountZ || x < 0 || x >= CellCountX)
		{
			cell = null;
			return false;
		}
		cell = Cells[x + z * CellCountX];
		return true;
	}

	/// <summary>
	/// Get the cell with specific offset coordinates.
	/// </summary>
	/// <param name="xOffset">X array offset coordinate.</param>
	/// <param name="zOffset">Z array offset coordinate.</param>
	/// <returns></returns>
	public HexCell GetCell (int xOffset, int zOffset) =>
		Cells[xOffset + zOffset * CellCountX];

	/// <summary>
	/// Get the cell with a specific index.
	/// </summary>
	/// <param name="cellIndex">Cell index, which should be valid.</param>
	/// <returns>The indicated cell.</returns>
	public HexCell GetCell (int cellIndex) => Cells[cellIndex];

	/// <summary>
	/// Control whether the map UI should be visible or hidden.
	/// </summary>
	/// <param name="visible">Whether the UI should be visibile.</param>
	public void ShowUI (bool visible)
	{
		for (int i = 0; i < chunks.Length; i++)
		{
			chunks[i].ShowUI(visible);
		}
	}

	void CreateCell (int x, int z, int i)
	{
		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * HexMetrics.innerDiameter;
		position.y = 0f;
		position.z = z * (HexMetrics.outerRadius * 1.5f);

		HexCell cell = Cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.Grid = this;
		cell.transform.localPosition = position;
		cell.Coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.Index = i;
		cell.ColumnIndex = x / HexMetrics.chunkSizeX;
		cell.ShaderData = cellShaderData;

		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.z);
		cell.UIRect = label.rectTransform;

		cell.Elevation = 0;

		AddCellToChunk(x, z, cell);
	}

	void AddCellToChunk (int x, int z, HexCell cell)
	{
		int chunkX = x / HexMetrics.chunkSizeX;
		int chunkZ = z / HexMetrics.chunkSizeZ;
		HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

		int localX = x - chunkX * HexMetrics.chunkSizeX;
		int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
		chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
	}

	/// <summary>
	/// Save the map.
	/// </summary>
	/// <param name="writer"><see cref="BinaryWriter"/> to use.</param>
	public void SaveMap(Map mapData)
	{
        mapData.SaveCellCount(CellCountX, CellCountZ);
        for (int i = 0; i < Cells.Length; i++)
		{
            CellData cellData = new();
            mapData.cells.Add(cellData);
            Cells[i].Save(cellData);
		}
        Debug.Log(mapData.cells[0].elevation);
    }

    /// <summary>
    /// Load the map.
    /// <param name="header">Header version.</param>
    public void LoadMap()
	{
		ClearUnits();
		int x = curMap.cellCountX;
		int z = curMap.cellCountZ;
		if (x != CellCountX || z != CellCountZ)
		{
			if (!CreateMap(x, z))
			{
				Debug.LogError("Cannot load map");
			}
		}
        //Have to loop backwards so the cells are loaded in the correct order
        for (int i = 0; i < Cells.Length; i++) 
		{
            Cells[i].Load(curMap.cells[i]);
		}
		for (int i = 0; i < chunks.Length; i++)
		{
			chunks[i].Refresh();
		}
	}
    public void SaveFeature(FeatureScriptbleObject featureObj)
    {
		featureObj.baseCount = bases.Count;
		for (int i = 0; i < bases.Count; i++)
		{
            FeatureData baseData = new FeatureData
			{
                playerId = bases[i].playerID,
                coordinate = bases[i].location.Coordinates,
                orientation = bases[i].orientation
            };
			featureObj.bases.Add(baseData);
        }
		//Reverse the list so it's saved in the correct order
		featureObj.bases.Reverse();
		featureObj.spawnPointsCount = spawnPoints.Count;
		for (int i = 0; i < spawnPoints.Count; i++)
		{
			FeatureData spawnPointData = new FeatureData
			{
                playerId = spawnPoints[i].playerID,
                coordinate = spawnPoints[i].location.Coordinates,
                orientation = spawnPoints[i].orientation
            };
			featureObj.spawnPoints.Add(spawnPointData);
		}
        //Reverse the list so it's saved in the correct order
        featureObj.spawnPoints.Reverse();
    }
    public void LoadFeature()
	{
        if (!curMap)
        {
            Debug.LogError("Map scriptableobject is not set");
            return;
        }

        for (int i = 0; i < curFeature.baseCount; i++)
        {
            FeatureData featureData = curFeature.bases[i];

            if (featureData.playerId == BaseLogic.PlayerFaction.Player1)
            {
                Feature.Load(featureData, this, player1BasePrefab);
            }
            else
            {
                Feature.Load(featureData, this, player2BasePrefab);
            }
        }
        for (int i = 0; i < curFeature.spawnPointsCount; i++)
		{
            FeatureData featureData = curFeature.spawnPoints[i];
            if (featureData.playerId == BaseLogic.PlayerFaction.Player1)
            {
				Feature.Load(featureData, this, player1SpawnPointPrefab);
            }
            else
			{
                Feature.Load(featureData, this, player2SpawnPointPrefab);
            }
        }
    }

    public void LoadFeatureLocal()
    {
		Feature spawnedFeature = null;
        for (int i = 0; i < curFeature.baseCount; i++)
        {
            FeatureData featureData = curFeature.bases[i];
            Base basePrefab = (featureData.playerId == BaseLogic.PlayerFaction.Player1) ? player1BasePrefab : player2BasePrefab;
            spawnedFeature = Feature.Load(featureData, this, basePrefab);
            if (featureData.playerId == BaseLogic.Instance.localFaction.playerID)
			{
                BaseLogic.Instance.localFaction.myBases.Add(spawnedFeature as Base);
			}
			else
			{
                BaseLogic.Instance.localFaction.myBases.Add(spawnedFeature as Base);
            }
        }
        for (int i = 0; i < curFeature.spawnPointsCount; i++)
        {
            FeatureData featureData = curFeature.spawnPoints[i];
            SpawnPoint spawnPointPrefab = (featureData.playerId == BaseLogic.PlayerFaction.Player1) ? player1SpawnPointPrefab : player2SpawnPointPrefab;
            spawnedFeature = Feature.Load(featureData, this, spawnPointPrefab);
            if (featureData.playerId == BaseLogic.Instance.localFaction.playerID)
            {
                BaseLogic.Instance.localFaction.myspawnPoints.Add(spawnedFeature as SpawnPoint);
			}
			else
			{
                BaseLogic.Instance.localFaction.myspawnPoints.Add(spawnedFeature as SpawnPoint);
            }
        }
    }

    /// <summary>
    /// Get a list of cells representing the currently visible path.
    /// </summary>
    /// <returns>The current path list, if a visible path exists.</returns>
    public bool GetPath (int pathLimit = 100)
	{
		if (!CurrentPathExists)
		{
			return false;
		}
		curPath = ListPool<HexCell>.Get();
		int loopTimes = 0;
		for (HexCell c = currentPathTo; c != currentPathFrom && loopTimes < 200; c = c.PathFrom)
		{
			curPath.Add(c);
			loopTimes += 1;
		}
		if (loopTimes > 190)
        {
			Debug.LogError("GetPath didn't find path");
        }
		curPath.Add(currentPathFrom);
		if (curPath.Count > pathLimit)
        {
			curPath = curPath.GetRange(curPath.Count - pathLimit - 1, pathLimit + 1);
		}
		curPath.Reverse();
		if (curPath[^1].unitFeature != null)
		{
			curPath.RemoveAt(curPath.Count - 1);
		}
		return true;
	}

	void ShowPath()
	{
		if (CurrentPathExists)
		{
			HexCell current = currentPathTo;
			while (current != currentPathFrom)
			{
				//int turn = (current.Distance - 1) / speed;
				//current.SetLabel(turn.ToString());
				ColorCells(Color.blue, current);
				current = current.PathFrom;
			}
		}
		ColorCells(Color.blue, currentPathFrom);
		ColorCells(Color.blue, currentPathTo);
	}

	/// <summary>
	/// Try to find a path.
	/// </summary>
	/// <param name="fromCell">Cell to start the search from.</param>
	/// <param name="toCell">Cell to find a path towards.</param>
	/// <param name="unit">Unit for which the path is.</param>
	public void FindPath (HexCell fromCell, HexCell toCell, HexUnit unit, int movementRange)
	{
		ClearCellColor(Color.blue); ;
		currentPathFrom = fromCell;
		currentPathTo = toCell;
		CurrentPathExists = Search(fromCell, toCell, unit, movementRange);
		ShowPath();
	}

	bool Search (HexCell fromCell, HexCell toCell, HexUnit unit, int movementRange)
	{
		searchFrontierPhase += 2;
		if (searchFrontier == null)
		{
			searchFrontier = new HexCellPriorityQueue();
		}
		else
		{
			searchFrontier.Clear();
		}

		fromCell.SearchPhase = searchFrontierPhase;
		fromCell.Distance = 0;
		searchFrontier.Enqueue(fromCell);
		while (searchFrontier.Count > 0)
		{
			HexCell current = searchFrontier.Dequeue();
			current.SearchPhase += 1;

			if (current == toCell)
			{
				return true;
			}

			int currentTurn = (current.Distance - 1) / movementRange;

			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
			{
				if (
					!current.TryGetNeighbor(d, out HexCell neighbor) ||
					neighbor.SearchPhase > searchFrontierPhase
				)
				{
					continue;
				}
				if (!unit.IsValidDestination(neighbor))
				{
					continue;
				}
				int moveCost = unit.GetMoveCost(current, neighbor, d);
				if (moveCost < 0)
				{
					continue;
				}

				int distance = current.Distance + moveCost;
				int turn = (distance - 1) / movementRange;
				if (turn > currentTurn)
				{
					distance = turn * movementRange + moveCost;
				}

				if (neighbor.SearchPhase < searchFrontierPhase && turn <= currentTurn)
				{
					neighbor.SearchPhase = searchFrontierPhase;
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					neighbor.SearchHeuristic =
						neighbor.Coordinates.DistanceTo(toCell.Coordinates);
					searchFrontier.Enqueue(neighbor);
				}
				//else if (distance < neighbor.Distance)
				//{
				//	int oldPriority = neighbor.SearchPriority;
				//	neighbor.Distance = distance;
				//	neighbor.PathFrom = current;
				//	searchFrontier.Change(neighbor, oldPriority);
				//}
			}
		}
		return false;
	}
	public void ClearCellColor(Color color)
	{
		foreach (HexCell current in coloredCells[color])
		{

			current.DisableHighlight();
			foreach (var item in coloredCells)
			{
				if (item.Key != color && item.Value.Contains(current))
				{
					current.EnableHighlight(item.Key);
					break;
				}
			}
		}
		coloredCells[color] = new();
	}

	public void ClearTargetCellColor(Color color, HexCell cell)
	{
        cell.DisableHighlight();
        foreach (var item in coloredCells)
        {
            if (item.Key != color && item.Value.Contains(cell))
            {
                cell.EnableHighlight(item.Key);
                break;
            }
        }
		coloredCells[color] = new();
    }
	public void showMoveRange(HexCell fromCell, HexUnit unit)
	{
		ClearCellColor(Color.white);
		int speed = unit.MovementRange;
		searchFrontierPhase += 2;
		if (searchFrontier == null)
		{
			searchFrontier = new HexCellPriorityQueue();
		}
		else
		{
			searchFrontier.Clear();
		}

		fromCell.SearchPhase = searchFrontierPhase;
		fromCell.Distance = 0;
		searchFrontier.Enqueue(fromCell);
		while (searchFrontier.Count > 0)
		{
			HexCell current = searchFrontier.Dequeue();
			current.SearchPhase += 1;

			int currentTurn = (current.Distance - 1) / speed;

			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
			{
				if (
					!current.TryGetNeighbor(d, out HexCell neighbor) ||
					neighbor.SearchPhase > searchFrontierPhase
				)
				{
					continue;
				}
				if (!unit.IsValidDestination(neighbor))
				{
					continue;
				}
				int moveCost = unit.GetMoveCost(current, neighbor, d);
				if (moveCost < 0)
				{
					continue;
				}

				int distance = current.Distance + moveCost;
				int turn = (distance - 1) / speed;

				if (neighbor.SearchPhase < searchFrontierPhase && turn <= currentTurn)
				{
					neighbor.SearchPhase = searchFrontierPhase;
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					searchFrontier.Enqueue(neighbor);
				}
				//else if (distance < neighbor.Distance)
				//{
				//	int oldPriority = neighbor.SearchPriority;
				//	neighbor.Distance = distance;
				//	neighbor.PathFrom = current;
				//	searchFrontier.Change(neighbor, oldPriority);
				//}
				ColorCells(Color.white, current);
			}
		}
	}
	public void ColorCells(Color color, HexCell current)
	{
		coloredCells[color].Add(current);
		current.EnableHighlight(color);
	}
    public HexCell GetCellUnderCursor() =>
        GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
}

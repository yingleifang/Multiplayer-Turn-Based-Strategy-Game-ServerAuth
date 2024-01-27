using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : ScriptableObject
{
    [Serializable]
    public class CellData
    {
        public int terrainTypeIndex, elevation, waterLevel;
        public int urbanLevel, farmLevel, plantLevel, specialIndex;
        public bool walled;
        public byte incomingRiver, outgoingRiver;
        public HexFlags flags;
    }
    public int cellCountX, cellCountZ;
    public List<CellData> cells = new();
    public int MapVersion;
    public void SaveCellCount(int x, int z)
    {
        cellCountX = x;
        cellCountZ = z;
    }
}

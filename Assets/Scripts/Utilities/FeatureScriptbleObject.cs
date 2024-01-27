using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatureScriptbleObject : ScriptableObject
{
    [Serializable]
    public class FeatureData
    {
        public BaseLogic.PlayerFaction playerId;
        public HexCoordinates coordinate;
        public float orientation;
    }

    public int MapVersion;

    public int baseCount;

    public int spawnPointsCount;

    public List<FeatureData> bases = new();

    public List<FeatureData> spawnPoints = new();
}

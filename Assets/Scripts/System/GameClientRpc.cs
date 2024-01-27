using System;
using System.Collections;
using System.IO;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using UnityEngine;
public class GameClientRpc : NetworkBehaviour
{
    public static GameClientRpc Instance { get; private set; }
    public bool ClientConnected
    {
        get => _connected;
        set
        {
            _connected = value;
        }
    }

    public Vector3 curAttackingPosition;

    private bool _connected;


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one GameManagerClient!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [ClientRpc]
    public void AddFeatureClientRpc(int x, int z, int cardId, ClientRpcParams clientRpcParams = default)
    {
        if (!CardDatabase.Instance.avaliableCards[cardId].cardTemplate.cardPrefab.TryGetComponent<HexUnit>(out var unit))
        {
            Debug.LogError("Unit is null");
        }
        HexGrid.Instance.AddUnit(BaseLogic.Instance.enemy, unit, HexGrid.Instance.GetCell(new HexCoordinates(x, z)));
    }

    [ClientRpc]
    public void EquipWeaponClientRpc(int x, int z, int cardId, ClientRpcParams clientRpcParams)
    {
        if (!CardDatabase.Instance.avaliableCards[cardId].cardTemplate.cardPrefab.TryGetComponent<WeaponBehavior>(out var weapon))
        {
            Debug.LogError("Weapon is null");
        }
        HexGrid.Instance.GetCell(new HexCoordinates(x, z)).unitFeature.GetComponent<HexUnit>().SetWeapon(weapon);
    }

    [ClientRpc]
    public void SyncUnitMovementClientRpc(int endX, int endZ, int unitIndex, ClientRpcParams clientRpcParams = default)
    {
        HexCell end = HexGrid.Instance.GetCell(new HexCoordinates(endX, endZ));
        HexUnit unit = BaseLogic.Instance.enemy.myUnits[unitIndex];
        UnitActionSystem.Instance.DoPathfinding(unit, end);
        unit.GetMoveAction().DoMove();
    }

    [ClientRpc]
    public void SyncUnitAttackClientRpc(int unitIndex, int targetType, int targetIndex, ClientRpcParams clientRpcParams = default)
    {
        HexUnit unit = BaseLogic.Instance.enemy.myUnits[unitIndex];
        UnitFeature target;
        if (targetType == 0)
        {
            target = BaseLogic.Instance.localFaction.myUnits[targetIndex];
        }
        else if (targetType == 1)
        {
            target = BaseLogic.Instance.localFaction.myBases[targetIndex];
        }
        else {
            Debug.LogError("targetType is neither 0 nor 1");
            return;
        }
        unit.GetAttackAction().DoAttack(target);
    }
}

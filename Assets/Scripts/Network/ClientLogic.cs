using CardSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class ClientLogic : BaseLogic
{
    public NetworkPlayer corresPlayer;

    public static new ClientLogic Instance { get; private set; }
    private new async void Start()
    {
        Instance = this;
        base.Start();
        corresPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkPlayer>();
        await InitMap();
    }
    public void OnCurrentActivePlayerChanged(int prev, int current)
    {
        StartOrStopTurn(current);
    }

    private void StartOrStopTurn(int current)
    {
        if (current == corresPlayer.GetPlayerID())
        {
            turnManagementSystem.BeginPlayerTurn();
            localFaction.refillActions();
            localFaction.enabled = true;
        }
        else
        {
            localFaction.enabled = false;
        }
    }

    public async Task InitMap()
    {
        HexGrid.Instance.LoadMap();
        HexGrid.Instance.LoadFeature();
        InitDeck(localFaction);
        //Todo: TurnManager.Instance.OnTurnChanged += TurnTransition;
        GameUIHandler.Instance.disableCanvasGroupRayCast();
        await WaitForPlayerIdAndInstances();
        localFaction.SetPlayerId((PlayerFaction)corresPlayer.GetPlayerID());
        enemy.SetPlayerId((PlayerFaction)corresPlayer.GetPlayerID() == PlayerFaction.Player1 ? PlayerFaction.Player2 : PlayerFaction.Player1);
        AssignFactionFeatures(localFaction, enemy);
        StartOrStopTurn(NetworkGroundTruth.Instance.GetCurrentActivePlayer().Value);
        NetworkGroundTruth.Instance.GetCurrentActivePlayer().OnValueChanged += OnCurrentActivePlayerChanged;
        LoadUnitBeforeGame();
        await WaitForUnits();
        UpdateUnitState();
    }

    private static void UpdateUnitState()
    {
        foreach (var unit in HexGrid.Instance.units)
        {
            if (unit.myPlayer.playerID == PlayerFaction.Player1)
            {
                unit.GetComponentInChildren<UnitWorldUI>().InitHealthBarUI(Color.green);
                unit.transform.rotation = Quaternion.Euler(0, 70, 0);
            }
            else
            {
                unit.GetComponentInChildren<UnitWorldUI>().InitHealthBarUI(Color.red);
            }
        }
        foreach (var myBase in HexGrid.Instance.bases)
        {
            if (myBase.myPlayer.playerID == PlayerFaction.Player1)
            {
                myBase.GetComponentInChildren<UnitWorldUI>().InitHealthBarUI(Color.green);
            }
            else
            {
                myBase.GetComponentInChildren<UnitWorldUI>().InitHealthBarUI(Color.red);
            }
        }
    }

    private async Task WaitForPlayerIdAndInstances()
    {
        while (corresPlayer.GetPlayerID() == 0 || GameServerRpc.Instance == null || GameClientRpc.Instance == null)
        {
            await Task.Yield();
        }
    }

    private async Task WaitForUnits()
    {
        while (HexGrid.Instance.units.Count != 4 || HexGrid.Instance.bases.Count != 2)
        {
            await Task.Yield();
        }

    }

    void LoadUnitBeforeGame()
    {
        AddUnit(warriorUnitPrefab, localFaction.myspawnPoints[1].location, 0);
        AddUnit(mageUnitPrefab, localFaction.myspawnPoints[0].location, 1);
    }

    //Todo:
    void TurnTransition(object sender, EventArgs e)
    {
        GameUIHandler.Instance.disableCanvasGroupRayCast();
    }
    public override void AddUnit(HexUnit unitToSpawn, HexCell locationToSpawn, int cardId)
    {
        HexGrid.Instance.AddUnit(localFaction, unitToSpawn, locationToSpawn);
        GameServerRpc.Instance.RequestUnitInstantiationServerRpc(locationToSpawn.Coordinates.X, locationToSpawn.Coordinates.Z, cardId);
    }
    public override void EquipWeapon(HexUnit unitToEquip, WeaponBehavior weaponToEquip, int cardId = 0)
    {
        unitToEquip.SetWeapon(weaponToEquip);
        GameServerRpc.Instance.RequestWeaponEquippingServerRpc(unitToEquip.location.Coordinates.X, unitToEquip.location.Coordinates.Z, cardId);
    }

    public override void EndPlayerTurn()
    {
        turnManagementSystem.EndPlayerTurn();
        GameServerRpc.Instance.PlayerTurnEndedServerRpc();
    }
    public override void DoAttack(HexUnit unit, UnitFeature target)
    {
        base.DoAttack(unit, target);
        int targetType;
        int targetIndex;
        if (unit.myPlayer.CurrentCell.unitFeature is HexUnit tempUnit)
        {
            targetType = 0;
            targetIndex = BaseLogic.Instance.enemy.myUnits.IndexOf(tempUnit);
        }
        else if (unit.myPlayer.CurrentCell.unitFeature is Base tempBase)
        {
            targetType = 1;
            targetIndex = BaseLogic.Instance.enemy.myBases.IndexOf(tempBase);
        }
        else
        {
            Debug.LogError("currentcell.unitFeature is neither a hexUnit nor a base");
            return;
        }

        GameServerRpc.Instance.SyncUnitAttackServerRpc(BaseLogic.Instance.localFaction.myUnits.IndexOf(unit), targetType, targetIndex);
    }


    public override void DoMove(HexUnit unit)
    {
        base.DoMove(unit);
        HexCoordinates end = HexGrid.Instance.currentPathTo.Coordinates;
        GameServerRpc.Instance.SyncUnitMovementServerRpc(end.X, end.Z, BaseLogic.Instance.localFaction.myUnits.IndexOf(unit));
    }
}
 
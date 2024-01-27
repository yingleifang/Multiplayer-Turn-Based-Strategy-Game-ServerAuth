using CardSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePlayerLogic : BaseLogic
{
    public bool deleteMode = false;

    // Start is called before the first frame update
    new void Start()
    {
        localFaction.SetPlayerId(PlayerFaction.Player1);
        enemy.SetPlayerId(PlayerFaction.Player2);
        HexGrid.Instance.LoadMap();
        HexGrid.Instance.LoadFeature();
        AssignFactionFeatures(localFaction, enemy);
        InitDeck(localFaction);
    }
    private void Update()
    {
        //if (IsMyTurn)
        //{
        //    localFaction.TakeAction();
        //}
    }
    public void UnsetDeleteMode()
    {
        var cursorHotspot = new Vector2(GameAssets.Instance.mainCursor.width / 2, GameAssets.Instance.mainCursor.height / 2);
        deleteMode = false;
        Cursor.SetCursor(GameAssets.Instance.mainCursor, cursorHotspot, CursorMode.Auto);
    }

    public override void AddUnit(HexUnit unitToSpawn, HexCell locationToSpawn, int cardId)
    {
        HexGrid.Instance.AddUnit(localFaction, unitToSpawn, locationToSpawn);
    }
    public override void EquipWeapon(HexUnit unitToEquip, WeaponBehavior weaponToEquip, int cardId = 0)
    {
        unitToEquip.SetWeapon(weaponToEquip);
    }

    public override void EndPlayerTurn()
    {
        turnManagementSystem.EndPlayerTurn();
    }
}

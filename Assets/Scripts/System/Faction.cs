using CardSystem;
using System;
using System.Collections.Generic;
using System.Security;
using UnityEngine;

public class Faction : MonoBehaviour
{
    public BaseLogic.PlayerFaction playerID { get; protected set; }
    public HexCell CurrentCell { get; protected set; }

    public List<HexUnit> myUnits;

    public List<Base> myBases;

    public List<SpawnPoint> myspawnPoints;
    public void SetPlayerId(BaseLogic.PlayerFaction id)
    {
        playerID = id;
    }



    private void Player_OnTurnChanged(object sender, EventArgs e)
    {
        //if (!GameManagerClient.Instance.isMyTurn)
        //{
        //    cardArea.HideAllCards();
        //    return;
        //}
        //curMana += manaRegen;
        //curMana = Math.Min(curMana, maxMana);
        //manaSystemUI.UpdateManaText(GameManagerClient.Instance.corresPlayer.curMana);
        //cardArea.FillSlots();
    }

    //Todo: To be rerwitten;
    protected HexCell FindNearestEnemyCell(HexCoordinates curUnitCoord)
    {
        List<HexUnit> enemyUnits;
        List<Base> enemyBases;
        HexCell res = null;
        //if (SinglePlayerLogic.Instance.corresPlayer == ServerLogic.Instance.player1)
        //{
        //    enemyUnits = ServerLogic.Instance.player2.myUnits;
        //    enemyBases = ServerLogic.Instance.player2.myBases;
        //}
        //else
        //{
        //    enemyUnits = ServerLogic.Instance.player1.myUnits;
        //    enemyBases = ServerLogic.Instance.player1.myBases;
        //}
        //int minDistance = Int32.MaxValue;
        //foreach (var enemyUnit in enemyUnits)
        //{
        //    int curDistance = enemyUnit.location.Coordinates.DistanceTo(curUnitCoord);
        //    if (curDistance < minDistance)
        //    {
        //        minDistance = curDistance;
        //        res = enemyUnit.location;
        //    }
        //}
        //foreach (var enemyBase in enemyBases)
        //{
        //    int curDistance = enemyBase.location.Coordinates.DistanceTo(curUnitCoord);
        //    if (curDistance < minDistance)
        //    {
        //        minDistance = curDistance;
        //        res = enemyBase.location;
        //    }
        //}
        return res;
    }

}

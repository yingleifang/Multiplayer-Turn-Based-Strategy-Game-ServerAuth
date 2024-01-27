using CardSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CompleteFaction : Faction
{
    public int startingMana = 9;

    public IntVariable PlayerMana;
    public Feature selectedFeature;
    bool CurrentCellChanged;

    public bool isAnimating;

    public enum PlayerState
    {
        NothingSelectedState,
        CardSelectedState,
        UnitSelectedState,
    }

    public PlayerState curState = PlayerState.NothingSelectedState;

    CardObject selectedCard;

    private void Awake()
    {
        PlayerMana.SetValue(startingMana);
    }
    void Start()
    {
        CurrentCell = HexGrid.Instance.Cells[0];
        HexGrid.Instance.ColorCells(Color.blue, CurrentCell);
    }
    private void Update()
    {
        if (isAnimating)
        {
            return;
        }
        TakeAction();
    }
    public virtual void TakeAction()
    {
        HexCell newCell = HexGrid.Instance.GetCellUnderCursor();
        if (newCell == null)
        {
            return;
        }
        HightlightCurCell(newCell);
        switch (curState)
        {
            case PlayerState.NothingSelectedState:
                if (Input.GetMouseButtonDown(0))
                {
                    DoSelection(HexGrid.Instance.GetCellUnderCursor());
                }
                break;
            case PlayerState.UnitSelectedState:
                HandleUnitSelectedAction();
                break;
            case PlayerState.CardSelectedState:
                HandleCardSelectedAction();
                break;
        }
    }

    private Feature GetCellFeature(HexCell cell)
    {
        if (cell == null)
        {
            return null;
        }
        if (cell.unitFeature != null)
        {
            return cell.unitFeature;
        }
        else if (cell.terrainFeature != null && cell.terrainFeature.myPlayer == BaseLogic.Instance.localFaction)
        {
            return cell.terrainFeature;
        }
        return null;
    }
    protected void DoSelection(HexCell cell)
    {
        HexGrid.Instance.ClearCellColor(Color.blue);
        HexGrid.Instance.ClearCellColor(Color.white);
        UpdateCurrentCell(cell);
        if (selectedFeature)
        {
            selectedFeature.RaiseFeatureDeSelectedEvent();
        }
        selectedFeature = GetCellFeature(cell);
        if (selectedFeature)
        {
            selectedFeature.RaiseFeatureSelectedEvent();
        }
        if (selectedFeature is HexUnit temp)
        {
            if (temp.canMove == true)
            {
                HexGrid.Instance.showMoveRange(temp.Location, temp);
                if (selectedFeature.myPlayer == BaseLogic.Instance.localFaction)
                {
                    curState = PlayerState.UnitSelectedState;
                }
            }
        }
        if (selectedFeature == null)
        {
            SetState(PlayerState.NothingSelectedState, null);
        }
    }
    void UpdateCurrentCell(HexCell cell)
    {
        if (cell != CurrentCell)
        {
            cell.DisableHighlight();
            CurrentCell = cell;
            CurrentCellChanged = true;
        }
        else
        {
            CurrentCellChanged = false;
        }
    }
    public void SetState(PlayerState state, CardObject card)
    {
        curState = state;
        selectedCard = card;
    }

    public void refillActions()
    {
        foreach (var unit in myUnits)
        {
            unit.reFillActions();
        }
        PlayerMana.SetValue(startingMana);
    }
    private void HandleCardSelectedAction()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var feature = GetCellFeature(HexGrid.Instance.GetCellUnderCursor());
            if (feature != null)
            {
                EffectResolutionSystem.Instance.ResolveCardEffects(selectedCard, feature);
                TargetVisualResolutionSystem.Instance.RevertHighlight();
            }
            selectedCard.SetState(CardObject.CardState.InHand);
            SetNothingSelectedState();
        }
    }

    private void SetNothingSelectedState()
    {
        curState = PlayerState.NothingSelectedState;
        selectedFeature = null;
    }

    private void HandleUnitSelectedAction()
    {
        if (selectedFeature is HexUnit temp && selectedFeature.myPlayer == BaseLogic.Instance.localFaction)
        {
            if (Input.GetMouseButtonDown(1))
            {
                UpdateCurrentCell(HexGrid.Instance.GetCellUnderCursor());
                if (CurrentCell.unitFeature == null)
                {
                    if (temp.canMove && HexGrid.Instance.CurrentPathExists)
                    {
                        BaseLogic.Instance.DoMove(temp);
                        return;
                    }
                }
                else if (temp.GetDamageType() == CardDatabase.DamageType.Physical && temp.canAttack && CurrentCell.unitFeature.myPlayer != this && UnitActionSystem.Instance.CanAttack(temp, CurrentCell))
                {
                    BaseLogic.Instance.DoAttack(temp, CurrentCell.unitFeature);
                    return;
                }else if (temp.GetDamageType() == CardDatabase.DamageType.Magical)
                {
                    BaseLogic.Instance.DoAttack(temp, CurrentCell.unitFeature);
                }
            }else if (CurrentCellChanged && temp.canMove && CurrentCell && CurrentCell.unitFeature == null)
            {
                UnitActionSystem.Instance.DoPathfinding(temp, CurrentCell);
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            DoSelection(HexGrid.Instance.GetCellUnderCursor());
        }
    }

    private void HightlightCurCell(HexCell newCell)
    {
        if (newCell != CurrentCell)
        {
            HexGrid.Instance.ClearCellColor(Color.blue);
            UpdateCurrentCell(newCell);
            HexGrid.Instance.ColorCells(Color.blue, CurrentCell);
        }
    }


}

using CardSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static CardSystem.CardObject;

public class TargetVisualResolutionSystem : MonoBehaviour
{
    public static TargetVisualResolutionSystem Instance { get; private set; }

    [SerializeField]
    private EffectResolutionSystem effectResolutionSystem;

    List<Feature> previousFetures = new List<Feature>();

    private void Awake()
    {
        Instance = this;
    }

    public void ResolveCardTargetVisual(CardObject card)
    {
        switch (card.Template.Type)
        {
            case EffectResolutionSystem.CardType.Weapon:
                HightlightUnitsOfType(CardDatabase.DamageType.Physical);
                break;
            case EffectResolutionSystem.CardType.Skill:
                HightlightUnitsOfType(CardDatabase.DamageType.Magical);
                break;
            case EffectResolutionSystem.CardType.SpecialEffect:
                break;
            case EffectResolutionSystem.CardType.Unit:
                HighlightAllSpawnPoints();
                break;
            
        }
    }

    private void HightlightUnitsOfType(CardDatabase.DamageType damageType)
    {
        List<HexUnit> units = new();
        foreach (var unit in BaseLogic.Instance.localFaction.myUnits)
        {
            if (unit.GetDamageType() == damageType)
            {
                units.Add(unit);
            }
        }
        HighlightUnits(units);
    }

    void HighlightAllSpawnPoints()
    {
        foreach (var feature in previousFetures)
        {
            feature.PotentialTargetIndicator.SetActive(false);
        }
        foreach (var spawnPoint in BaseLogic.Instance.localFaction.myspawnPoints)
        {
            if (spawnPoint.location.unitFeature != null)
                continue;
            previousFetures.Add(spawnPoint);
            spawnPoint.PotentialTargetIndicator.SetActive(true);
        }
    }
    void HighlightUnits(List<HexUnit> potentialTargets)
    {
        foreach (var feature in previousFetures)
        {
            feature.PotentialTargetIndicator.SetActive(false);
        }
        foreach (var unit in potentialTargets)
        {
            previousFetures.Add(unit);
            unit.PotentialTargetIndicator.SetActive(true);
        }
    }

    public void RevertHighlight()
    {
        foreach (var feature in previousFetures)
        {
            feature.PotentialTargetIndicator.SetActive(false);
        }
    }
}

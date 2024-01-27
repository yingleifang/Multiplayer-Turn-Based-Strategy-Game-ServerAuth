using CardSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore;

public abstract class BaseLogic : MonoBehaviour
{
    public static BaseLogic Instance;
    public enum PlayerFaction // PlayerFaction ID starts from 1
    {
        Player1 = 1,
        Player2 = 2
    }

    [SerializeField]
    protected HexUnit mageUnitPrefab;
    [SerializeField]
    protected HexUnit warriorUnitPrefab;

    public CompleteFaction localFaction;
    public BasicFaction enemy;
    public Canvas cardPileCanvas;
    public DeckDrawingSystem deckDrawingSystem;
    public TurnManagementSystem turnManagementSystem;

    public List<CardTemplate> playerDeck = new List<CardTemplate>();

    GameObject GameSystems;
    protected HandPresentationSystem handPresentationSystem;
    protected ObjectPool cardPool;
    protected CardDatabase cardLibrary;

    [SerializeField]
    CanvasGroup CardCanvasPrefab;
    [SerializeField]
    CardDatabase CardLibraryPrefab;
    [SerializeField]
    Canvas CardPileCanvasPrefab;
    [SerializeField]
    ObjectPool CardPoolPrefab;
    [SerializeField]
    GameObject GameSystemsPrefab;

    [SerializeField]
    CompleteFaction CompleteFactionPrefab;
    [SerializeField]
    BasicFaction BasicFactionPrefab;

    protected void Start()
    {
        Instance = this;
        Instantiate(CardCanvasPrefab);
        InstantiatePrefabWithReference();
        deckDrawingSystem = GameSystems.GetComponentInChildren<DeckDrawingSystem>();
        handPresentationSystem = GameSystems.GetComponentInChildren<HandPresentationSystem>();
        turnManagementSystem = GameSystems.GetComponentInChildren<TurnManagementSystem>();
    }
    private void InstantiatePrefabWithReference()
    {
        localFaction = Instantiate(CompleteFactionPrefab);
        enemy = Instantiate(BasicFactionPrefab);
        GameSystems = Instantiate(GameSystemsPrefab);
        cardPool = Instantiate(CardPoolPrefab);
        cardLibrary = Instantiate(CardLibraryPrefab);
        cardPileCanvas = Instantiate(CardPileCanvasPrefab);

    }
    protected void InitDeck(CompleteFaction faction)
    {
        GameUIHandler.Instance.manaWidget.Initialize(faction.PlayerMana);
        foreach (var card in cardLibrary.avaliableCards)
        {
            for (int i = 0; i < card.number; i++)
            {
                playerDeck.Add(card.cardTemplate);
            }
        }
        handPresentationSystem.Initialize(cardPool, GameUIHandler.Instance.discardPileWidgetTransform);
        deckDrawingSystem.LoadDeck(playerDeck);
    }

    protected void AssignFactionFeatures(Faction faction1, Faction faction2)
    {
        foreach (var target in HexGrid.Instance.bases)
        {
            if (target.playerID == faction1.playerID)
            {
                faction1.myBases.Add(target);
                target.myPlayer = faction1;
            }
            else
            {
                faction2.myBases.Add(target);
                target.myPlayer = faction2;
            }
        }
        foreach (var target in HexGrid.Instance.spawnPoints)
        {
            if (target.playerID == faction1.playerID)
            {
                faction1.myspawnPoints.Add(target);
                target.myPlayer = faction1;
            }
            else
            {
                faction2.myspawnPoints.Add(target);
                target.myPlayer = faction2;
            }
        }
    }
    public abstract void AddUnit(HexUnit unitToSpawn, HexCell locationToSpawn, int cardId = 0);
    public abstract void EquipWeapon(HexUnit unitToEquip, WeaponBehavior weaponToEquip, int cardId = 0);
    public abstract void EndPlayerTurn();
    public virtual void DoAttack(HexUnit unit, UnitFeature target)
    {
        unit.GetAttackAction().DoAttack(target);
    }

    public virtual void DoMove(HexUnit unit)
    {
        unit.GetMoveAction().DoMove();
    }
}

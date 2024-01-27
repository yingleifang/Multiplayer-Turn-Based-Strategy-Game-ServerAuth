using CardSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIHandler : MonoBehaviour
{
    public static GameUIHandler Instance { get; private set; }

    [SerializeField]
    public ManaWidget manaWidget;
    [SerializeField]
    public RectTransform discardPileWidgetTransform;
    [SerializeField]
    public List<RectTransform> CardTransforms;
    [SerializeField]
    public DeckButton DeckButton;
    [SerializeField]
    public GameObject TurnIndicator;
    [SerializeField]
    public TextMeshProUGUI TurnText;
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one TurnSystem!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [SerializeField]
    CanvasGroup canvasGroup;
    public void disableCanvasGroupRayCast()
    {
        canvasGroup.blocksRaycasts = false;
    }

    public void enableCanvasGroupRayCast()
    {
        canvasGroup.blocksRaycasts = true;
    }
}

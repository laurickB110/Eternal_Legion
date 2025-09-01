using UnityEngine;
using System.Collections.Generic;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }
    [SerializeField] Transform hand;
    [SerializeField] GameObject cardPrefab;
    private List<GameObject> cardsInHand = new List<GameObject>();
    private bool isPlayerTurn = true;

    void Awake()
    {
        // Sécurité : éviter les doublons de singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void AddCardToHand()
    {
        if (cardsInHand.Count >= 8) return; // Limite de cartes en main
        GameObject card = Instantiate(cardPrefab);
        cardsInHand.Add(card);
        Instance.UpdateDraggableStates();
    }

    public void RemoveCard(GameObject card)
    {
        if (cardsInHand.Contains(card))
        {
            cardsInHand.Remove(card);
            Destroy(card);
        }
    }

    public void UpdateDraggableStates()
    {
        foreach (GameObject card in cardsInHand)
        {
            var drag = card.GetComponent<DragAndDropCard>();
            if (drag != null)
                drag.SetDraggable(isPlayerTurn); // Autorise ou pas le drag
        }
    }
    
    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        UpdateDraggableStates();
    }

    public void EndPlayerTurn()
    {
        isPlayerTurn = false;
        UpdateDraggableStates();
    }

}

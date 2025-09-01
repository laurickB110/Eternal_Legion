using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class DisplayCard : MonoBehaviour
{
    [SerializeField] Image glowCard;
    public Card displayCard;
    public int displayId;

    private int id;
    private string cardName;
    private int cost;
    private int power;
    private int health;
    private string cardDescription;
    private Sprite spriteImage;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI powerText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI descriptionText;
    public Image artImage;

    public int numberOfCardsinDeck;

    private bool firstDisplay = true;

    void Start()
    {
        numberOfCardsinDeck = PlayerDeck.deckSize;

        displayCard = CardDatabase.cardList[displayId];

    }

    // Update is called once per frame
    void Update()
    {
        id = displayCard.id;
        cardName = displayCard.cardName;
        cost = displayCard.cost;
        power = displayCard.power;
        health = displayCard.health;
        cardDescription = displayCard.cardDescription;
        spriteImage = displayCard.spriteImage;

        nameText.text = "" + cardName;
        costText.text = "" + cost;
        powerText.text = "" + power;
        healthText.text = "" + health;
        descriptionText.text = "" + cardDescription;
        artImage.sprite = spriteImage;


        if (firstDisplay)
        {
            displayCard = PlayerDeck.staticDeck[numberOfCardsinDeck - 1];
            numberOfCardsinDeck -= 1;
            PlayerDeck.deckSize -= 1;
            firstDisplay = false;
        }
    }

    public int GetCost()
    {
        return cost;
    }

    public void SetHighlight(bool state)
    {
        if (glowCard != null)
        {
            glowCard.enabled = state;
        }
    }

    public int GetId()
    {
        return id;
    }

    public string GetName()
    {
        return cardName;
    }
    public int GetPower()
    {
        return power;
    }
    public int GetHealth()
    {
        return health;
    }
    public string GetDescription()
    {
        return cardDescription;
    }
}

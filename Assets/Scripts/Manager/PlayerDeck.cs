using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeck : MonoBehaviour
{
    public List<Card> container = new List<Card>();
    public int x;
    public static int deckSize;
    public List<Card> deck = new List<Card>();
    public static List<Card> staticDeck = new List<Card>();

    public GameObject cardInDeck1;
    public GameObject cardInDeck2;
    public GameObject cardInDeck3;
    public GameObject cardInDeck4;
    public GameObject cardInDeck5;
    public GameObject cardInDeck6;
    public GameObject cardInDeck7;
    public GameObject cardInDeck8;
    public GameObject cardInDeck9;
    public GameObject cardInDeck10;

    public GameObject CardToHand;
    public GameObject[] Clones;
    public GameObject Hand;
    void Start()
    {
        // x = 0;
        // deckSize = 20;
        // for (int i = 0; i < deckSize; i++)
        // {
        //     x = Random.Range(0, 5);
        //     deck[i] = CardDatabase.cardList[x];
        // }
        deck = CardDatabase.vikingDeck;
        deckSize = deck.Count;
        Shuffle();

        StartCoroutine(StartGame());
    }

    // Update is called once per frame
    void Update()
    {
        staticDeck = deck;

        if (deckSize < 30)
        {
            cardInDeck1.SetActive(false);
        }
        if (deckSize < 27)
        {
            cardInDeck2.SetActive(false);
        }
        if (deckSize < 24)
        {
            cardInDeck3.SetActive(false);
        }
        if (deckSize < 21)
        {
            cardInDeck4.SetActive(false);
        }
        if (deckSize < 18)
        {
            cardInDeck5.SetActive(false);
        }
        if (deckSize < 15)
        {
            cardInDeck6.SetActive(false);
        }
        if (deckSize < 12)
        {
            cardInDeck7.SetActive(false);
        }
        if (deckSize < 9)
        {
            cardInDeck8.SetActive(false);
        }
        if (deckSize < 6)
        {
            cardInDeck9.SetActive(false);
        }
        if (deckSize < 3)
        {
            cardInDeck10.SetActive(false);
        }

        if (TurnSystem.Instance.startTurn == true)
        {
            StartCoroutine(Draw(1));
            TurnSystem.Instance.startTurn = false;
        }
    }


    IEnumerator StartGame()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(1);
            HandManager.Instance.AddCardToHand();
        }
    }

    IEnumerator Draw(int number)
    {
        for (int i = 0; i < number; i++)
        {
            yield return new WaitForSeconds(1);
            HandManager.Instance.AddCardToHand();
        }
    }

    public void Shuffle()
    {
        for (int i = 0; i < deckSize; i++)
        {
            container[0] = deck[i];
            int randomIndex = Random.Range(i, deckSize);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = container[0];
        }
    }
}

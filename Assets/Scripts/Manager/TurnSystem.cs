using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TurnSystem : MonoBehaviour
{
    // --- Singleton ---
    public static TurnSystem Instance { get; private set; }

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    // --- Variables ---
    public bool isYourTurn;
    public int yourTurn;
    public int OpponentTurn;

    public TextMeshProUGUI turnText;
    private int maxMana;
    private int currentMana; // Player mana
    private int opponentMaxMana;
    private int opponentCurrentMana;
    public TextMeshProUGUI manaText;

    public bool startTurn;

    void Start()
    {
        isYourTurn = true;
        yourTurn = 1;
        OpponentTurn = 0;

        maxMana = 1;
        currentMana = 1;
        opponentMaxMana = 1;
        opponentCurrentMana = 1;

        startTurn = false;
        TimerManager.Instance.StartTurnTimer();
    }

    void Update()
    {
        turnText.text = isYourTurn ? "Your Turn" : "Opponent Turn";
        manaText.text = (isYourTurn ? currentMana : opponentCurrentMana).ToString();
    }

    public void EndTurn()
    {
        if (isYourTurn)
        {
            TimerManager.Instance.StopTurnTimer();
            HandManager.Instance.EndPlayerTurn();
            BoardManager.Instance.MobsCanMove(false);
            OpponentTurn += 1;

            // Start opponent turn: increase opponent mana and run simple AI
            if (opponentMaxMana < 8) opponentMaxMana += 1;
            opponentCurrentMana = opponentMaxMana;
            EternalLegion.AI.OpponentAI.TryStartOpponentTurn();
        }
        else
        {
            TimerManager.Instance.StartTurnTimer();
            HandManager.Instance.StartPlayerTurn();
            BoardManager.Instance.MobsCanMove(true);
            yourTurn += 1;
            if (maxMana < 8)
            {
                maxMana += 1;         
            }
            currentMana = maxMana;

            startTurn = true;
        }

        isYourTurn = !isYourTurn;
    }
    public void UseMana(int amount)
    {
        if (isYourTurn) currentMana -= amount; else opponentCurrentMana -= amount;
    }
    public int GetMana(){ return isYourTurn ? currentMana : opponentCurrentMana; }
    public int GetOpponentMana(){ return opponentCurrentMana; }
    public void UseOpponentMana(int amount){ opponentCurrentMana -= amount; }
}

using System;
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
    private int currentMana;
    public TextMeshProUGUI manaText;

    public bool startTurn;

    public enum PlayerSide { Blue, Red }
    public static event Action<PlayerSide> OnTurnStarted;
    public static event Action<PlayerSide> OnTurnEnded;

    void Start()
    {
        isYourTurn = true;
        yourTurn = 1;
        OpponentTurn = 0;

        maxMana = 1;
        currentMana = 1;

        startTurn = false;
        TimerManager.Instance.StartTurnTimer();

        // Notify Blue turn start at game begin
        OnTurnStarted?.Invoke(PlayerSide.Blue);
    }

    void Update()
    {
        turnText.text = isYourTurn ? "Your Turn" : "Opponent Turn";
        manaText.text = currentMana.ToString();
    }

    public void EndTurn()
    {
        if (isYourTurn)
        {
            // Blue ends turn
            OnTurnEnded?.Invoke(PlayerSide.Blue);
            TimerManager.Instance.StopTurnTimer();
            HandManager.Instance.EndPlayerTurn();
            BoardManager.Instance.EndTurn();
            OpponentTurn += 1;

            // Switch to Red turn
            isYourTurn = false;
            // Reset mana for opponent side as well
            currentMana = maxMana;
            OnTurnStarted?.Invoke(PlayerSide.Red);
        }
        else
        {
            // Red ends turn
            OnTurnEnded?.Invoke(PlayerSide.Red);
            TimerManager.Instance.StartTurnTimer();
            HandManager.Instance.StartPlayerTurn();
            BoardManager.Instance.StartTurn();
            yourTurn += 1;
            if (maxMana < 8)
            {
                maxMana += 1;         
            }
            currentMana = maxMana;

            startTurn = true;

            // Switch to Blue turn
            isYourTurn = true;
            OnTurnStarted?.Invoke(PlayerSide.Blue);
        }
    }
    public void UseMana(int amount)
    {
        currentMana -= amount;
    }
    public int GetMana(){
        return currentMana;
    }
}

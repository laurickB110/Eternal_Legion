using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private TurnSystem turnSystem;

    [Serializable]
    public class AICardOption
    {
        public GameObject prefab;
        public int cost = 1;
    }

    [Header("AI Options")] 
    [SerializeField] private List<AICardOption> aiCards = new List<AICardOption>();
    [SerializeField] private GameObject aiMobPrefab; // Prefab unique utilisé pour toutes les cartes AI
    [SerializeField] private float actionDelay = 0.2f;
    [SerializeField] public bool debugAI = false;

    // Deck/Hand IA (utilise CardDatabase.vikingDeck)
    private readonly List<Card> aiDeck = new List<Card>();
    private readonly List<Card> aiHand = new List<Card>();
    private bool aiInitialized = false;

    private const float cellSize = 2f;

    private void Awake()
    {
        if (boardManager == null)
            boardManager = FindFirstObjectByType<BoardManager>();
        if (turnSystem == null)
            turnSystem = FindFirstObjectByType<TurnSystem>();

        // Construire le deck IA depuis CardDatabase.vikingDeck
        aiDeck.Clear();
        aiHand.Clear();
        if (CardDatabase.vikingDeck != null)
        {
            foreach (var c in CardDatabase.vikingDeck)
                if (c != null) aiDeck.Add(c);
            Shuffle(aiDeck);
        }
    }

    private void OnEnable()
    {
        TurnSystem.OnTurnStarted += HandleTurnStarted;
    }

    private void OnDisable()
    {
        TurnSystem.OnTurnStarted -= HandleTurnStarted;
    }

    private void HandleTurnStarted(TurnSystem.PlayerSide side)
    {
        if (side == TurnSystem.PlayerSide.Red)
        {
            // (Re)charger le deck si vide (au cas où CardDatabase n'était pas initialisé à Awake)
            if (aiDeck.Count == 0 && CardDatabase.vikingDeck != null && CardDatabase.vikingDeck.Count > 0)
            {
                foreach (var c in CardDatabase.vikingDeck)
                    if (c != null) aiDeck.Add(c);
                Shuffle(aiDeck);
                if (debugAI) Debug.Log($"[AI] Loaded deck from CardDatabase at turn start. Size={aiDeck.Count}");
            }
            if (debugAI) Debug.Log("[AI] Red turn started");
            StartCoroutine(TakeTurn());
        }
    }

    private IEnumerator TakeTurn()
    {
        if (boardManager == null || turnSystem == null)
            yield break;

        // Draw phase (mirroir du joueur : 3 au premier tour IA, puis 1)
        if (!aiInitialized)
        {
            Draw(3);
            aiInitialized = true;
        }
        else
        {
            Draw(1);
        }

        // Enable actions for red team at turn start
        if (debugAI)
        {
            Debug.Log($"[AI] Setup: Red={boardManager.GetRedTeam().Count}, Blue={boardManager.GetBlueTeam().Count}, Mana={turnSystem.GetMana()}");
        }
        foreach (var go in boardManager.GetRedTeam())
        {
            var m = go != null ? go.GetComponent<Mob>() : null;
            if (m != null) m.StartOfTurn();
        }

        // Summon phase: try to play while mana allows and there is space
        yield return StartCoroutine(SummonWhilePossible());

        // Move phase
        yield return StartCoroutine(MoveAllRedMobs());

        // Attack phase
        yield return StartCoroutine(AttackWithAllRedMobs());

        // End red team turn flags
        foreach (var go in boardManager.GetRedTeam())
        {
            var m = go != null ? go.GetComponent<Mob>() : null;
            if (m != null) m.EndOfTurn();
        }

        // Pass turn back to player
        if (debugAI) Debug.Log("[AI] Red turn ended");
        turnSystem.EndTurn();
    }

    private IEnumerator SummonWhilePossible()
    {
        // protect against infinite loops
        int safety = 20;
        while (safety-- > 0)
        {
            var mana = turnSystem.GetMana();
            // Priorité: jouer depuis la main IA
            var playable = aiHand.Where(card => card != null && card.cost <= mana).ToList();
            if ((playable == null || playable.Count == 0))
            {
                if (debugAI)
                {
                    int cheapest = aiDeck.Count > 0 ? aiDeck.Where(c => c != null).Select(c => c.cost).DefaultIfEmpty(int.MaxValue).Min() : -1;
                    Debug.Log($"[AI] Cannot summon from hand: mana={mana}, hand={aiHand.Count}, playable={playable?.Count ?? 0}, cheapest-in-deck={cheapest}");
                }
                break; // Rien à jouer
            }

            var freeCases = boardManager.GetFreeCasesForTeam(Team.Red).ToList();
            if (freeCases.Count == 0)
            {
                if (debugAI) Debug.Log("[AI] No free red-half cells to summon.");
                break;
            }

            if (aiMobPrefab == null)
            {
                if (debugAI) Debug.LogWarning("[AI] aiMobPrefab is not assigned; cannot summon.");
                break;
            }

            // Choisir la carte la moins chère jouable, puis aléatoire si égalité
            int minCost = playable.Min(c => c.cost);
            var affordable = playable.Where(c => c.cost == minCost).ToList();
            var pickedCard = affordable[UnityEngine.Random.Range(0, affordable.Count)];
            var targetCase = freeCases[UnityEngine.Random.Range(0, freeCases.Count)];

            var spawned = boardManager.SpawnMobAt(targetCase, aiMobPrefab, Team.Red);
            if (spawned != null)
            {
                // Initialiser les stats du mob à partir de la carte
                var m = spawned.GetComponent<Mob>();
                if (m != null)
                {
                    m.Initialize(pickedCard.id, pickedCard.cardName, pickedCard.power, pickedCard.health, pickedCard.cardDescription);
                }

                // Consommer la carte & le mana
                aiHand.Remove(pickedCard);
                turnSystem.UseMana(pickedCard.cost);
                if (debugAI) Debug.Log($"[AI] Spawned {spawned.name} with card '{pickedCard.cardName}' (A={pickedCard.power}/H={pickedCard.health}) at {targetCase.transform.position}, cost {pickedCard.cost}");
                yield return new WaitForSeconds(actionDelay);
            }
            else
            {
                break;
            }
        }
    }

    private IEnumerator MoveAllRedMobs()
    {
        var red = boardManager.GetRedTeam().ToList();
        var blue = boardManager.GetBlueTeam().ToList();
        if (blue.Count == 0) yield break;
        if (debugAI) Debug.Log($"[AI] Move phase: red={red.Count}, blue={blue.Count}");

        foreach (var go in red)
        {
            if (go == null) continue;
            var m = go.GetComponent<Mob>();
            if (m == null || !m.CanMove()) continue;
            var current = m.GetCurrentCase();
            if (current == null) continue;

            // find nearest blue target
            Mob targetMob = FindNearestEnemy(m, blue);
            if (targetMob == null || targetMob.GetCurrentCase() == null) continue;

            var next = ChooseStepTowards(current, targetMob.GetCurrentCase());
            if (next != null && !next.IsOccupied() && !boardManager.IsBaseCase(next))
            {
                m.MoveTo(next.transform.position + new Vector3(0, 1, 0));
                m.SetCurrentCase(next);
                m.SetCanMove(false);
                if (debugAI) Debug.Log($"[AI] {m.name} moves to {next.transform.position}");
                yield return new WaitForSeconds(actionDelay);
            }
        }
    }

    private IEnumerator AttackWithAllRedMobs()
    {
        var red = boardManager.GetRedTeam().ToList();
        var blue = boardManager.GetBlueTeam().ToList();
        if (debugAI) Debug.Log($"[AI] Attack phase: red={red.Count}, blue={blue.Count}");
        foreach (var go in red)
        {
            if (go == null) continue;
            var m = go.GetComponent<Mob>();
            if (m == null || !m.CanAttack()) continue;
            var current = m.GetCurrentCase();
            if (current == null) continue;

            // find adjacent enemy
            var target = FindAdjacentEnemy(current, blue);
            if (target != null)
            {
                m.AttackMob(target);
                m.SetCanAttack(false);
                if (debugAI) Debug.Log($"[AI] {m.name} attacks {target.name}");
                yield return new WaitForSeconds(actionDelay);
            }
        }
    }

    private Mob FindNearestEnemy(Mob self, List<GameObject> enemies)
    {
        var current = self.GetCurrentCase();
        if (current == null) return null;

        Mob best = null;
        int bestDist = int.MaxValue;
        foreach (var e in enemies)
        {
            if (e == null) continue;
            var em = e.GetComponent<Mob>();
            if (em == null) continue;
            var ec = em.GetCurrentCase();
            if (ec == null) continue;
            int d = Manhattan(current.transform.position, ec.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = em;
            }
        }
        return best;
    }

    private Case ChooseStepTowards(Case from, Case target)
    {
        int currentDist = Manhattan(from.transform.position, target.transform.position);
        Case best = null;
        int bestDist = currentDist;
        foreach (var candidate in boardManager.GetAllCases())
        {
            if (candidate == null) continue;
            int step = Manhattan(from.transform.position, candidate.transform.position);
            if (step != 1) continue; // only one step away
            if (candidate.IsOccupied()) continue;
            if (boardManager.IsBaseCase(candidate)) continue;
            int d = Manhattan(candidate.transform.position, target.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = candidate;
            }
        }
        return best;
    }

    private Mob FindAdjacentEnemy(Case current, List<GameObject> enemies)
    {
        foreach (var e in enemies)
        {
            if (e == null) continue;
            var em = e.GetComponent<Mob>();
            if (em == null) continue;
            var ec = em.GetCurrentCase();
            if (ec == null) continue;
            if (Manhattan(current.transform.position, ec.transform.position) == 1)
                return em;
        }
        return null;
    }

    private int Manhattan(Vector3 a, Vector3 b)
    {
        int dx = Mathf.Abs(Mathf.RoundToInt((a.x - b.x) / cellSize));
        int dz = Mathf.Abs(Mathf.RoundToInt((a.z - b.z) / cellSize));
        return dx + dz;
    }

    private void Draw(int number)
    {
        for (int i = 0; i < number; i++)
        {
            if (aiDeck.Count == 0) return;
            var top = aiDeck[aiDeck.Count - 1];
            aiDeck.RemoveAt(aiDeck.Count - 1);
            if (top != null) aiHand.Add(top);
        }
        if (debugAI) Debug.Log($"[AI] Drew {number} card(s). Hand={aiHand.Count}, Deck={aiDeck.Count}");
    }

    private void Shuffle(List<Card> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, deck.Count);
            var tmp = deck[i];
            deck[i] = deck[r];
            deck[r] = tmp;
        }
    }
}

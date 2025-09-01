using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EternalLegion.AI;

namespace EternalLegion.AI
{
    // Simple, deterministic AI: draws 1, plays cheapest units it can afford, then steps toward nearest enemy
    public class OpponentAI : MonoBehaviour
    {
        public static OpponentAI Instance { get; private set; }

        [SerializeField] private GameObject mobPrefab; // Assign a basic mob prefab in Inspector
        [SerializeField] private bool useVikingDeckIfEgyptianEmpty = true;

        private readonly List<Card> deck = new List<Card>();
        private readonly List<Card> hand = new List<Card>();
        private bool initialized = false;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void InitIfNeeded()
        {
            if (initialized) return;
            // Populate deck from database
            if (CardDatabase.egyptianDeck != null && CardDatabase.egyptianDeck.Count > 0)
                deck.AddRange(CardDatabase.egyptianDeck);
            else if (useVikingDeckIfEgyptianEmpty && CardDatabase.vikingDeck != null)
                deck.AddRange(CardDatabase.vikingDeck);

            Shuffle(deck);
            initialized = true;
        }

        public static void TryStartOpponentTurn()
        {
            if (Instance == null)
            {
                var go = new GameObject("OpponentAI");
                Instance = go.AddComponent<OpponentAI>();
            }
            Instance.InitIfNeeded();
            Instance.StartCoroutine(Instance.PlayTurn());
        }

        private IEnumerator PlayTurn()
        {
            // Draw 1
            yield return new WaitForSeconds(0.3f);
            Draw(1);

            // Play as many as possible (cheapest first)
            while (hand.Count > 0)
            {
                hand.Sort((a,b) => a.cost.CompareTo(b.cost));
                var affordable = hand.FirstOrDefault(c => c.cost <= TurnSystem.Instance.GetOpponentMana());
                if (affordable == null) break;

                if (!TryPlayCard(affordable)) break; // can't place
                hand.Remove(affordable);
                yield return new WaitForSeconds(0.25f);
            }

            // Move red team units one step toward nearest enemy (or base)
            var reds = BoardManager.Instance.GetTeam(BoardManager.Team.Red);
            foreach (var mobGO in reds.ToList())
            {
                if (mobGO == null) continue;
                MoveTowardNearestEnemy(mobGO);
                yield return new WaitForSeconds(0.15f);
            }

            // End turn back to player
            yield return new WaitForSeconds(0.25f);
            TurnSystem.Instance.EndTurn();
        }

        private void Draw(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (deck.Count == 0) return;
                var top = deck[0];
                deck.RemoveAt(0);
                hand.Add(top);
            }
        }

        private bool TryPlayCard(Card card)
        {
            if (mobPrefab == null)
            {
                // Fallback: try to use the same prefab as the player's card drag component
                var drag = GameObject.FindObjectOfType<DragAndDropCard>();
                if (drag != null) mobPrefab = drag.mob;
            }
            if (mobPrefab == null) return false;
            // Prefer spawning 1-3 cells from a blue unit; fallback to base vicinity
            var spawnCase = BoardManager.Instance.FindSpawnNearEnemies(BoardManager.Team.Red, BoardManager.Team.Blue, 1, 3);
            if (spawnCase == null)
                spawnCase = BoardManager.Instance.FindBestSpawnCase(BoardManager.Team.Red);
            if (spawnCase == null) return false;

            var mob = GameObject.Instantiate(mobPrefab, spawnCase.transform.position + new Vector3(0,1,0), Quaternion.identity);
            var mobComp = mob.GetComponent<Mob>();
            if (mobComp != null)
            {
                mobComp.Initialize(card.id, card.cardName, card.power, card.health, card.cardDescription);
                mobComp.SetCurrentCase(spawnCase);
            }
            spawnCase.SetOccupied(true, mobComp);
            BoardManager.Instance.AddMobToBoard(mob, BoardManager.Team.Red);
            TurnSystem.Instance.UseOpponentMana(card.cost);
            return true;
        }

        private void MoveTowardNearestEnemy(GameObject red)
        {
            var blueList = BoardManager.Instance.GetTeam(BoardManager.Team.Blue);
            Vector3 targetPos;
            if (blueList.Count == 0 || blueList.All(b => b == null))
            {
                // fallback: move toward enemy base (blue)
                targetPos = red.transform.position + (BoardManager.Instance.transform.position); // will be replaced below
                // Find blue base via spawn case of blue team
                var blueSpawn = BoardManager.Instance.FindBestSpawnCase(BoardManager.Team.Blue);
                targetPos = blueSpawn != null ? blueSpawn.transform.position : red.transform.position;
            }
            else
            {
                float best = float.MaxValue;
                targetPos = blueList[0] != null ? blueList[0].transform.position : red.transform.position;
                foreach (var b in blueList)
                {
                    if (b == null) continue;
                    float d = Vector3.SqrMagnitude(b.transform.position - red.transform.position);
                    if (d < best)
                    {
                        best = d; targetPos = b.transform.position;
                    }
                }
            }

            var redMob = red.GetComponent<Mob>();
            var redMobComp = red.GetComponent<Mob>();
            var currentCase = redMobComp != null ? redMobComp.GetCurrentCase() : BoardManager.Instance.FindCaseAtPosition(red.transform.position);
            if (currentCase == null) return;

            // If already adjacent to a blue unit, don't move this turn
            int nearestBlueDist = int.MaxValue;
            foreach (var b in blueList)
            {
                if (b == null) continue;
                var bc = BoardManager.Instance.FindCaseAtPosition(b.transform.position);
                if (bc == null) continue;
                int d = ManhattanCells(currentCase.transform.position, bc.transform.position);
                if (d < nearestBlueDist) nearestBlueDist = d;
            }
            if (nearestBlueDist <= 1) return;
            var stepCase = BoardManager.Instance.FindStepToward(currentCase.transform.position, targetPos);
            if (stepCase == null) return;

            redMob.MoveTo(stepCase.transform.position + new Vector3(0,1,0));
            redMob.SetCurrentCase(stepCase);
            redMob.SetCanMove(false);
        }

        private int ManhattanCells(Vector3 a, Vector3 b)
        {
            const float cellSize = 2f;
            int dx = Mathf.Abs(Mathf.RoundToInt((a.x - b.x) / cellSize));
            int dz = Mathf.Abs(Mathf.RoundToInt((a.z - b.z) / cellSize));
            return dx + dz;
        }

        private void Shuffle<T>(IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = Random.Range(i, list.Count);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}

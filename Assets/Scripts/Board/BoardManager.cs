using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }
    public enum Team { Blue, Red }

    [SerializeField] GameObject[] cases;
    [SerializeField] GameObject redBase;
    [SerializeField] GameObject blueBase;
    [SerializeField] int baseRestrictedRadiusCells = 1; // cells around each base where units cannot enter

    private List<GameObject> redTeam = new List<GameObject>();
    private List<GameObject> blueTeam = new List<GameObject>();
    private List<GameObject> allMobs = new List<GameObject>();
    private bool inAction = false;
    private bool wasClickedThisFrame = false;

    private Mob selectedMob;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void HighlightMovableCells(Vector3 mobPos, int range)
    {
        float caseSize = 2f;
        foreach (GameObject c in cases)
        {
            Vector3 cellPos = c.transform.position;
            int distX = Mathf.Abs(Mathf.RoundToInt((mobPos.x - cellPos.x) / caseSize));
            int distZ = Mathf.Abs(Mathf.RoundToInt((mobPos.z - cellPos.z) / caseSize));

            int manhattanDistance = distX + distZ;

            if (manhattanDistance <= range)
            {
                c.GetComponent<Case>().Highlight(true);
            }
        }
        DisableCollidersMobs();
    }

    public void ClearHighlights()
    {
        foreach (GameObject c in cases)
        {
            c.GetComponent<Case>().Highlight(false);
        }
    }

    public void EnableCollidersMobs()
    {
        foreach (GameObject mob in allMobs)
        {
            Collider mobCollider = mob.GetComponentInChildren<Collider>();
            Debug.Log(mobCollider);
            if (mobCollider != null)
            {
                mobCollider.enabled = true;
            }
        }
    }

    public void DisableCollidersMobs()
    {
        foreach (GameObject mob in allMobs)
        {
            Collider mobCollider = mob.GetComponentInChildren<Collider>();
            if (mobCollider != null)
            {
                mobCollider.enabled = false;
            }
        }
    }

    public void MobsCanMove(bool state)
    {
        // Only allow player's units (Blue) to be clicked/moved
        foreach (GameObject mob in blueTeam)
        {
            if (mob == null) continue;
            var m = mob.GetComponent<Mob>();
            if (m != null) m.SetCanMove(state);
        }
        foreach (GameObject mob in redTeam)
        {
            if (mob == null) continue;
            var m = mob.GetComponent<Mob>();
            if (m != null) m.SetCanMove(false);
        }
    }

    public void AddMobToBoard(GameObject mob)
    {
        // Default to Blue team for backward compatibility
        AddMobToBoard(mob, Team.Blue);
    }

    public void AddMobToBoard(GameObject mob, Team team)
    {
        if (mob == null) return;
        if (!allMobs.Contains(mob)) allMobs.Add(mob);
        if (team == Team.Blue)
        {
            if (!blueTeam.Contains(mob)) blueTeam.Add(mob);
        }
        else
        {
            if (!redTeam.Contains(mob)) redTeam.Add(mob);
        }
    }

    public System.Collections.Generic.IReadOnlyList<GameObject> GetTeam(Team team)
    {
        return team == Team.Blue ? (System.Collections.Generic.IReadOnlyList<GameObject>)blueTeam : redTeam;
    }

    public bool IsInAction()
    {
        return inAction;
    }

    public void SetSelectedMob(Mob mob)
    {
        selectedMob = mob;
    }

    public void SetClickedThisFrame(bool state)
    {
        wasClickedThisFrame = state;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (selectedMob == null) return;
            if (wasClickedThisFrame)
            {
                wasClickedThisFrame = false;
                return; // Ignore ce clic dans Update car déjà traité
            }

            float caseSize = 2f;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject caseTouched = hit.collider.gameObject;
                if (hit.collider.name.StartsWith("Case"))
                {
                    Vector3 mobPos = selectedMob.transform.position;
                    Vector3 targetPos = hit.collider.transform.position;

                    int distX = Mathf.Abs(Mathf.RoundToInt((mobPos.x - targetPos.x) / caseSize));
                    int distZ = Mathf.Abs(Mathf.RoundToInt((mobPos.z - targetPos.z) / caseSize));
                    int manhattanDist = distX + distZ;

                    var targetCase = caseTouched.GetComponent<Case>();
                    if (manhattanDist <= selectedMob.GetMovementRange() && !targetCase.IsOccupied() && !IsBaseRestrictedCase(targetCase))
                    {
                        selectedMob.MoveTo(targetPos + new Vector3(0, 1, 0)); // léger offset en Y
                        selectedMob.SetCurrentCase(targetCase);
                        selectedMob.SetCanMove(false);
                    }
                }
            }
            selectedMob = null;
            ClearHighlights();
            EnableCollidersMobs();
        }
    }

    // --- Helpers for AI ---
    private const float CellSize = 2f;

    public Case FindBestSpawnCase(Team team)
    {
        Vector3 origin = team == Team.Red ? redBase.transform.position : blueBase.transform.position;
        float best = float.MaxValue;
        Case bestCase = null;
        foreach (GameObject c in cases)
        {
            var caseComp = c.GetComponent<Case>();
            if (caseComp == null || caseComp.IsOccupied() || IsBaseRestrictedCase(caseComp)) continue;
            float d = Vector3.SqrMagnitude(c.transform.position - origin);
            if (d < best)
            {
                best = d;
                bestCase = caseComp;
            }
        }
        return bestCase;
    }

    // Find a free case at Manhattan distance in [minRange, maxRange] from any enemy unit
    public Case FindSpawnNearEnemies(Team myTeam, Team enemyTeam, int minRange, int maxRange)
    {
        var enemies = GetTeam(enemyTeam);
        if (enemies == null || enemies.Count == 0) return null;

        Case best = null;
        int bestDist = int.MaxValue;
        foreach (var go in cases)
        {
            var cc = go.GetComponent<Case>();
            if (cc == null || cc.IsOccupied() || IsBaseRestrictedCase(cc)) continue;

            int closest = int.MaxValue;
            for (int i = 0; i < enemies.Count; i++)
            {
                var e = enemies[i];
                if (e == null) continue;
                var ec = FindCaseAtPosition(e.transform.position);
                if (ec == null) continue;
                int d = ManhattanCells(go.transform.position, ec.transform.position);
                if (d < closest) closest = d;
            }

            if (closest >= minRange && closest <= maxRange)
            {
                if (closest < bestDist)
                {
                    bestDist = closest;
                    best = cc;
                }
            }
        }

        return best;
    }

    private int ManhattanCells(Vector3 a, Vector3 b)
    {
        int dx = Mathf.Abs(Mathf.RoundToInt((a.x - b.x) / CellSize));
        int dz = Mathf.Abs(Mathf.RoundToInt((a.z - b.z) / CellSize));
        return dx + dz;
    }

    public bool IsBaseRestrictedCase(Case c)
    {
        if (c == null) return false;
        Vector3 p = c.transform.position;
        int dBlue = ManhattanCells(p, blueBase.transform.position);
        int dRed  = ManhattanCells(p, redBase.transform.position);
        return (dBlue <= baseRestrictedRadiusCells) || (dRed <= baseRestrictedRadiusCells);
    }

    public Case FindStepToward(Vector3 from, Vector3 to)
    {
        // One-step Manhattan move toward target on free cell
        Vector3 dir = to - from;
        Vector3 step = Vector3.zero;
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.z))
            step = new Vector3(Mathf.Sign(dir.x) * CellSize, 0, 0);
        else
            step = new Vector3(0, 0, Mathf.Sign(dir.z) * CellSize);

        // Try primary axis then the other if blocked
        Vector3[] candidates = new Vector3[] { from + step, from + new Vector3(0,0, Mathf.Sign(dir.z) * CellSize), from + new Vector3(Mathf.Sign(dir.x) * CellSize,0,0) };
        foreach (var p in candidates)
        {
            var c = FindCaseAtPosition(p);
            if (c != null && !c.IsOccupied() && !IsBaseRestrictedCase(c)) return c;
        }
        return null;
    }

    public Case FindCaseAtPosition(Vector3 pos)
    {
        foreach (GameObject c in cases)
        {
            // Compare on XZ plane; ignore Y offset between mob (y=1) and case (y=0)
            var a = new Vector2(c.transform.position.x, c.transform.position.z);
            var b = new Vector2(pos.x, pos.z);
            if (Vector2.Distance(a, b) < 0.6f)
                return c.GetComponent<Case>();
        }
        return null;
    }
}

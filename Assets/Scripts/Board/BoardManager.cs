using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [SerializeField] GameObject[] cases;
    [SerializeField] GameObject redBase;
    [SerializeField] GameObject blueBase;

    private List<GameObject> redTeam = new List<GameObject>();
    private List<GameObject> blueTeam = new List<GameObject>();
    private List<GameObject> allMobs = new List<GameObject>();
    private bool inAction = false;
    private bool wasClickedThisFrame = false;

    private Mob selectedMob;

    private float? cachedMidX = null;
    private Case redBaseCaseCache;
    private Case blueBaseCaseCache;

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

    public void HighlightAttackableCells(Vector3 mobPos, int range)
    {
        float caseSize = 2f;
        foreach (GameObject c in cases)
        {
            Vector3 cellPos = c.transform.position;
            int distX = Mathf.Abs(Mathf.RoundToInt((mobPos.x - cellPos.x) / caseSize));
            int distZ = Mathf.Abs(Mathf.RoundToInt((mobPos.z - cellPos.z) / caseSize));

            int manhattanDistance = distX + distZ;

            if (manhattanDistance <= range && c.GetComponent<Case>() != selectedMob.GetCurrentCase())
            {
                c.GetComponent<Case>().HighlightAsAttackable(true);
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

    public void ClearAttackHighlights()
    {
        foreach (GameObject c in cases)
        {
            c.GetComponent<Case>().HighlightAsAttackable(false);
        }
    }

    public void EnableCollidersMobs()
    {
        foreach (GameObject mob in GetAllMobs())
        {
            Collider mobCollider = mob.GetComponentInChildren<Collider>();
            if (mobCollider != null)
            {
                mobCollider.enabled = true;
            }
        }
    }

    public void DisableCollidersMobs()
    {
        foreach (GameObject mob in GetAllMobs())
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
        foreach (GameObject mob in GetAllMobs())
        {
            mob.GetComponent<Mob>().SetCanMove(state);
        }
    }

    public void AddMobToBoard(GameObject mob)
    {
        allMobs.Add(mob);
    }

    public void AddMobToRedTeam(GameObject mob)
    {
        redTeam.Add(mob);
        var m = mob.GetComponent<Mob>();
        if (m != null) m.Team = Team.Red;
    }

    public void AddMobToBlueTeam(GameObject mob)
    {
        blueTeam.Add(mob);
        var m = mob.GetComponent<Mob>();
        if (m != null) m.Team = Team.Blue;
    }

    public void RemoveMobFromBoard(GameObject mob)
    {
        if (allMobs.Contains(mob))
        {
            allMobs.Remove(mob);    
        }
    }

    public void RemoveMobFromRedTeam(GameObject mob)
    {
        if (redTeam.Contains(mob))
        {
            redTeam.Remove(mob);    
        }
    }

    public void RemoveMobFromBlueTeam(GameObject mob)
    {
        if (blueTeam.Contains(mob))
        {
            blueTeam.Remove(mob); 
        }
    }

    public IEnumerable<GameObject> GetAllMobs()
    {
        // Prefer union of team lists to avoid divergence
        return blueTeam.Concat(redTeam);
    }

    public IReadOnlyList<GameObject> GetBlueTeam() => blueTeam;
    public IReadOnlyList<GameObject> GetRedTeam() => redTeam;

    public GameObject SpawnMobAt(Case targetCase, GameObject mobPrefab, Team team)
    {
        if (targetCase == null || mobPrefab == null) return null;
        if (targetCase.IsOccupied()) return null;
        if (IsBaseCase(targetCase)) return null;

        GameObject instance = Instantiate(mobPrefab, targetCase.transform.position + new Vector3(0, 1, 0), Quaternion.identity);
        var mobComp = instance.GetComponent<Mob>();
        if (mobComp != null)
        {
            mobComp.SetCurrentCase(targetCase);
        }
        AddMobToBoard(instance);
        if (team == Team.Blue)
        {
            AddMobToBlueTeam(instance);
        }
        else
        {
            AddMobToRedTeam(instance);
        }
        return instance;
    }

    private float GetBoardMidX()
    {
        if (cachedMidX.HasValue) return cachedMidX.Value;
        if (cases == null || cases.Length == 0)
        {
            cachedMidX = 0f;
            return cachedMidX.Value;
        }
        float minX = float.MaxValue, maxX = float.MinValue;
        foreach (var c in cases)
        {
            if (c == null) continue;
            float x = c.transform.position.x;
            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
        }
        cachedMidX = (minX + maxX) * 0.5f;
        return cachedMidX.Value;
    }

    public bool IsBaseCase(Case c)
    {
        if (c == null) return false;

        // Resolve nearest case to base objects once, if provided
        if (redBaseCaseCache == null && redBase != null)
        {
            redBaseCaseCache = FindNearestCaseTo(redBase.transform.position);
        }
        if (blueBaseCaseCache == null && blueBase != null)
        {
            blueBaseCaseCache = FindNearestCaseTo(blueBase.transform.position);
        }

        if (redBaseCaseCache != null && c == redBaseCaseCache) return true;
        if (blueBaseCaseCache != null && c == blueBaseCaseCache) return true;

        return false;
    }

    private Case FindNearestCaseTo(Vector3 position)
    {
        Case best = null;
        float bestSqr = float.PositiveInfinity;
        foreach (var g in cases)
        {
            if (g == null) continue;
            var cc = g.GetComponent<Case>();
            if (cc == null) continue;
            float d = (cc.transform.position - position).sqrMagnitude;
            if (d < bestSqr)
            {
                bestSqr = d;
                best = cc;
            }
        }
        return best;
    }

    public bool IsInBlueHalf(Case c)
    {
        if (c == null) return false;
        float mid = GetBoardMidX();
        bool redIsRight = true;
        if (redBase != null)
            redIsRight = redBase.transform.position.x > mid;
        // Blue is the opposite half of Red
        if (redIsRight)
            return c.transform.position.x <= mid;
        else
            return c.transform.position.x > mid;
    }

    public bool IsInRedHalf(Case c)
    {
        if (c == null) return false;
        float mid = GetBoardMidX();
        bool redIsRight = true;
        if (redBase != null)
            redIsRight = redBase.transform.position.x > mid;
        if (redIsRight)
            return c.transform.position.x > mid;
        else
            return c.transform.position.x <= mid;
    }

    public IEnumerable<Case> GetFreeCasesForTeam(Team t)
    {
        foreach (var g in cases)
        {
            if (g == null) continue;
            var c = g.GetComponent<Case>();
            if (c == null) continue;
            if (c.IsOccupied()) continue;
            if (IsBaseCase(c)) continue;
            if (t == Team.Blue && !IsInBlueHalf(c)) continue;
            if (t == Team.Red && !IsInRedHalf(c)) continue;
            yield return c;
        }
    }

    public IEnumerable<Case> GetAllCases()
    {
        foreach (var g in cases)
        {
            if (g == null) continue;
            var c = g.GetComponent<Case>();
            if (c == null) continue;
            yield return c;
        }
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

    public void EndTurn()
    {
        foreach (GameObject mob in blueTeam)
        {
            mob.GetComponent<Mob>().EndOfTurn();

        }
        UIManager.Instance.HideMenu();
    }

    public void StartTurn()
    {
        foreach (GameObject mob in blueTeam)
        {
            mob.GetComponent<Mob>().StartOfTurn();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (selectedMob == null) return;

            if (selectedMob.IsOnMove())
            {
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

                        if (manhattanDist <= selectedMob.GetMovementRange() && !caseTouched.GetComponent<Case>().IsOccupied())
                        {
                            selectedMob.MoveTo(targetPos + new Vector3(0, 1, 0)); // léger offset en Y
                            selectedMob.SetCurrentCase(caseTouched.GetComponent<Case>());
                            selectedMob.SetCanMove(false);
                        }
                    }
                }
                selectedMob.SetOnMove(false);
                selectedMob = null;
                ClearHighlights();
                EnableCollidersMobs();
            }
            
            else if (selectedMob.IsOnAttack())
            {
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

                        if (manhattanDist <= selectedMob.GetAttackRange() && caseTouched.GetComponent<Case>().IsOccupied() && caseTouched.GetComponent<Case>() != selectedMob.GetCurrentCase())
                        {
                            // Mettre ici la logique pour l'attaque
                            Mob targetMob = caseTouched.GetComponent<Case>().GetOccupyingMob();
                            selectedMob.AttackMob(targetMob);
                            Debug.Log($"{selectedMob.name} attaque la case en {targetPos}");
                            selectedMob.SetCanAttack(false);
                        }
                    }
                }
                selectedMob.SetOnAttack(false);
                selectedMob = null;
                ClearAttackHighlights();
                EnableCollidersMobs();
            }


        }
    }
}

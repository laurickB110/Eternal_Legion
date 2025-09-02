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
    [SerializeField] float baseExclusionMargin = 1.0f; // expand XZ bounds to fully cover underlying cases
    private List<Case> redBaseCasesCache = null;
    private List<Case> blueBaseCasesCache = null;

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

    void Start()
    {
        BuildBaseAreasIfNeeded();
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

        GameObject instance = Instantiate(mobPrefab, targetCase.transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
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
        Debug.Log("IsBaseCase check", c);
        if (c == null) return false;
        if (redBaseCasesCache != null && redBaseCasesCache.Contains(c)) return true;
        if (blueBaseCasesCache != null && blueBaseCasesCache.Contains(c)) return true;
        return false;
    }

    // Returns the Base component whose area covers the given case, or null
    public Base GetBaseUnderCase(Case c)
    {
        if (c == null) return null;
        BuildBaseAreasIfNeeded();
        if (redBaseCasesCache != null && redBaseCasesCache.Contains(c))
        {
            EnsureBaseReferences();
            return redBase != null ? redBase.GetComponent<Base>() : null;
        }
        if (blueBaseCasesCache != null && blueBaseCasesCache.Contains(c))
        {
            EnsureBaseReferences();
            return blueBase != null ? blueBase.GetComponent<Base>() : null;
        }
        return null;
    }

    private void BuildBaseAreasIfNeeded()
    {
        if (redBaseCasesCache == null || blueBaseCasesCache == null)
        {
            redBaseCasesCache = new List<Case>();
            blueBaseCasesCache = new List<Case>();

            EnsureBaseReferences();

            if (redBase != null)
            {
                var area = GetXZBounds(redBase);
                if (area.HasValue)
                {
                    foreach (var c in GetAllCases())
                    {
                        Vector3 p = c.transform.position;
                        if (IsInsideXZ(area.Value, p)) redBaseCasesCache.Add(c);
                    }
                }
            }

            if (blueBase != null)
            {
                var area = GetXZBounds(blueBase);
                if (area.HasValue)
                {
                    foreach (var c in GetAllCases())
                    {
                        Vector3 p = c.transform.position;
                        if (IsInsideXZ(area.Value, p)) blueBaseCasesCache.Add(c);
                    }
                }
            }

            // Fallback: if one or both bases not found, try to infer by name and side
            if (redBase == null || blueBase == null)
            {
                var allBaseLike = FindBaseLikeObjects();
                float mid = GetBoardMidX();
                foreach (var go in allBaseLike)
                {
                    var area2 = GetXZBounds(go);
                    if (!area2.HasValue) continue;
                    bool goesToRed = go.transform.position.x > mid; // right side assumed red
                    foreach (var c in GetAllCases())
                    {
                        Vector3 p = c.transform.position;
                        if (IsInsideXZ(area2.Value, p))
                        {
                            if (goesToRed)
                            {
                                if (!redBaseCasesCache.Contains(c)) redBaseCasesCache.Add(c);
                            }
                            else
                            {
                                if (!blueBaseCasesCache.Contains(c)) blueBaseCasesCache.Add(c);
                            }
                        }
                    }
                }
            }
        }
    }

    private void EnsureBaseReferences()
    {
        if (redBase == null)
        {
            redBase = FindByNameSubstring("RedBase") ?? FindByNameSubstring("Red Base");
        }
        if (blueBase == null)
        {
            blueBase = FindByNameSubstring("BlueBase") ?? FindByNameSubstring("Blue Base");
        }
    }

    private GameObject FindByNameSubstring(string contains)
    {
        var all = FindObjectsOfType<Transform>();
        foreach (var t in all)
        {
            if (t == null || t.gameObject == null) continue;
            string name = t.gameObject.name;
            if (name != null && name.IndexOf(contains, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return t.gameObject;
            }
        }
        return null;
    }

    private List<GameObject> FindBaseLikeObjects()
    {
        var list = new List<GameObject>();
        var all = FindObjectsOfType<Transform>();
        foreach (var t in all)
        {
            if (t == null || t.gameObject == null) continue;
            string name = t.gameObject.name;
            if (string.IsNullOrEmpty(name)) continue;
            if (name.IndexOf("Base", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                list.Add(t.gameObject);
            }
        }
        return list;
    }

    private struct XZBounds
    {
        public float minX, maxX, minZ, maxZ;
    }

    private XZBounds? GetXZBounds(GameObject root)
    {
        if (root == null) return null;
        bool any = false;
        var renderers = root.GetComponentsInChildren<Renderer>();
        Bounds b = new Bounds(root.transform.position, Vector3.zero);
        foreach (var r in renderers)
        {
            if (r == null) continue;
            if (!any) { b = r.bounds; any = true; }
            else b.Encapsulate(r.bounds);
        }
        if (!any)
        {
            var colliders = root.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                if (col == null) continue;
                if (!any) { b = col.bounds; any = true; }
                else b.Encapsulate(col.bounds);
            }
        }
        if (!any) return null;

        return new XZBounds
        {
            minX = b.min.x - baseExclusionMargin,
            maxX = b.max.x + baseExclusionMargin,
            minZ = b.min.z - baseExclusionMargin,
            maxZ = b.max.z + baseExclusionMargin
        };
    }

    private bool IsInsideXZ(XZBounds bounds, Vector3 p)
    {
        return p.x >= bounds.minX && p.x <= bounds.maxX && p.z >= bounds.minZ && p.z <= bounds.maxZ;
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
            foreach(Case c in redBaseCasesCache)
            {
                Debug.Log("redbasecase", c);
            }
            foreach(Case c in blueBaseCasesCache)
            {
                Debug.Log("bluebasecase", c);
            }

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
                            selectedMob.MoveTo(targetPos + new Vector3(0, 0.5f, 0)); // léger offset en Y
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

                        if (manhattanDist <= selectedMob.GetAttackRange() && caseTouched.GetComponent<Case>() != selectedMob.GetCurrentCase())
                        {
                            var targetCase = caseTouched.GetComponent<Case>();

                            // 1) If a mob is on the case, attack it
                            if (targetCase.IsOccupied())
                            {
                                Mob targetMob = targetCase.GetOccupyingMob();
                                selectedMob.AttackMob(targetMob);
                                Debug.Log($"{selectedMob.name} attaque la case en {targetPos}");
                                selectedMob.SetCanAttack(false);
                            }
                            // 2) Else, if this case belongs to a base area, attack the base
                            else if (IsBaseCase(targetCase))
                            {
                                var targetBase = GetBaseUnderCase(targetCase);
                                if (targetBase != null)
                                {
                                    selectedMob.AttackMob(targetBase);
                                    Debug.Log($"{selectedMob.name} attaque la base via la case en {targetPos}");
                                    selectedMob.SetCanAttack(false);
                                }
                            }
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

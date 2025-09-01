using UnityEngine;
using System.Collections.Generic;

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
        foreach (GameObject mob in allMobs)
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
        foreach (GameObject mob in allMobs)
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
    }

    public void AddMobToBlueTeam(GameObject mob)
    {
        blueTeam.Add(mob);
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

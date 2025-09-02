using UnityEngine;
using System.Collections;
using TMPro;

public enum Team { Blue, Red }

public class Mob : MonoBehaviour, IAttackable
{
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI attackText;
    private int id;
    private string mobName;
    private int attack;
    private int health;
    private string mobDescription;

    private int movementRange = 2;
    private int attackRange = 1;

    private bool canMove = false;
    private bool onMove = false;

    private bool canAttack = false;
    private bool onAttack = false;

    private bool firstTurn = true;

    private Case currentCase;

    private float moveSpeed = 5f;

    private Coroutine moveCoroutine;

    public Team Team;


    public void Initialize(int Id, string MobName, int Attack, int Health, string MobDescription)
    {
        id = Id;
        mobName = MobName;
        attack = Attack;
        health = Health;
        mobDescription = MobDescription;
    }

    public void OnClickForMove()
    {
        if (BoardManager.Instance.IsInAction()) return;
        if (!canMove) return;
        BoardManager.Instance.SetSelectedMob(this);
        BoardManager.Instance.HighlightMovableCells(transform.position, movementRange);
        BoardManager.Instance.SetClickedThisFrame(true);
    }

    public void OnClickForAttack()
    {
        if (BoardManager.Instance.IsInAction()) return;
        if (!canAttack) return;
        BoardManager.Instance.SetSelectedMob(this);
        BoardManager.Instance.HighlightAttackableCells(transform.position, attackRange);
        BoardManager.Instance.SetClickedThisFrame(true);
    }

    public void MoveTo(Vector3 destination)
    {
        // Si un mouvement est déjà en cours, on l'arrête
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        // On lance le nouveau déplacement
        moveCoroutine = StartCoroutine(MoveSmoothly(destination));
    }

    private IEnumerator MoveSmoothly(Vector3 destination)
    {
        Vector3 start = transform.position;
        float distance = Vector3.Distance(start, destination);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        transform.LookAt(destination);

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, destination, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Finir pile à la position cible
        transform.position = destination;

        moveCoroutine = null; // Reset
    }

    public void AttackMob(IAttackable target)
    {
        if (target == null) return;
        if (target is Mob)
        {
            Mob targetMob = target as Mob;
            targetMob.SetHealth(targetMob.GetHealth() - attack);
            if (targetMob.GetHealth() <= 0)
            {
                // Le mob est mort, gérer la suppression
                if (targetMob.GetCurrentCase() != null)
                {
                    targetMob.GetCurrentCase().SetOccupied(false, null);
                    targetMob.RemoveFromList();
                }
            }
        }
        if (target is Base)
        {
            Base targetBase = target as Base;
            targetBase.SetHealth(targetBase.GetHealth() - attack);
            Debug.Log($"Base health is now: {targetBase.GetHealth()}");
            if (targetBase.GetHealth() <= 0)
            {
                // La base est détruite, gérer la fin de partie
                Debug.Log("Base destroyed! Game Over.");
            }
        }

    }

    public void RemoveFromList()
    {
        BoardManager.Instance.RemoveMobFromBoard(this.gameObject);
        BoardManager.Instance.RemoveMobFromRedTeam(this.gameObject);
        BoardManager.Instance.RemoveMobFromBlueTeam(this.gameObject);
        Destroy(this.gameObject);
    }   

    public int GetMovementRange()
    {
        return movementRange;
    }

    public int GetAttackRange()
    {
        return attackRange;
    }

    public void SetCurrentCase(Case newCase)
    {
        if (currentCase != null)
        {
            currentCase.SetOccupied(false, null);
        }
        currentCase = newCase;
        currentCase.SetOccupied(true, this);
        this.transform.SetParent(currentCase.transform);
    }

    public Case GetCurrentCase()
    {
        return currentCase;
    }

    public void SetCanMove(bool state)
    {
        canMove = state;
    }

    public bool CanMove()
    {
        return canMove;
    }

    public bool CanAttack()
    {
        return canAttack;
    }

    public void SetCanAttack(bool state)
    {
        canAttack = state;
    }

    public int GetHealth()
    {
        return health;
    }

    public void SetHealth(int value)
    {
        health = value;
    }

    public int GetAttack()
    {
        return attack;
    }

    public void SetAttack(int value)
    {
        attack = value;
    }

    public bool IsFirstTurn()
    {
        return firstTurn;
    }

    public void SetFirstTurn(bool state)
    {
        firstTurn = state;
    }

    public bool GetFirstTurn()
    {
        return firstTurn;
    }

    public bool IsOnMove()
    {
        return onMove;
    }

    public void SetOnMove(bool state)
    {
        onMove = state;
    }

    public bool IsOnAttack()
    {
        return onAttack;
    }

    public void SetOnAttack(bool state)
    {
        onAttack = state;
    }

    public void EndOfTurn()
    {
        canMove = false;
        canAttack = false;
        firstTurn = false;
    }

    public void StartOfTurn()
    {
        canMove = true;
        canAttack = true;
    }

    void Update()
        {
            if (healthText != null)
                healthText.text = health.ToString();
            if (attackText != null)
                attackText.text = attack.ToString();
        }
}


using UnityEngine;
using System.Collections;

public class Mob : MonoBehaviour
{
    private int id;
    private string mobName;
    private int attack;
    private int health;
    private string mobDescription;

    private int movementRange = 2;

    private bool canMove = false;

    private Case currentCase;

    private float moveSpeed = 5f;

    private Coroutine moveCoroutine;


    public void Initialize(int Id, string MobName, int Attack, int Health, string MobDescription)
    {
        id = Id;
        mobName = MobName;
        attack = Attack;
        health = Health;
        mobDescription = MobDescription;
    }

    public void OnClickFromChild()
    {
        if (BoardManager.Instance.IsInAction()) return;
        if(!canMove) return;
        BoardManager.Instance.SetSelectedMob(this);
        BoardManager.Instance.HighlightMovableCells(transform.position, movementRange);
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

    public int GetMovementRange()
    {
        return movementRange;
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
}

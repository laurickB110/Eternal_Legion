using UnityEngine;

public class Case : MonoBehaviour
{
    [SerializeField] GameObject hoveredTrue;
    [SerializeField] GameObject hoveredFalse;
    [SerializeField] GameObject highlightedAttack;
    private bool isOccupied = false;
    private Mob mob;

    public void Highlight(bool highlight)
    {
        if (highlight)
        {
            if (isOccupied)
            {
                hoveredTrue.SetActive(false);
                hoveredFalse.SetActive(true);
            }
            else
            {
                hoveredTrue.SetActive(true);
                hoveredFalse.SetActive(false);
            }
        }
        else
        {
            hoveredTrue.SetActive(false);
            hoveredFalse.SetActive(false);
        }
    }

    public void HighlightAsAttackable(bool highlight)
    {
        if (highlight)
        {
            highlightedAttack.SetActive(true);
        }
        else
        {
            highlightedAttack.SetActive(false);
        }
    }

    public void SetOccupied(bool state, Mob mob)
    {
        isOccupied = state;
        this.mob = mob;
    }

    public bool IsOccupied()
    {
        return isOccupied;
    }

    public Mob GetOccupyingMob()
    {
        return mob;
    }
}

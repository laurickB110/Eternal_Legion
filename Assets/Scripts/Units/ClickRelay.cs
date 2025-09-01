using UnityEngine;

public class ClickRelay : MonoBehaviour
{
    [SerializeField] Mob mob; // le script sur l'objet parent

    void OnMouseDown()
    {
        if(mob.GetFirstTurn()) // Si c'est le premier tour du mob
            return; // On ne fait rien
        if(!mob.CanMove() && !mob.CanAttack()) // Si on est en train de déplacer ou d'attaquer
            return; // On ne fait rien
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.5f);
        UIManager.Instance.ShowMobActionMenu(mob, screenPosition);
    }
}

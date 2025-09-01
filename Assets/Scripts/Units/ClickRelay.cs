using UnityEngine;

public class ClickRelay : MonoBehaviour
{
   [SerializeField] Mob mob; // le script sur l'objet parent

    void OnMouseDown()
    {
        mob.OnClickFromChild();
    }
}

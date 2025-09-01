using UnityEngine;

public class Base : MonoBehaviour, IAttackable
{
    private int health = 20;

    public int GetHealth(){
        return health;
    
    }

    public void SetHealth(int value){
        health = value;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Debug.Log("Base destroyed!");
            // Handle base destruction (e.g., end game)
        }
    }
}

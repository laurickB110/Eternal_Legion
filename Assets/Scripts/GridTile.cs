using UnityEngine;

public class GridTile : MonoBehaviour
{
    public Vector2Int gridPosition;

    private void OnMouseDown()
    {
        Debug.Log("Tu as cliqu� sur la case : " + gridPosition);
    }
}

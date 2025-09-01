using UnityEngine;

public class GridTile : MonoBehaviour
{
    public Vector2Int gridPosition;

    private void OnMouseDown()
    {
        Debug.Log("Tu as cliqué sur la case : " + gridPosition);
    }
}

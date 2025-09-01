using UnityEngine;

public class GridManager : MonoBehaviour
{
    // 1) Taille de la grille
    public int width = 5;
    public int height = 7;

    // 2) Référence au prefab de case
    public GameObject tilePrefab;

    // 3) Espace entre chaque case
    public float tileSpacing = 1.1f;

    // 4) Méthode appelée une seule fois au démarrage de la scène
    private void Start()
    {
        GenerateGrid();
    }

    // 5) Génère toutes les cases
    private void GenerateGrid()
    {
        // Boucle sur chaque colonne (x)
        for (int x = 0; x < width; x++)
        {
            // Boucle sur chaque ligne (y)
            for (int y = 0; y < height; y++)
            {
                // Calcul de la position 3D dans la scène
                Vector3 position = new Vector3(x * tileSpacing, 0, y * tileSpacing);

                // Instancie une copie du prefab à la position donnée, parenté sous ce GameObject
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);

                // Renomme l’objet dans la hiérarchie pour t’y retrouver
                tile.name = $"Tile {x},{y}";

                // Récupère ton script GridTile sur la case pour lui donner ses coordonnées
                GridTile gridTile = tile.GetComponent<GridTile>();
                gridTile.gridPosition = new Vector2Int(x, y);
            }
        }
    }
}

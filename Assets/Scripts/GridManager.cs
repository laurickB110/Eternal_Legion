using UnityEngine;

public class GridManager : MonoBehaviour
{
    // 1) Taille de la grille
    public int width = 5;
    public int height = 7;

    // 2) R�f�rence au prefab de case
    public GameObject tilePrefab;

    // 3) Espace entre chaque case
    public float tileSpacing = 1.1f;

    // 4) M�thode appel�e une seule fois au d�marrage de la sc�ne
    private void Start()
    {
        GenerateGrid();
    }

    // 5) G�n�re toutes les cases
    private void GenerateGrid()
    {
        // Boucle sur chaque colonne (x)
        for (int x = 0; x < width; x++)
        {
            // Boucle sur chaque ligne (y)
            for (int y = 0; y < height; y++)
            {
                // Calcul de la position 3D dans la sc�ne
                Vector3 position = new Vector3(x * tileSpacing, 0, y * tileSpacing);

                // Instancie une copie du prefab � la position donn�e, parent� sous ce GameObject
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);

                // Renomme l�objet dans la hi�rarchie pour t�y retrouver
                tile.name = $"Tile {x},{y}";

                // R�cup�re ton script GridTile sur la case pour lui donner ses coordonn�es
                GridTile gridTile = tile.GetComponent<GridTile>();
                gridTile.gridPosition = new Vector2Int(x, y);
            }
        }
    }
}

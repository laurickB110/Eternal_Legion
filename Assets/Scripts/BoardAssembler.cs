using UnityEngine;

public class BoardAssembler : MonoBehaviour
{
    public GameObject leftBoardPrefab;
    public GameObject rightBoardPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    // Instancier les demi-plateaux
         GameObject leftBoard = Instantiate(leftBoardPrefab, Vector3.zero, Quaternion.identity);
         GameObject rightBoard = Instantiate(rightBoardPrefab, Vector3.zero, Quaternion.identity);

         // Chercher les AnchorPoints
         Transform leftAnchor = leftBoard.transform.Find("AnchorPoint");
         Transform rightAnchor = rightBoard.transform.Find("AnchorPoint");

         if (leftAnchor == null || rightAnchor == null)
         {
             Debug.LogError("AnchorPoint manquant sur un prefab!");
             return;
         }

         // Calculer le décalage : amener l'anchor du rightBoard sur l'anchor du leftBoard
         Vector3 offset = leftAnchor.position - rightAnchor.position;

         // Déplacer le rightBoard pour qu'ils se rejoignent
         rightBoard.transform.position += offset;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDatabase : MonoBehaviour
{
    public static List<Card> cardList = new List<Card>();

    public static List<Card> vikingDeck = new List<Card>();
    public static List<Card> egyptianDeck = new List<Card>();

    void Awake()
    {
        // les éléments de cards dans l'ordre Card(id, CardName, cost,  attack, health, pm, description, Sprite image, nomduPrefab)
        // )
        cardList.Add(new Card(0, "Dieu Caillou", 2, 7, 9, "Apparition: inflige 1 point de dégats a tous les unités ennemies.", Resources.Load<Sprite>("Cards/Stone_Golem")));

        cardList.Add(new Card(1, "ZEUS", 1, 8, 8, "Apparition: foudroie un ennemi en lui infligeant 7 points de dégats.", Resources.Load<Sprite>("Cards/Zeus")));

        cardList.Add(new Card(2, "Anubis", 1, 5, 5, "Apparition: réincarne 3 cartes alliés présentes dans le cimetière.", Resources.Load<Sprite>("Cards/Anubis")));

        cardList.Add(new Card(3, "Troll", 4, 4, 6, "Un Troll bélliqueux tout ce qu'il y a de plus classique.", Resources.Load<Sprite>("Cards/Troll")));

        cardList.Add(new Card(4, "Frondeur", 2, 2, 2, "Portée: 3", Resources.Load<Sprite>("Cards/Frondeur")));
        

        vikingDeck.Add(new Card(0, "Epeiste débutant", 1, 2, 1, "Inflige 1 point de dégat à 2 unités ennemie autour d'elle", Resources.Load<Sprite>("Vikings/Epeiste")));
        vikingDeck.Add(new Card(1, "Epeiste débutant", 1, 2, 1, "Inflige 1 point de dégat à 2 unités ennemie autour d'elle", Resources.Load<Sprite>("Vikings/Epeiste")));
        vikingDeck.Add(new Card(2, "Epeiste débutant", 1, 2, 1, "Inflige 1 point de dégat à 2 unités ennemie autour d'elle", Resources.Load<Sprite>("Vikings/Epeiste")));
        vikingDeck.Add(new Card(3, "Archer débutant", 1, 1, 2, "Inflige 1 point de dégat à une unité ennemie quand elle apparait", Resources.Load<Sprite>("Vikings/Archer")));
        vikingDeck.Add(new Card(4, "Archer débutant", 1, 1, 2, "Inflige 1 point de dégat à une unité ennemie quand elle apparait", Resources.Load<Sprite>("Vikings/Archer")));
        vikingDeck.Add(new Card(5, "Archer débutant", 1, 1, 2, "Inflige 1 point de dégat à une unité ennemie quand elle apparait", Resources.Load<Sprite>("Vikings/Archer")));
        vikingDeck.Add(new Card(6, "Berserker", 3, 2, 3, "A chaque fois qu'il subit des dégats, il gagne +1 en attaque", Resources.Load<Sprite>("Vikings/Berserker")));
        vikingDeck.Add(new Card(7, "Berserker", 3, 2, 3, "A chaque fois qu'il subit des dégats, il gagne +1 en attaque", Resources.Load<Sprite>("Vikings/Berserker")));
        vikingDeck.Add(new Card(8, "Valkyrie", 2, 2, 2, "Donne +1 pv et +1 d'attaque à une unité", Resources.Load<Sprite>("Vikings/Valkyrie")));
        vikingDeck.Add(new Card(9, "Valkyrie", 2, 2, 2, "Donne +1 pv et +1 d'attaque à une unité", Resources.Load<Sprite>("Vikings/Valkyrie")));
        vikingDeck.Add(new Card(10, "Bouclier de Jormun", 3, 1, 4, "Provocation: oblige les ennemies alentour à l'attaquer", Resources.Load<Sprite>("Vikings/Bouclier")));
        vikingDeck.Add(new Card(11, "Bouclier de Jormun", 3, 1, 4, "Provocation: oblige les ennemies alentour à l'attaquer", Resources.Load<Sprite>("Vikings/Bouclier")));
        vikingDeck.Add(new Card(12, "Porte-Hache d'Helheim", 4, 5, 4, "Si cette unité tue une autre unité, gagne +2 pv", Resources.Load<Sprite>("Vikings/Berserker")));
        vikingDeck.Add(new Card(13, "Porte-Hache d'Helheim", 4, 5, 4, "Si cette unité tue une autre unité, gagne +2 pv", Resources.Load<Sprite>("Vikings/Berserker")));
        vikingDeck.Add(new Card(14, "Champion du Valhalla", 5, 6, 6, "Quand il apparait, inflige 2 points de dégats dans une croix de 3 cases", Resources.Load<Sprite>("Vikings/Champion")));
        vikingDeck.Add(new Card(15, "Champion du Valhalla", 5, 6, 6, "Quand il apparait, inflige 2 points de dégats dans une croix de 3 cases", Resources.Load<Sprite>("Vikings/Champion")));
        vikingDeck.Add(new Card(16, "L'Elu des Dieux", 6, 6, 7, "Attaque deux fois par tour, ne subit pas de contre lors de sa deuxième attaque", Resources.Load<Sprite>("Vikings/Elu")));
        vikingDeck.Add(new Card(17, "Thor", 7, 7, 7, "Quand il apparait, foudroie une unité ennemie en lui infligeant 5 points de dégats", Resources.Load<Sprite>("Vikings/Thor")));
        vikingDeck.Add(new Card(18, "Odin", 7, 5, 8, "A la fin de votre tour, donne +1 pv et +1 d'attaque à toutes les unités alliés", Resources.Load<Sprite>("Vikings/Odin")));
        vikingDeck.Add(new Card(19, "Briseur de porte", 4, 4, 6, "ne peut attaquer que la base ennemie", Resources.Load<Sprite>("Vikings/Bouclier")));

    }
}

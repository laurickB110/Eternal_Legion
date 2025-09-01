using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] GameObject actionMenu;
    [SerializeField] Button moveButton;
    [SerializeField] Button attackButton;

    private Mob selectedMob;

    private bool justOpenedMenu = false;

    void Awake()
    {
        // Sécurité : éviter les doublons de singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        actionMenu.SetActive(false); // Cacher au démarrage
    }

    public void ShowMobActionMenu(Mob mob, Vector3 screenPosition)
    {
        selectedMob = mob;
        actionMenu.SetActive(true);
        actionMenu.transform.position = screenPosition;

        moveButton.onClick.RemoveAllListeners();
        attackButton.onClick.RemoveAllListeners();

        moveButton.onClick.AddListener(() =>
        {
            selectedMob.SetOnMove(true);
            selectedMob.OnClickForMove();
            HideMenu();
        });

        attackButton.onClick.AddListener(() =>
        {
            selectedMob.SetOnAttack(true);
            selectedMob.OnClickForAttack();
            HideMenu();
        });
        justOpenedMenu = true;
    }

    public void HideMenu()
    {
        actionMenu.SetActive(false);
        selectedMob = null;
    }

    public void SetMobSelected(Mob mob)
    {
        selectedMob = mob;
    }

    void LateUpdate()
    {
        if (justOpenedMenu)
        {
            // ⏱ Attendre une frame complète avant d’autoriser la fermeture
            justOpenedMenu = false;
            return;
        }
        // Si on clique n'importe où ET que ce n’est pas sur de l’UI
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HideMenu();
        }
    }
}

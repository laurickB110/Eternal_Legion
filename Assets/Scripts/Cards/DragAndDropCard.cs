using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class DragAndDropCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Camera worldCamera; // Caméra principale 3D
    [SerializeField] GameObject cardVisual; // visuel de la carte
    private DisplayCard dataCard;

    private GameObject lastCaseTouched;
    public GameObject mob;

    private GameObject instanceMob;

    private Vector3 originalPosition;
    private Vector3 originalScale;

    public GameObject Hand;
    public GameObject HandCard;

    private bool canDrag = true;

    private CardHoverEffect hoverEffect;


    private void Awake()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        hoverEffect = GetComponent<CardHoverEffect>();
        dataCard = cardVisual.GetComponent<DisplayCard>();

    }

    void Start()
    {
        originalScale = transform.localScale;

        Hand = GameObject.Find("Hand");
        HandCard.transform.SetParent(Hand.transform);
        HandCard.transform.localScale = originalScale;
        HandCard.transform.position = new Vector3(transform.position.x, transform.position.y, -48);
        HandCard.transform.eulerAngles = new Vector3(25, 0, 0);
    }

    void Update()
{
    if (dataCard != null)
    {
        bool canPlay = TurnSystem.Instance.isYourTurn &&
                       TurnSystem.Instance.GetMana() >= dataCard.GetCost();

        dataCard.SetHighlight(canPlay);
    }
}

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Logic for beginning the drag
        if (!canDrag || TurnSystem.Instance.GetMana() < dataCard.GetCost()) return;
        originalPosition = this.transform.position;

        if (hoverEffect != null)
            hoverEffect.OnDragStart();
        
        BoardManager.Instance.DisableCollidersMobs();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canDrag || TurnSystem.Instance.GetMana() < dataCard.GetCost()) return;

        this.transform.position = eventData.position; // Move the card with the mouse

        // canvasGroup.blocksRaycasts = false;


        Ray ray = worldCamera.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject caseTouched = hit.collider.gameObject;
            if (caseTouched.name.StartsWith("Case"))
            {
                if (lastCaseTouched != null)
                {
                    lastCaseTouched.GetComponent<Case>().Highlight(false);
                }
                lastCaseTouched = caseTouched;
                lastCaseTouched.GetComponent<Case>().Highlight(true);
                cardVisual.SetActive(false);
                if (instanceMob == null)
                {
                    instanceMob = Instantiate(mob, mob.transform.position, Quaternion.identity);
                    instanceMob.GetComponent<Mob>().Initialize(dataCard.GetId(), dataCard.GetName(), dataCard.GetPower(), dataCard.GetHealth(), dataCard.GetDescription());
                }
                else
                {
                    instanceMob.transform.position = lastCaseTouched.transform.position + new Vector3(0, 1, 0);
                }
            }
            else
            {
                lastCaseTouched.GetComponent<Case>().Highlight(false);
            }
        }
        else
        {
            if (lastCaseTouched != null)
            {
                lastCaseTouched.GetComponent<Case>().Highlight(false); 
            }
            cardVisual.SetActive(true);
            if (instanceMob != null)
            {
                Destroy(instanceMob);
            }
        }

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Logic for ending the drag
        if (!canDrag || TurnSystem.Instance.GetMana() < dataCard.GetCost()) return;

        Ray ray = worldCamera.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (lastCaseTouched != null)
            {
                if (lastCaseTouched.GetComponent<Case>().IsOccupied())
                {
                    this.transform.position = originalPosition; // Reset position if not dropped on a valid target
                    this.transform.localScale = originalScale;
                    cardVisual.SetActive(true);
                    Destroy(instanceMob);
                }
                else
                {
                    TurnSystem.Instance.UseMana(dataCard.GetCost());
                    HandManager.Instance.RemoveCard(this.gameObject);
                    lastCaseTouched.GetComponent<Case>().SetOccupied(true, instanceMob.GetComponent<Mob>());
                    BoardManager.Instance.AddMobToBoard(instanceMob);
                    BoardManager.Instance.AddMobToBlueTeam(instanceMob);
                    instanceMob.GetComponent<Mob>().SetCurrentCase(lastCaseTouched.GetComponent<Case>());
                }
            }
        }
        else
        {
            this.transform.position = originalPosition; // Reset position if not dropped on a valid target
            this.transform.localScale = originalScale;
        }

        if (hoverEffect != null)
            hoverEffect.OnDragEnd();

        if (lastCaseTouched != null)
        {
            lastCaseTouched.GetComponent<Case>().Highlight(false);
        }
            
        BoardManager.Instance.EnableCollidersMobs();

    }
    
    public void SetDraggable(bool state)
    {
        canDrag = state;
    }
}

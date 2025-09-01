using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [Header("Durée du tour en secondes")]
    private float turnDuration = 30f;
    
    private float timer;
    private bool isCounting = false;

    [Header("UI")]
    [SerializeField] private Image radialImage;

    [Header("Couleurs")]
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color endColor = Color.red;

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (!isCounting) return;

        timer -= Time.deltaTime;
        timer = Mathf.Clamp(timer, 0f, turnDuration);

        UpdateVisual();

        if (timer <= 0f)
        {
            isCounting = false;
            EndTurnDueToTimeout();
        }
    }

    public void StartTurnTimer()
    {
        timer = turnDuration;
        isCounting = true;
        UpdateVisual();
    }

    public void StopTurnTimer()
    {
        isCounting = false;
    }

    private void UpdateVisual()
    {
        float t = timer / turnDuration;

        if (radialImage != null)
        {
            radialImage.fillAmount = t;
            radialImage.color = Color.Lerp(endColor, startColor, t);
        }
    }

    private void EndTurnDueToTimeout()
    {
        Debug.Log("Temps écoulé, fin du tour !");
        TurnSystem.Instance.EndTurn();
    }
}

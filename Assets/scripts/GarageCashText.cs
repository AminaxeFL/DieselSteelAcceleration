using TMPro;
using UnityEngine;

public class GarageCashText : MonoBehaviour
{
    [SerializeField] private TMP_Text cashGarageText;
    [SerializeField] private string cashPrefix = "CASH: ";
    [SerializeField] private bool useTestCash;
    [SerializeField] private float testCash = 1000f;

    private int lastShown = int.MinValue;

    private void Awake()
    {
        if (!cashGarageText)
            cashGarageText = GetComponent<TMP_Text>();

        if (useTestCash)
            Money.SetSavedBalance(testCash);
    }

    [ContextMenu("Give Test Cash")]
    public void GiveTestCash()
    {
        Money.SetSavedBalance(testCash);
        Refresh();
    }

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        int shown = Mathf.RoundToInt(Money.SavedBalance);
        if (shown == lastShown)
            return;

        lastShown = shown;
        cashGarageText.text = cashPrefix + shown.ToString("N0");
    }
}

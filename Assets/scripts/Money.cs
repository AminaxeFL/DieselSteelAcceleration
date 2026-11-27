using TMPro;
using UnityEngine;

public class Money : MonoBehaviour
{
    public static Money Instance { get; private set; }
    public static float SavedBalance { get; private set; }

    private static bool hasSavedBalance;

    public float startMoney = 0f;

    [SerializeField] private TMP_Text cashLabel;
    [SerializeField] private string cashPrefix = "CASH: ";

    private int lastShown = int.MinValue;

    public float Balance { get; private set; }

    private void Awake()
    {
        Instance = this;

        if (!hasSavedBalance)
        {
            SavedBalance = startMoney;
            hasSavedBalance = true;
        }

        Balance = SavedBalance;
        RefreshLabel();
    }

    public void AddMoney(float amount)
    {
        Balance += amount;
        SavedBalance = Balance;
        RefreshLabel();
    }

    public bool RemoveMoney(float amount)
    {
        if (Balance < amount)
            return false;

        Balance -= amount;
        SavedBalance = Balance;
        RefreshLabel();
        return true;
    }

    public void Bankrot()
    {
        Balance = 0f;
        SavedBalance = 0f;
        RefreshLabel();
    }

    public static void SetSavedBalance(float amount)
    {
        SavedBalance = amount;
        hasSavedBalance = true;
    }

    private void RefreshLabel()
    {
        int shown = Mathf.RoundToInt(Balance);

        if (shown == lastShown)
            return;

        lastShown = shown;
        cashLabel.text = cashPrefix + shown.ToString("N0");
    }
}

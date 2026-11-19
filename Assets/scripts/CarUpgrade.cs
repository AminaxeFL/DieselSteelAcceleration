using UnityEngine;

public class CarUpgrade : MonoBehaviour
{
    [SerializeField] private CarDamage carDamage;
    [SerializeField] private WheelSpinner wheelSpinner;
    [SerializeField] private WheelJoint2D wheels;

    [SerializeField] private float upgradeEnginePerLevel;
    [SerializeField] private float upgradeFuelTankPerLevel;
    [SerializeField] private float upgradeCarBodyPerLevel;
    [SerializeField] private float upgradePodveskaPerLevel;

    public int engineLevel = 1;
    public int fuelTankLevel = 1;
    public int carBodyLevel = 1;
    public int podveskaLevel = 1;

    private float upgradedEngine;
    private JointSuspension2D upgradeSuspension;

    private void Awake()
    {
        carDamage = GetComponent<CarDamage>();
        wheelSpinner = GetComponent<WheelSpinner>();
        wheels = GetComponentInChildren<WheelJoint2D>();
    }

    private void Start()
    {
        upgradeSuspension = wheels.suspension;
        upgradedEngine = wheelSpinner.maxSpeed + (engineLevel - 1) * upgradeEnginePerLevel;
    }

    private void Update()
    {
        if (upgradedEngine > 0f)
            wheelSpinner.maxSpeed = upgradedEngine;
    }

    public void UpgradeEngine()
    {
        if (engineLevel < 5)
        {
            engineLevel++;
            upgradedEngine += upgradeEnginePerLevel;
            wheelSpinner.maxSpeed = upgradedEngine;
        }
        else
        {
            Debug.Log("Max Level");
        }
    }

    public void UpgradeFuelTank()
    {
        if (fuelTankLevel < 5)
        {
            fuelTankLevel++;
            wheelSpinner.maxFuel += upgradeFuelTankPerLevel * fuelTankLevel;
        }
        else
        {
            Debug.Log("Max Level");
        }
    }

    public void UpgradeCarBody()
    {
        if (carBodyLevel < 5)
        {
            carBodyLevel++;
            carDamage.maxHp += upgradeCarBodyPerLevel * carBodyLevel;
        }
        else
        {
            Debug.Log("Max Level");
        }
    }

    public void UpgradePodveska()
    {
        if (podveskaLevel < 5)
        {
            podveskaLevel++;
            upgradeSuspension.frequency += upgradePodveskaPerLevel * podveskaLevel;
            wheels.suspension = upgradeSuspension;
        }
        else
        {
            Debug.Log("Max Level");
        }
    }
}

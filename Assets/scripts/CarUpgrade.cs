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

    [SerializeField] private int maxLevel;
    public int engineLevel = 1;
    public int fuelTankLevel = 1;
    public int carBodyLevel = 1;
    public int podveskaLevel = 1;

    private float upgradedEngine;
    private float upgradedCarBody;
    private float upgradedFuelTank;
    private JointSuspension2D upgradedPodveska;

    private void Awake()
    {
        carDamage = GetComponent<CarDamage>();
        wheelSpinner = GetComponent<WheelSpinner>();
        wheels = GetComponentInChildren<WheelJoint2D>();
    }

    private void Start()
    {
        upgradedEngine = wheelSpinner.maxSpeed;
        upgradedFuelTank = wheelSpinner.maxFuel;
        upgradedCarBody = carDamage.maxHp;
        upgradedPodveska = wheels.suspension;
    }

    private void Upgrade()
    {
        wheels.suspension = upgradedPodveska;
        wheelSpinner.maxSpeed = upgradedEngine;
        wheelSpinner.maxFuel = upgradedFuelTank;
        carDamage.maxHp = upgradedCarBody;
    }

    public void UpgradeEngine()
    {
        if (engineLevel < maxLevel)
        {
            engineLevel++;
            upgradedEngine += upgradeEnginePerLevel * engineLevel;
            Upgrade();
            Debug.Log(upgradedEngine);
        }
        else
        {
            Debug.Log("Max Level");
        }
    }

    public void UpgradeFuelTank()
    {
        if (fuelTankLevel < maxLevel)
        {
            fuelTankLevel++;
            upgradedFuelTank = upgradeFuelTankPerLevel * fuelTankLevel;
            Upgrade();
            Debug.Log(upgradedFuelTank);
        }
        else
        {
            Debug.Log("Max Level");
        }
    }

    public void UpgradeCarBody()
    {
        if (carBodyLevel < maxLevel)
        {
            carBodyLevel++;
            upgradedCarBody = upgradeCarBodyPerLevel * carBodyLevel;
            Upgrade();
            Debug.Log(upgradedCarBody);
        }
        else
        {
            Debug.Log("Max Level");
        }
    }

    public void UpgradePodveska()
    {
        if (podveskaLevel < maxLevel)
        {
            podveskaLevel++;
            upgradedPodveska.frequency = upgradePodveskaPerLevel * podveskaLevel;
            Upgrade();
            Debug.Log(upgradedPodveska.frequency);
        }
        else
        {
            Debug.Log("Max Level");
        }
    }
}

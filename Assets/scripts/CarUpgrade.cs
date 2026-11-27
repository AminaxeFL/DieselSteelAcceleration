using System.Collections;
using UnityEngine;

public class CarUpgrade : MonoBehaviour
{
    [SerializeField] private float upgradeEnginePerLevel = 50f;
    [SerializeField] private float upgradeFuelTankPerLevel = 20f;
    [SerializeField] private float upgradeCarBodyPerLevel = 200f;
    [SerializeField] private float upgradePodveskaPerLevel = 1f;
    [SerializeField] private float upgradeBounceForce = 1200f;
    [SerializeField] private float upgradeBounceTorque = 60f;
    [SerializeField] private float upgradeBounceDuration = 0.25f;
    [SerializeField] private float wheelStaggerDelay = 0.08f;

    [SerializeField] private int maxLevel = 5;
    [SerializeField] private Sprite[] upgradeLineSprites = new Sprite[4];
    [SerializeField] private SpriteRenderer engineLine;
    [SerializeField] private SpriteRenderer fuelLine;
    [SerializeField] private SpriteRenderer lifeLine;
    [SerializeField] private SpriteRenderer springLine;
    [SerializeField] private SpriteButton engineButton;
    [SerializeField] private SpriteButton fuelButton;
    [SerializeField] private SpriteButton lifeButton;
    [SerializeField] private SpriteButton springButton;

    public int engineLevel = 1;
    public int fuelTankLevel = 1;
    public int carBodyLevel = 1;
    public int podveskaLevel = 1;

    private CarDamage carDamage;
    private WheelSpinner wheelSpinner;
    private WheelJoint2D[] wheels;

    private float upgradedEngine;
    private float upgradedCarBody;
    private float upgradedFuelTank;
    private JointSuspension2D upgradedPodveska;

    private void Start()
    {
        GameObject car = GameObject.FindGameObjectWithTag("Target");
        carDamage = car.GetComponent<CarDamage>();
        wheelSpinner = car.GetComponent<WheelSpinner>();
        wheels = car.GetComponentsInChildren<WheelJoint2D>(true);

        upgradedEngine = wheelSpinner.maxSpeed;
        upgradedFuelTank = wheelSpinner.maxFuel;
        upgradedCarBody = carDamage.maxHp;
        upgradedPodveska = wheels[0].suspension;

        UpdateLines();
    }

    private void ApplyUpgrades()
    {
        for (int i = 0; i < wheels.Length; i++)
            wheels[i].suspension = upgradedPodveska;

        wheelSpinner.maxSpeed = upgradedEngine;
        wheelSpinner.maxFuel = upgradedFuelTank;
        wheelSpinner.fuel = upgradedFuelTank;
        carDamage.maxHp = upgradedCarBody;
        carDamage.hp = upgradedCarBody;

        UpdateLines();
        BounceCar();
    }

    private void BounceCar()
    {
        StopAllCoroutines();
        StartCoroutine(SuspensionPush());
    }

    private IEnumerator SuspensionPush()
    {
        Rigidbody2D chassis = wheelSpinner.Chassis;
        float perWheelForce = upgradeBounceForce / wheels.Length;
        float torque = Random.Range(-upgradeBounceTorque, upgradeBounceTorque);
        int firstWheel = Random.Range(0, wheels.Length);
        float totalDuration = upgradeBounceDuration + wheelStaggerDelay;
        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                float delay = i == firstWheel ? 0f : wheelStaggerDelay;
                float localElapsed = elapsed - delay;

                if (localElapsed < 0f || localElapsed >= upgradeBounceDuration)
                    continue;

                float curve = Mathf.Sin((localElapsed / upgradeBounceDuration) * Mathf.PI);
                Rigidbody2D wheelBody = wheels[i].connectedBody;
                wheelBody.AddForce(Vector2.down * (perWheelForce * curve), ForceMode2D.Force);
                chassis.AddForceAtPosition(Vector2.up * (perWheelForce * curve), wheelBody.position, ForceMode2D.Force);
            }

            if (elapsed < upgradeBounceDuration)
            {
                float torqueCurve = Mathf.Sin((elapsed / upgradeBounceDuration) * Mathf.PI);
                chassis.AddTorque(torque * torqueCurve, ForceMode2D.Force);
            }

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    private void UpdateLines()
    {
        UpdateLine(engineLine, engineLevel);
        UpdateLine(fuelLine, fuelTankLevel);
        UpdateLine(lifeLine, carBodyLevel);
        UpdateLine(springLine, podveskaLevel);

        engineButton.interactable = engineLevel < maxLevel;
        fuelButton.interactable = fuelTankLevel < maxLevel;
        lifeButton.interactable = carBodyLevel < maxLevel;
        springButton.interactable = podveskaLevel < maxLevel;

        engineLine.GetComponent<SpriteButton>().interactable = engineLevel < maxLevel;
        fuelLine.GetComponent<SpriteButton>().interactable = fuelTankLevel < maxLevel;
        lifeLine.GetComponent<SpriteButton>().interactable = carBodyLevel < maxLevel;
        springLine.GetComponent<SpriteButton>().interactable = podveskaLevel < maxLevel;
    }

    private void UpdateLine(SpriteRenderer line, int level)
    {
        int upgradeCount = level - 1;
        if (upgradeCount <= 0)
        {
            line.enabled = false;
            return;
        }

        line.enabled = true;
        line.sprite = upgradeLineSprites[Mathf.Clamp(upgradeCount - 1, 0, upgradeLineSprites.Length - 1)];
    }

    public void UpgradeEngine()
    {
        if (engineLevel >= maxLevel)
            return;

        engineLevel++;
        upgradedEngine += upgradeEnginePerLevel;
        ApplyUpgrades();
    }

    public void UpgradeFuelTank()
    {
        if (fuelTankLevel >= maxLevel)
            return;

        fuelTankLevel++;
        upgradedFuelTank += upgradeFuelTankPerLevel;
        ApplyUpgrades();
    }

    public void UpgradeCarBody()
    {
        if (carBodyLevel >= maxLevel)
            return;

        carBodyLevel++;
        upgradedCarBody += upgradeCarBodyPerLevel;
        ApplyUpgrades();
    }

    public void UpgradePodveska()
    {
        if (podveskaLevel >= maxLevel)
            return;

        podveskaLevel++;
        upgradedPodveska.frequency += upgradePodveskaPerLevel;
        ApplyUpgrades();
    }
}

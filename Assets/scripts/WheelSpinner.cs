using UnityEngine;

public class WheelSpinner : MonoBehaviour
{
    public bool canGo = true;
    public float maxSpeed = 800f;
    public float accelerationTime = 3f;
    public float airStopTime = 3f;

    public LayerMask groundMask = -1;
    public float groundCheckDistance = 0.2f;

    public float fuel = 100f;
    public float maxFuel = 100f;
    public float fuelTime = 1f;

    private Rigidbody2D body;
    private WheelJoint2D[] joints;
    private float wheelRadius = 0.35f;
    private float targetMotorSpeed;
    private float currentMotorSpeed;
    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];
    private float fuelDrain;

    public float Speed
    {
        get
        {
            float sum = 0f;
            int count = 0;

            for (int i = 0; i < joints.Length; i++)
            {
                Rigidbody2D wheel = joints[i].connectedBody;
                sum += wheel.angularVelocity;
                count++;
            }

            return count == 0 ? 0f : -sum / count * wheelRadius;
        }
    }

    public float Fuel01 => maxFuel <= 0f ? 0f : Mathf.Clamp01(fuel / maxFuel);

    public Rigidbody2D Chassis => body;

    private void Awake()
    {
        joints = GetComponentsInChildren<WheelJoint2D>();
        body = GetComponent<Rigidbody2D>();

        for (int i = 0; i < joints.Length; i++)
            SetMotor(joints[i], 0f, 0f);

        MeasureWheelRadius();
        targetMotorSpeed = 0f;
        currentMotorSpeed = 0f;
    }

    private void MeasureWheelRadius()
    {
        float sum = 0f;
        int count = 0;

        for (int i = 0; i < joints.Length; i++)
        {
            Rigidbody2D wheel = joints[i].connectedBody;
            float radius = WheelRadius(wheel);
            if (radius <= 0f)
                continue;

            sum += radius;
            count++;
        }

        if (count > 0)
            wheelRadius = sum / count;
    }

    private static void SetMotor(WheelJoint2D joint, float motorSpeed, float maxMotorTorque)
    {
        joint.motor = new JointMotor2D
        {
            motorSpeed = motorSpeed,
            maxMotorTorque = maxMotorTorque
        };
    }

    private float MaxMotorSpeed()
    {
        return -maxSpeed / Mathf.Max(wheelRadius, 0.05f);
    }

    private int CountGrounded()
    {
        int count = 0;
        for (int j = 0; j < joints.Length; j++)
        {
            Rigidbody2D wheel = joints[j].connectedBody;
            float radius = WheelRadius(wheel);
            Vector2 origin = wheel.position + Vector2.down * radius * 0.8f;
            float distance = groundCheckDistance + radius * 0.2f;
            int hitCount = Physics2D.RaycastNonAlloc(origin, Vector2.down, groundHits, distance, groundMask);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = groundHits[i].collider;
                Rigidbody2D hitBody = hit.attachedRigidbody;

                if (hitBody == wheel || hitBody == body)
                    continue;

                if (hit.transform.IsChildOf(transform))
                    continue;

                count++;
                break;
            }
        }

        return count;
    }

    private void FixedUpdate()
    {
        int grounded = CountGrounded();
        bool airStop = grounded < 2;
        bool hasFuel = fuel > 0f;

        if (canGo && maxSpeed > 0f && !airStop && hasFuel)
        {
            targetMotorSpeed = MaxMotorSpeed();
        }
        else
        {
            targetMotorSpeed = 0f;
        }

        float accelTime = accelerationTime;
        if (Mathf.Approximately(targetMotorSpeed, 0f) && airStop && grounded < 2)
        {
            accelTime = accelerationTime * 3f;
            if (accelTime < 0.01f)
                accelTime = 0.01f;
        }

        float accel = accelTime > 0f ? Mathf.Abs(MaxMotorSpeed() - currentMotorSpeed) / accelTime : float.PositiveInfinity;
        if (Mathf.Approximately(targetMotorSpeed, 0f))
        {
            accel = accelTime > 0f ? Mathf.Abs(currentMotorSpeed) / accelTime : float.PositiveInfinity;
        }

        float delta = accel * Time.fixedDeltaTime;
        currentMotorSpeed = Mathf.MoveTowards(currentMotorSpeed, targetMotorSpeed, delta);

        for (int i = 0; i < joints.Length; i++)
            SetMotor(joints[i], currentMotorSpeed, float.PositiveInfinity);

        if (hasFuel && canGo && !Mathf.Approximately(targetMotorSpeed, 0f) && maxSpeed > 0f)
        {
            float speedFactor = Mathf.Clamp01(Mathf.Abs(currentMotorSpeed) / Mathf.Abs(MaxMotorSpeed()) * 1.5f);
            if (speedFactor > 1.5f)
                speedFactor = 1.5f;

            if (fuelTime <= 0f)
                fuelDrain = 0f;
            else
                fuelDrain = 1f / fuelTime * speedFactor;

            fuel -= fuelDrain * Time.fixedDeltaTime;
            if (fuel < 0f)
                fuel = 0f;
        }
    }

    private static float WheelRadius(Rigidbody2D wheel)
    {
        CircleCollider2D circle = wheel.GetComponent<CircleCollider2D>();
        return circle.radius * Mathf.Abs(wheel.transform.lossyScale.x);
    }
}

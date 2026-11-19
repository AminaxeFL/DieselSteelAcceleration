using System.Collections.Generic;
using UnityEngine;

public class CarNoCollide : MonoBehaviour
{
    [SerializeField] private bool rescanEveryFrame = false;
    [SerializeField] private float rescanInterval = 0.25f;

    private readonly List<WheelSpinner> cars = new List<WheelSpinner>();
    private readonly List<Collider2D[]> carColliders = new List<Collider2D[]>();
    private float rescanTimer;

    private void Start()
    {
        Rescan();
    }

    private void FixedUpdate()
    {
        if (rescanEveryFrame)
        {
            Rescan();
            return;
        }

        rescanTimer -= Time.fixedDeltaTime;
        if (rescanTimer <= 0f)
            Rescan();
    }

    private void Rescan()
    {
        rescanTimer = Mathf.Max(rescanInterval, 0.05f);

        cars.Clear();
        carColliders.Clear();

        WheelSpinner[] found = FindObjectsByType<WheelSpinner>();
        for (int i = 0; i < found.Length; i++)
        {
            cars.Add(found[i]);
            carColliders.Add(CollidersOf(found[i]));
        }

        for (int i = 0; i < cars.Count; i++)
        {
            Collider2D[] left = carColliders[i];
            for (int j = i + 1; j < cars.Count; j++)
            {
                Collider2D[] right = carColliders[j];
                IgnorePair(left, right);
            }
        }
    }

    private static void IgnorePair(Collider2D[] left, Collider2D[] right)
    {
        for (int a = 0; a < left.Length; a++)
        {
            Collider2D leftCollider = left[a];
            for (int b = 0; b < right.Length; b++)
            {
                Collider2D rightCollider = right[b];
                if (rightCollider != leftCollider)
                    Physics2D.IgnoreCollision(leftCollider, rightCollider, true);
            }
        }
    }

    private static Collider2D[] CollidersOf(WheelSpinner car)
    {
        List<Collider2D> list = new List<Collider2D>();
        Rigidbody2D chassis = car.GetComponent<Rigidbody2D>();
        list.AddRange(chassis.GetComponents<Collider2D>());

        foreach (WheelJoint2D joint in car.GetComponentsInChildren<WheelJoint2D>(true))
        {
            list.AddRange(joint.GetComponents<Collider2D>());
        }

        return list.ToArray();
    }
}

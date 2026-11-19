using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [SerializeField] private TerrainGenerator terrain;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private float cellSize = 0.1f;
    [SerializeField] private float density = 0.004f;
    [SerializeField] private float viewRadius = 22f;
    [SerializeField] private float heightAbove = 0.3f;
    [SerializeField] private Vector2Int groupCountRange = new Vector2Int(1, 1);
    [SerializeField] private float groupWidth = 1.5f;
    [SerializeField] private float arcHeight = 0.35f;
    [SerializeField] private int seed = 12345;
    [SerializeField] private int sortingOrder = 0;

    private readonly Dictionary<int, List<GameObject>> cells = new Dictionary<int, List<GameObject>>();
    private readonly List<int> staleList = new List<int>();
    private Transform follow;

    private void Awake()
    {
        WheelSpinner car = FindAnyObjectByType<WheelSpinner>();
        follow = car.transform;
    }

    private void Update()
    {
        float center = follow.position.x;
        float step = Mathf.Max(cellSize, 0.1f);

        int first = Mathf.FloorToInt((center - viewRadius) / step);
        int last = Mathf.FloorToInt((center + viewRadius) / step);

        for (int i = first; i <= last; i++)
        {
            if (!cells.ContainsKey(i))
                SpawnCell(i, step);
        }

        if (cells.Count > 0)
        {
            staleList.Clear();

            foreach (KeyValuePair<int, List<GameObject>> pair in cells)
            {
                if (pair.Key < first - 1 || pair.Key > last + 1)
                    staleList.Add(pair.Key);
            }

            for (int i = 0; i < staleList.Count; i++)
                ClearCell(staleList[i]);
        }
    }

    private void SpawnCell(int cell, float step)
    {
        List<GameObject> coins = new List<GameObject>();

        if (Hash(cell, 0) < density)
        {
            float cx = (cell + Hash(cell, 1)) * step;
            int count = Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(groupCountRange.x, groupCountRange.y, Hash(cell, 2))));
            float peak = heightAbove + Mathf.Lerp(-arcHeight, arcHeight, Hash(cell, 3));

            for (int i = 0; i < count; i++)
            {
                float t = count <= 1 ? 0.5f : (float)i / (count - 1);
                float x = cx + (t - 0.5f) * groupWidth;
                float y = terrain.HeightAt(x) + peak + Mathf.Sin(t * Mathf.PI) * arcHeight;

                GameObject coin = Instantiate(
                    coinPrefab,
                    new Vector3(x, y, 0f),
                    Quaternion.identity,
                    transform
                );

                SpriteRenderer renderer = coin.GetComponent<SpriteRenderer>();
                renderer.sortingOrder = sortingOrder + Mathf.RoundToInt(Hash(cell, 9 + i) * 2f);

                coins.Add(coin);
            }
        }

        cells[cell] = coins;
    }

    private void ClearCell(int cell)
    {
        if (!cells.TryGetValue(cell, out List<GameObject> coins))
            return;

        for (int i = 0; i < coins.Count; i++)
            Destroy(coins[i]);

        cells.Remove(cell);
    }

    private float Hash(int cell, int salt)
    {
        unchecked
        {
            int n = cell * 374761393 + (seed + salt * 6151) * 668265263;
            n = (n ^ (n >> 13)) * 1274126177;
            return (n & 0x7fffffff) / (float)0x7fffffff;
        }
    }
}

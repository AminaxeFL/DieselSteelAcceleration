using System.Collections.Generic;
using UnityEngine;

public class BushSpawner : MonoBehaviour
{
    [SerializeField] private TerrainGenerator terrain;
    [SerializeField] private GameObject bushPrefab;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float density = 0.55f;
    [SerializeField] private float viewRadius = 22f;
    [SerializeField] private float yOffset = 0f;
    [SerializeField] private int seed = 12345;
    [SerializeField] private Vector2 scaleRange = new Vector2(0.7f, 1.5f);
    [SerializeField] private Vector2 heightRange = new Vector2(0.7f, 1f);
    [SerializeField] private Vector2 radiusRange = new Vector2(0.8f, 0.7f);
    [SerializeField] private Vector2 lumpsRange = new Vector2(0.1f, 0.45f);
    [SerializeField] private Vector2 lumpFreqRange = new Vector2(1.5f, 4f);
    [SerializeField] private int sortingOrder = -10000;

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
        List<GameObject> bushes = new List<GameObject>(1);

        if (Hash(cell, 0) < density)
        {
            float x = (cell + Hash(cell, 1)) * step;
            float y = terrain.HeightAt(x);

            GameObject bush = Instantiate(
                bushPrefab,
                new Vector3(x, y + yOffset, 0f),
                Quaternion.identity,
                transform
            );

            float scale = Mathf.Lerp(scaleRange.x, scaleRange.y, Hash(cell, 2));
            bush.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer renderer = bush.GetComponent<SpriteRenderer>();

            if (Hash(cell, 3) < 0.5f)
                renderer.flipX = !renderer.flipX;

            renderer.sortingOrder = sortingOrder + Mathf.RoundToInt(Hash(cell, 9) * 2f);

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetFloat("_BushHeight", Lerp(heightRange, cell, 4));
            block.SetFloat("_BushRadius", Lerp(radiusRange, cell, 5));
            block.SetFloat("_BushLumps", Lerp(lumpsRange, cell, 6));
            block.SetFloat("_BushLumpFreq", Lerp(lumpFreqRange, cell, 7));
            block.SetFloat("_Seed", Mathf.Floor(Hash(cell, 8) * 9973f));
            renderer.SetPropertyBlock(block);

            bushes.Add(bush);
        }

        cells[cell] = bushes;
    }

    private void ClearCell(int cell)
    {
        if (!cells.TryGetValue(cell, out List<GameObject> bushes))
            return;

        for (int i = 0; i < bushes.Count; i++)
            Destroy(bushes[i]);

        cells.Remove(cell);
    }

    private float Lerp(Vector2 range, int cell, int salt)
    {
        return Mathf.Lerp(range.x, range.y, Hash(cell, salt));
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

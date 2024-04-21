using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPool : MonoBehaviour
{
    public static BallPool Instance;

    [SerializeField]
    private Ball prefab;

    [SerializeField]
    private int amount;
    
    private List<Ball> spawnedPrefabs = new();


    private void Awake()
    {
        Instance = this;
        InstantiatePrefabs(amount);
    }


    private void InstantiatePrefabs(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var go = InstantiateBall();
            spawnedPrefabs.Add(go);

        }
    }


    public Ball GetBall()
    {
        foreach (var e in spawnedPrefabs)
        {
            if (e.gameObject.activeInHierarchy)
                continue;

            return e;
        }

        var go = InstantiateBall();
        spawnedPrefabs.Add(go);
        return go;
    }

    private Ball InstantiateBall()
    {
        var go = Instantiate(prefab, transform);
        go.gameObject.SetActive(false);
        return go;
    }
}

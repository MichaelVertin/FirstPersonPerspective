using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;

public class SpawnArea : MonoBehaviour
{
    private List<SpawnPoint> spawnPoints;
    [SerializeField] private float spawnDelay = 10f;
    [SerializeField] private int startingZombies = 10;
    [SerializeField] private int maxSpawns = 10;
    private List<Zombie> zombies = new List<Zombie>();
    [SerializeField] Zombie zombiePrefab;

    public List<Zombie> Zombies
    {
        get
        {
            zombies.RemoveAll(z => z == null);
            return zombies;
        }
    }

    private void Awake()
    {
        spawnPoints = new List<SpawnPoint>(GetComponentsInChildren<SpawnPoint>());
    }

    private void Start()
    {
        for (int i = 0; i < startingZombies; i++)
        {
            SpawnZombie();
        }
        StartCoroutine(SpawnUnits());
    }

    private IEnumerator SpawnUnits()
    {
        yield return new WaitForEndOfFrame();
        while (true)
        {
            yield return new WaitForSeconds(spawnDelay);
            if(Zombies.Count < maxSpawns)
            {
                SpawnZombie();
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        Zombie zombie = other.GetComponent<Zombie>();
        if(zombie != null)
        {
            RegisterZombie(zombie);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        Zombie zombie = other.GetComponent<Zombie>();
        if (zombie != null)
        {
            zombies.Remove(zombie);
        }
    }

    public Zombie SpawnZombie()
    {
        SpawnPoint point = spawnPoints[Random.Range(0, spawnPoints.Count)];
        Zombie zombie = point.Spawn(zombiePrefab);
        RegisterZombie(zombie);
        return zombie;
    }

    public void RegisterZombie(Zombie zombie)
    {
        if(!zombies.Contains(zombie))
        {
            zombies.Add(zombie);
        }
    }
}

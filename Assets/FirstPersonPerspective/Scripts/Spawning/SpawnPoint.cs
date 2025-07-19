using UnityEngine;
using UnityEngine.AI;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] Transform initialZombieTransform;

    public BoxCollider boxCollider = null;
    public void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    public Zombie Spawn(Zombie zombiePrefab)
    {
        // create new zombie
        Zombie zombie = Instantiate<Zombie>(zombiePrefab);

        // set it to a random point in the collider
        Vector3 randomNavMeshPosition = GetRandomNavMeshPoint(); //  GetRandomPointInCollider();
        zombie.SetPosition(initialZombieTransform);
        zombie.SetTarget(randomNavMeshPosition);
        zombie.transform.parent = this.transform.parent;
        return zombie;
    }

    public Vector3 GetRandomNavMeshPoint()
    {
        float SAMPLE_DISTANCE = 5f; // max distance from randomly generated box point
        int MAX_ATTEMPTS = 100; // retry on fail

        for (int i = 0; i < MAX_ATTEMPTS; i++)
        {
            Vector3 randomPoint = GetRandomPointInCollider();
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, SAMPLE_DISTANCE, NavMesh.AllAreas))
            {
                // Only add if position is valid
                if (boxCollider.bounds.Contains(hit.position))
                {
                    return hit.position;
                }
            }
        }

        Debug.LogWarning("Failed to find valid NavMesh point within BoxCollider after " + MAX_ATTEMPTS + " attempts.");
        return Vector3.zero;
    }

    // returns a random point in 
    private Vector3 GetRandomPointInCollider()
    {
        Vector3 center = boxCollider.bounds.center;
        Vector3 extents = boxCollider.bounds.extents;

        // random x, y, and z
        float randomX = Random.Range(center.x - (extents.x), center.x + (extents.x));
        float randomY = Random.Range(center.y - (extents.y), center.y + (extents.y));
        float randomZ = Random.Range(center.z - (extents.z), center.z + (extents.z));

        return new Vector3(randomX, randomY, randomZ);
    }



}

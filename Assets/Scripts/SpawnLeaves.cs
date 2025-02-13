using UnityEngine;

public class SpawnLeaves : MonoBehaviour
{
    public GameObject LeafPrefab;
    public float SpawnTime;
    public float SpawnInterval;
    public float DestroyTimer;

    // Spawns the Prefab Leaf
    void Start()
    {
        InvokeRepeating("Spawn", SpawnTime, SpawnInterval);
    }

    // Spawns a Prefab LeafPrefab and destroys it after set Destroy Timer
    private void Spawn()
    {
        GameObject leaf = Instantiate(LeafPrefab, transform.position, Quaternion.identity);
        Destroy(leaf, DestroyTimer);
    }
}

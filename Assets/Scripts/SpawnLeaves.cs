using UnityEngine;

public class SpawnLeaves : MonoBehaviour
{
    public GameObject LeafPrefab;
    public float SpawnTime;
    public float SpawnInterval;
    public float DestroyTimer;

    void Start()
    {
        InvokeRepeating("Spawn", SpawnTime, SpawnInterval);
    }

    private void Spawn()
    {
        GameObject leaf = Instantiate(LeafPrefab, transform.position, Quaternion.identity);

        Destroy(leaf, DestroyTimer);
    }
}

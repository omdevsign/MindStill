using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Chunk Settings")]
    [SerializeField] List<GameObject> chunkPrefabs; 
    [SerializeField] int startingChunksAmount = 12;
    [SerializeField] Transform chunkParent;
    [SerializeField] float chunkLength = 10f;
    [SerializeField] float moveSpeed = 8f;
    
    [Header("Obstacle Settings")]
    [SerializeField] GameObject obstaclePrefab; 
    [SerializeField] float obstacleChance = 0.5f; 

    List<GameObject> chunks = new List<GameObject>();

    void Start()
    {
        SpawnStartingChunks();
    }
    void FixedUpdate() 
    {
        if (Time.timeScale == 0f) return;
        MoveChunks();
    }
    void SpawnStartingChunks()
    {
        for (int i = 0; i < startingChunksAmount; i++)
        {
            SpawnChunk();
        }
    }
    private void SpawnChunk()
    {
        int randomIndex = Random.Range(0, chunkPrefabs.Count);
        GameObject chunkPrefabToSpawn = chunkPrefabs[randomIndex];
        float spawnPositionZ = CalculateSpawnPositionZ();
        Vector3 chunkSpawnPos = new Vector3(transform.position.x, transform.position.y, spawnPositionZ);
        GameObject newChunk = Instantiate(chunkPrefabToSpawn, chunkSpawnPos, Quaternion.identity, chunkParent);
        chunks.Add(newChunk);
        if (randomIndex == 2)
        {
            float[] lanes = { -1.5f, 0f, 1.5f };   
            float obstacleX = lanes[Random.Range(0, lanes.Length)]; 
            float obstacleY = 0.5f; 
            float obstacleZ = newChunk.transform.position.z + chunkLength * 0.5f; 
            Vector3 obstaclePos = new Vector3(obstacleX, obstacleY, obstacleZ); 
            Instantiate(obstaclePrefab, obstaclePos, Quaternion.identity, newChunk.transform);
        }
    }
    float CalculateSpawnPositionZ()
    {
        float spawnPositionZ;
        if (chunks.Count == 0)
        {
            spawnPositionZ = transform.position.z;
        }
        else
        {
            spawnPositionZ = chunks[chunks.Count - 1].transform.position.z + chunkLength;
        }
        return spawnPositionZ;
    }
    void MoveChunks() 
    {
        float moveDistance = moveSpeed * Time.fixedDeltaTime; 
        for (int i = 0; i < chunks.Count; i++)
        {
            GameObject chunk = chunks[i];
            chunk.transform.Translate(-transform.forward * moveDistance); 
            if (chunk.transform.position.z < -chunkLength)
            {
                chunks.Remove(chunk);
                Destroy(chunk);
                SpawnChunk();
            }
        }
    }
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyGroup
{
    public GameObject enemyPrefab;
    public int count = 1;
    public int spawnPointIndex = 0;
    public float delayBetweenSpawns = 0.5f; // Time between each enemy in this group
}

[System.Serializable]
public class EnemySet
{
    public string setName = "Set";
    public List<EnemyGroup> enemyGroups = new List<EnemyGroup>();
    public float delayAfterSet = 2f; // Only used if spawning sets sequentially
}

[System.Serializable]
public class Wave
{
    public string waveName = "Wave";
    public List<EnemySet> enemySets = new List<EnemySet>();
    public bool spawnSetsSimultaneously = false; // Toggle: spawn all sets at once or one at a time
    public float delayBeforeNextWave = 5f;
}

[System.Serializable]
public class SpawnPoint
{
    public string spawnPointName = "Spawn Point";
    public Transform spawnTransform;
    public Transform[] waypoints;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    [Header("Wave Configuration")]
    [SerializeField] private List<Wave> waves = new List<Wave>();

    [Header("Settings")]
    [SerializeField] private bool autoStartWave = true;
    [SerializeField] private bool loopWaves = false;

    private int currentWaveIndex = -1;
    private int enemiesAliveThisWave = 0;
    private bool isSpawning = false;

    private void Start()
    {
        if (autoStartWave && waves.Count > 0)
        {
            StartNextWave();
        }
    }

    public void StartNextWave()
    {
        if (isSpawning) return;

        currentWaveIndex++;

        if (currentWaveIndex >= waves.Count)
        {
            if (loopWaves)
            {
                currentWaveIndex = 0;
                Debug.Log("All waves complete! Restarting from wave 1...");
            }
            else
            {
                Debug.Log("All waves complete!");
                return;
            }
        }

        Wave currentWave = waves[currentWaveIndex];

        // Calculate total enemies in wave
        int totalEnemies = 0;
        foreach (EnemySet set in currentWave.enemySets)
        {
            foreach (EnemyGroup group in set.enemyGroups)
            {
                totalEnemies += group.count;
            }
        }

        enemiesAliveThisWave = totalEnemies;

        string spawnMode = currentWave.spawnSetsSimultaneously ? "simultaneously" : "sequentially";
        Debug.Log($"Starting {currentWave.waveName} ({currentWaveIndex + 1}/{waves.Count}) with {totalEnemies} enemies - Sets spawning {spawnMode}");

        StartCoroutine(SpawnWave(currentWave));
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        isSpawning = true;

        if (wave.spawnSetsSimultaneously)
        {
            // Spawn all sets at the same time
            List<Coroutine> setCoroutines = new List<Coroutine>();

            foreach (EnemySet set in wave.enemySets)
            {
                StartCoroutine(SpawnSet(set));
            }

            // Wait for all sets to finish spawning
            // (We'll wait for enemies to be defeated in WaitForWaveComplete)
        }
        else
        {
            // Spawn sets one after another
            foreach (EnemySet set in wave.enemySets)
            {
                yield return StartCoroutine(SpawnSet(set));
                yield return new WaitForSeconds(set.delayAfterSet);
            }
        }

        isSpawning = false;
        StartCoroutine(WaitForWaveComplete(wave));
    }

    private IEnumerator SpawnSet(EnemySet set)
    {
        // Start all groups in this set simultaneously
        foreach (EnemyGroup group in set.enemyGroups)
        {
            StartCoroutine(SpawnGroup(group));
        }

        // Calculate longest spawn time in this set to know when it's done
        float longestSpawnTime = 0f;
        foreach (EnemyGroup group in set.enemyGroups)
        {
            float groupSpawnTime = (group.count - 1) * group.delayBetweenSpawns;
            if (groupSpawnTime > longestSpawnTime)
            {
                longestSpawnTime = groupSpawnTime;
            }
        }

        yield return new WaitForSeconds(longestSpawnTime);
    }

    private IEnumerator SpawnGroup(EnemyGroup group)
    {
        // Validate spawn point
        if (group.spawnPointIndex < 0 || group.spawnPointIndex >= spawnPoints.Count)
        {
            Debug.LogWarning($"Invalid spawn point index: {group.spawnPointIndex}. Skipping group.");
            enemiesAliveThisWave -= group.count;
            yield break;
        }

        SpawnPoint spawnPoint = spawnPoints[group.spawnPointIndex];

        // Spawn each enemy in the group
        for (int i = 0; i < group.count; i++)
        {
            if (group.enemyPrefab != null && spawnPoint.spawnTransform != null)
            {
                SpawnEnemy(group.enemyPrefab, spawnPoint);
            }
            else
            {
                Debug.LogWarning("Enemy prefab or spawn point is null. Skipping enemy.");
                enemiesAliveThisWave--;
            }

            // Wait between spawns (except after the last one)
            if (i < group.count - 1)
            {
                yield return new WaitForSeconds(group.delayBetweenSpawns);
            }
        }
    }

    private IEnumerator WaitForWaveComplete(Wave wave)
    {
        // Wait for all enemies to be defeated or reach the end
        while (enemiesAliveThisWave > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log($"{wave.waveName} Complete!");

        yield return new WaitForSeconds(wave.delayBeforeNextWave);

        if (autoStartWave)
        {
            StartNextWave();
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab, SpawnPoint spawnPoint)
    {
        GameObject enemy = Instantiate(
            enemyPrefab,
            spawnPoint.spawnTransform.position,
            spawnPoint.spawnTransform.rotation
        );

        // Set waypoints for enemy movement
        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        if (movement != null && spawnPoint.waypoints != null && spawnPoint.waypoints.Length > 0)
        {
            movement.SetWaypoints(spawnPoint.waypoints);
        }

        // Subscribe to death event
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.OnDeath += OnEnemyDeath;
        }
    }

    private void OnEnemyDeath()
    {
        enemiesAliveThisWave--;
    }

    public void OnEnemyReachedEnd()
    {
        enemiesAliveThisWave--;
    }

    // Public methods for UI/debugging
    public int GetCurrentWaveNumber() => currentWaveIndex + 1;
    public int GetTotalWaves() => waves.Count;
    public int GetEnemiesAlive() => enemiesAliveThisWave;
    public bool IsSpawning() => isSpawning;
    public string GetCurrentWaveName() => currentWaveIndex >= 0 && currentWaveIndex < waves.Count ? waves[currentWaveIndex].waveName : "None";
}
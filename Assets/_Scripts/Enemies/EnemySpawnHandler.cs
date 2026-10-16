using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnHandler : MonoBehaviour
{
    [Header("Debug Spawn")]
    [SerializeField] string spawnThisAtStart;
    [SerializeField] float startDelay;

    [Header("Spawn Points")]
    [SerializeField] Transform doorSpawnPoint;
    [SerializeField] Transform windowSpawnPoint;
    private Vector3 doorSpawnPosition;
    private Vector3 windowSpawnPosition;

    [Header("References")]
    [SerializeField] ParticleManager particleController;
    private Transform player;

    [Header("Prefabs")]
    [SerializeField] List<GameObject> enemiesPrefabs;

    private IEnumerator Start()
    {
        GameManager.instance.RegisterEnemySpawnHandler(this);
        player = Camera.main.transform;
        doorSpawnPosition = doorSpawnPoint.position;
        windowSpawnPosition = windowSpawnPoint.position;

        yield return new WaitForSeconds(startDelay);
        if (spawnThisAtStart != null) StartCoroutine(SpawnEnemi());        
    }

    private IEnumerator SpawnEnemi()
    {
        yield return new WaitForSeconds(2f);
        SpawnEnemy(1, doorSpawnPosition);
    }

    public void SpawnEnemies(string enemiesToSpawn) => SpawnEnemies(enemiesToSpawn, 0);
    public IEnumerator SpawnEnemies(string enemiesToSpawn, float timeBetween)
    {
        for (int i = 0; i < enemiesToSpawn.Length; i++)
        {
            SpawnEnemy(int.Parse(enemiesToSpawn[i].ToString()), doorSpawnPosition);
            yield return new WaitForSeconds(timeBetween);
        }
    }
    public void SpawnEnemy(int enemyToSpawn, Vector3 startPosition)
    {
        Mathf.Clamp(enemyToSpawn, 1, 4);
        GameObject newEnemy = Instantiate(enemiesPrefabs[enemyToSpawn-1], startPosition, Quaternion.identity);
        UpdateEnemyReferences(newEnemy, enemyToSpawn);
    }

    private void UpdateEnemyReferences(GameObject newEnemy, int enemyToSpawn)
    {
        var newEnemyScript = newEnemy.GetComponent<Enemy>();
        newEnemyScript.particleManager = particleController;        
        newEnemy.GetComponent<LookAtCamera>().target = player;

        GameManager.instance.AddNewEnemy(newEnemy, enemyToSpawn);
    }

}

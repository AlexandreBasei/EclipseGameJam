using System.Collections.Generic;
using UnityEngine;

public class SpawnersManager : MonoBehaviour
{
    [SerializeField] private int numberToDisable = 5;

    [SerializeField] private int maxSamePrefab = 2;

    private Dictionary<GameObject, int> prefabUsage = new Dictionary<GameObject, int>();

    private void Start()
    {
        DisableRandomSpawners();

        GenerateObjects();
    }

    private void DisableRandomSpawners()
    {
        List<GameObject> spawners = new List<GameObject>();

        foreach (Transform child in transform)
            spawners.Add(child.gameObject);
    

        int amountToDisable = Mathf.Clamp(numberToDisable, 0,spawners.Count);

        for (int i = 0; i < amountToDisable; i++)
        {
            int randomIndex = Random.Range(0, spawners.Count);
            spawners[randomIndex].SetActive(false);
            spawners.RemoveAt(randomIndex);
        }

    }

    private void GenerateObjects()
    {

        ItemSpawner[] activeSpawners = GetComponentsInChildren<ItemSpawner>();

        List<ItemSpawner> shuffledSpawners = new List<ItemSpawner>(activeSpawners);

        Shuffle(shuffledSpawners);

        foreach (ItemSpawner spawner in shuffledSpawners)
            SpawnRandomObject(spawner);
    }

    private void SpawnRandomObject(ItemSpawner spawner)
    {
        List<GameObject> availablePrefabs = new List<GameObject>();

        foreach (GameObject prefab in spawner.PossiblePrefabs)
        {
            if (prefab == null)
                continue;

            if (!prefabUsage.ContainsKey(prefab))
                availablePrefabs.Add(prefab);
            
            else if (prefabUsage[prefab] < maxSamePrefab)
                availablePrefabs.Add(prefab);
            
        }

        if (availablePrefabs.Count == 0)
            return;
        
        GameObject selectedPrefab = availablePrefabs[Random.Range(0, availablePrefabs.Count)];

        spawner.Spawn(selectedPrefab);

        if (!prefabUsage.ContainsKey(selectedPrefab))
            prefabUsage[selectedPrefab] = 0;
        
        prefabUsage[selectedPrefab]++;

    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
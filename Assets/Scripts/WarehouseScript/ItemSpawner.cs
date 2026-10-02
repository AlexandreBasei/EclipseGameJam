using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> possiblePrefabs = new List<GameObject>();

    public List<GameObject> PossiblePrefabs => possiblePrefabs;

    public void Spawn(GameObject prefab)
    {
        if (prefab == null)
            return;

        Instantiate(prefab, transform.position, transform.rotation);
    }
}
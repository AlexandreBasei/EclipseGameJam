using UnityEngine;
using System.Collections.Generic;

public class WorkShopManager : MonoBehaviour
{
    [SerializeField] private Transform[] truckSpawnPoints = new Transform[10];
    [SerializeField] private Transform[] chestSpawnPoints = new Transform[4];
    void Start()
    {
        GameObject[] truckItems = DaysManager.Instance.truckContent;
        GameObject[] chestItems = DaysManager.Instance.chestContent;
        
        for (int i = 0; i < truckItems.Length && i < truckSpawnPoints.Length; i++)
        {
            if (truckItems[i] == null || truckSpawnPoints[i] == null)
                continue;

            Instantiate(truckItems[i], truckSpawnPoints[i].position, truckSpawnPoints[i].rotation);
        }

        for (int i = 0; i < chestItems.Length && i < chestSpawnPoints.Length; i++)
        {
            if (chestItems[i] == null || chestSpawnPoints[i] == null)
                continue;

            Instantiate(chestItems[i], chestSpawnPoints[i].position, chestSpawnPoints[i].rotation);
        }

        DaysManager.Instance.chestContent = new GameObject[DaysManager.Instance.maxChestLevel];
    }
}

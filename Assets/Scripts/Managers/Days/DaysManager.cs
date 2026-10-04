using NUnit.Framework;
using UnityEngine;

public class DaysManager : PersistentSingleton<DaysManager>
{
    public int currentDay = 1;

    public GameObject[] chestContent;

    public bool isInWorkshop = false;

    public int truckLevel = 0;
    public int maxTruckLevel = 4;
    public int chestLevel = 0;
    public int maxChestLevel = 4;
    public int itemsInChest = 0;
    [HideInInspector] public int truckSize;
    public int defaultTruckSize = 6;

    public GameObject[] truckContent;

    [SerializeField] private string workshopSceneName = "WorkShopAlex";
    [SerializeField] private string warehouseSceneName = "WAREHOUSEScene";
    private int truckContentIndex = 0;

    protected override void Awake()
    {
        base.Awake();
        chestContent = new GameObject[4];
        truckSize = defaultTruckSize;
        setTruckSize();
    }

    public void NextDay()
    {
        currentDay++;
        itemsInChest = 0;
        truckContentIndex = 0;
        setTruckSize();
        loadWarehouseScene();
    }

    public void AddToTruck(Item item)
    {
        if (truckContentIndex < truckSize)
        {
            truckContent[truckContentIndex] = item.itemData.prefab;
            truckContentIndex++;
        }
        // print("Added to item to truck : " + item.GetComponent<Item>().itemData.name + ", " + truckContentIndex + "items in truck");
    }

    public void AddToChest(Item item)
    {
        if (itemsInChest <= chestLevel && chestLevel != 0)
        {
            chestContent[itemsInChest] = item.itemData.prefab;
            itemsInChest++;
        }
    }

    public void RemoveFromChest(Item item)
    {
        GameObject prefab = item.itemData.prefab;

        for (int i = 0; i < chestContent.Length; i++)
        {
            if (chestContent[i] != prefab)
            {
                continue;
            }

            for (int j = i; j < chestContent.Length - 1; j++)
            {
                chestContent[j] = chestContent[j + 1];
            }

            chestContent[chestContent.Length - 1] = null;
            itemsInChest = Mathf.Max(0, itemsInChest - 1);
            return;
        }
    }

    public void setTruckSize()
    {
        truckContent = new GameObject[truckSize];
    }

    public void UpgradeChest()
    {
        if (chestLevel == maxChestLevel)
            return;

        chestLevel++;
    }

    public void UpgradeTruck()
    {
        if (truckLevel == maxTruckLevel)
            return;

        truckLevel++;
        truckSize += 1;
        setTruckSize();
    }

    public void ResetDays()
    {
        currentDay = 1;
        chestContent = new GameObject[4];
        chestLevel = 0;
        truckLevel = 0;
        truckSize = defaultTruckSize;
        setTruckSize();
    }

    public void loadWorkShopScene()
    {
        isInWorkshop = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene(workshopSceneName);
    }

    public void loadWarehouseScene()
    {
        isInWorkshop = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(warehouseSceneName);
    }
}

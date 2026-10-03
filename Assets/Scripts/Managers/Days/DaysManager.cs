using NUnit.Framework;
using UnityEngine;

public class DaysManager : MonoBehaviour
{

    public static DaysManager Instance;

    public int currentDay = 1;

    public Item[] items;

    public bool isInWorkshop;

    public int truckLevel = 1;

    public Item[] truckContent;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        items = new Item[10];
    }

    public void NextDay(Item[] keptItems, int truckSize)
    {
        currentDay++;
        items = keptItems;
        truckContent = new Item[truckSize];
    }

    public void setTruckSize(int newSize)
    {
        truckContent = new Item[newSize];
    }

    public void ResetDays()
    {
        currentDay = 1;
        items = new Item[10];
        truckLevel = 1;
    }
}

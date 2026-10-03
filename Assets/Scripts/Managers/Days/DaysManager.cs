using NUnit.Framework;
using UnityEngine;

public class DaysManager : PersistentSingleton<DaysManager>
{
    public int currentDay = 1;

    public Item[] items;

    public bool isInWorkshop = true;

    public int truckLevel = 1;

    public Item[] truckContent;


    protected override void Awake()
    {
        base.Awake();
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

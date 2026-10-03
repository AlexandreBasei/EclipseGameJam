using NUnit.Framework;
using UnityEngine;

public class DaysManager : MonoBehaviour
{

    public static DaysManager Instance;

    public int currentDay = 1;

    public Item[] items;

    public bool IsInWorkshop;


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

    public void NextDay(Item[] keptItems)
    {
        currentDay++;
        items = keptItems;

    }

    public void ResetDays()
    {
        currentDay = 1;
        items = new Item[10];
    }
}

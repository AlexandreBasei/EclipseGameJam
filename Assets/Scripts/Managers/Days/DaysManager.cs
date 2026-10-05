using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DaysManager : PersistentSingleton<DaysManager>
{
    public int currentDay = 1;

    public GameObject[] chestContent;

    public bool isInWorkshop = false;
    public bool hasVisitedWareHouse = false;

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

    public bool tutoFinished = false;

    public bool tutoStarted = false;

    public int tutoProgress = 0;

    public TextMeshProUGUI tutoText;

    protected override void Awake()
    {
        base.Awake();
        chestContent = new GameObject[maxChestLevel];
        truckSize = defaultTruckSize;
        setTruckSize();
    }

    public void NextDay()
    {
        if(tutoFinished == false)
        {
            tutoFinished = true;
        }
        currentDay++;
        itemsInChest = 0;
        truckContentIndex = 0;
        hasVisitedWareHouse = false;
        CommandManager.Instance.DiscardAllCurrentsCommands();
        setTruckSize();
        loadWorkShopScene();
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
        if(tutoFinished == false && tutoProgress == 7)
        {
            tutoFirstBills();
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
        tutoFinished = false;
        tutoStarted = false;
        tutoProgress = 0;
    }

    public void loadWorkShopScene()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.MusicMainMenuAndStore);
        isInWorkshop = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene(workshopSceneName);
    }

    public void loadWarehouseScene()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.MusicWarehouse);
        isInWorkshop = false;
        hasVisitedWareHouse = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene(warehouseSceneName);
    }

    public void findTuto()
    {
        GameObject tutoObject = GameObject.FindGameObjectWithTag("Tuto");
        tutoObject.SetActive(true);
        tutoText = tutoObject.GetComponent<TextMeshProUGUI>();
        if (tutoFinished == false)
        {
            tutoText.text = "Find and collect the materials needed to fulfill orders within the time limit. Be careful, you can't bring everything back to the store. Once the trunk is full you can take it to leave.";
            tutoProgress = 4;
        }
    }


    public void tutorialStart()
    {
        print(tutoStarted);
        if (tutoStarted == false)
        {
            if (tutoFinished)
            {
                return;
            }
            tutoStarted = true;
            GameObject tutoObject = GameObject.FindGameObjectWithTag("Tuto");
            tutoObject.SetActive(true);
            tutoText = tutoObject.GetComponent<TextMeshProUGUI>();
            tutoText.text = "Check your computer for new commands and select your first command. ";
            tutoProgress = 1;
        }
        else
        {
            GameObject tutoObject = GameObject.FindGameObjectWithTag("Tuto");
            tutoObject.SetActive(true);
            tutoText = tutoObject.GetComponent<TextMeshProUGUI>();
            tutoText.text = "Welcome back home.\nTake one object and bring it close to another to merge them and thus fulfill the command.\nYou can merge a maximum of 3 objects.";
            tutoProgress = 5;
        }
    }

    public void tutoFirstTruck()
    {
        tutoText.text = "Take your truck to the warehouse.";
        tutoProgress = 3;
    }

    public void tutoFirstSale()
    {
        tutoText.text = "The more your item matches the criteria, the more money you'll get. \nOnce you are satisfied with the item you created, place it in the sales area and confirm the sale on the tablet.";
        tutoProgress = 6;
    }

    public void tutoFirstReview()
    {
        tutoText.text = "You can review the commands you have selected by pressing TAB.";
        tutoProgress = 2;
    }

    public void tutoFirstChestUse()
    {
        tutoText.text = "If you didn't use some of the items today, you can store some of them in the chest, to have them available the next day.";
        tutoProgress = 7;
    }

    public void tutoFirstBills()
    {
        tutoText.text = "Your money is used for two things:\n- paying your daily rent\n- buying upgrades for the trunk or for the storage box\nGo to your computer to purchase an upgrade";
        tutoProgress = 8;
    }

    public void tutoFirstSleep()
    {
        tutoText.text = "At the start of each day, the rent will be automatically debited from your account. \nDo your best to avoid going bankrupt within the next 4 days. \n\nGood Luck !";
        tutoProgress = 9;
        tutoStarted = false;
        tutoFinished = true;
    }
}

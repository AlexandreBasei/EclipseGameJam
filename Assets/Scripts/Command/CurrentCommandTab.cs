using System;
using UnityEngine;

public class CurrentCommandTab : MonoBehaviour
{
    [SerializeField] private GameObject commandUi;

    private void Awake()
    {
        commandUi.SetActive(false);
    }

    public void HideTab()
    {
        if (commandUi.activeSelf)
            commandUi.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if(Time.timeScale == 0) return;
            
            commandUi.SetActive(!commandUi.activeInHierarchy);
            if (DaysManager.Instance.tutoStarted && DaysManager.Instance.tutoProgress == 2 && commandUi.activeInHierarchy)
            {
                DaysManager.Instance.tutoFirstTruck();
            }
        }       
    }
}

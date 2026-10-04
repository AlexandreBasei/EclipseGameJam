using UnityEngine;

public class CurrentCommandTab : MonoBehaviour
{
    [SerializeField] private GameObject commandUi;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            commandUi.SetActive(!commandUi.activeInHierarchy);
            if (DaysManager.Instance.tutoStarted && DaysManager.Instance.tutoProgress == 2 && commandUi.activeInHierarchy)
            {
                DaysManager.Instance.tutoFirstTruck();
            }
        }       
    }
}

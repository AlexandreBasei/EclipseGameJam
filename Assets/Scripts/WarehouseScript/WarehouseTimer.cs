using UnityEngine;

public class WarehouseTimer : MonoBehaviour
{
    public float targetTime = 60.0f;

    private void Update()
    {

        targetTime -= Time.deltaTime;

        if (targetTime <= 0.0f)
        {
            timerEnded();
        }

    }

    void timerEnded()
    {
        //Return to the truck
    }
}

using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : Singleton<PlayerHUD>
{
    public enum InputTip
    {   
        PickUp = 0,
        Drop = 1,
        Rotate = 2,
        Snap = 3,
        Detach = 4,
        StartCar = 5,
        AddChest = 6,
        RemoveChest = 7,
        GoToSleep = 8,
        GoToWareHouse = 9,
        Computer = 10,
    }

    [SerializeField] private List<GameObject> _inputTips = new List<GameObject>();

    public PauseManager pauseManager;
    public int moneyValue = 1000;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseManager.PauseOpen();
        }
    }
    public void ShowInputTip(InputTip tip)
    {
        SetInputTipVisible(tip, true);
    }

    public void HideInputTip(InputTip tip)
    {
        SetInputTipVisible(tip, false);
    }

    public void SetInputTipVisible(InputTip tip, bool visible)
    {
        int index = (int)tip;
        if (index < 0 || index >= _inputTips.Count)
        {
            Debug.LogError($"Input tip '{tip}' has no corresponding entry in the PlayerHUD list.", this);
            return;
        }

        GameObject inputTip = _inputTips[index];
        if (inputTip == null)
        {
            Debug.LogError($"The TextMeshPro reference for input tip '{tip}' is not assigned.", this);
            return;
        }

        if (inputTip.activeSelf != visible)
        {
            inputTip.SetActive(visible);
        }
    }

    public void HideAllInputTips()
    {
        for (int i = 0; i < _inputTips.Count; i++)
        {
            if (_inputTips[i] == null)
            {
                Debug.LogError($"The TextMeshPro reference at PlayerHUD input tip index {i} is not assigned.", this);
                continue;
            }

            _inputTips[i].gameObject.SetActive(false);
        }
    }

    public void ChangedMoneyValue(int newValue)
    {
        moneyValue = newValue;
    }
}

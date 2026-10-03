using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : Singleton<PlayerHUD>
{
    public enum InputTip
    {
        Grab = 0,
        PickUp = 1,
    }

    [SerializeField] private List<GameObject> _inputTips = new List<GameObject>();

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
}

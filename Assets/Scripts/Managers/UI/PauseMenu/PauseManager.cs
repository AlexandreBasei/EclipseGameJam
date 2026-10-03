using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public void PauseOpen()
    {
        gameObject.SetActive(true);
        PlayerController.Instance.CanLook = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        PlayerController.Instance.CanLook = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Quit()
    {
        Application.Quit();
    }
}

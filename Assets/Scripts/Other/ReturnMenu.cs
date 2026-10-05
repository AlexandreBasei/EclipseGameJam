using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnMenu : MonoBehaviour
{
    public void OnClickMenu()
    {
        AudioManager.Instance.PlayClic();
        EndingScript.Instance.Reset();
        DaysManager.Instance.ResetDays();
        AudioManager.Instance.PlayClic();
        SceneManager.LoadScene("MainMenu");
    }
}

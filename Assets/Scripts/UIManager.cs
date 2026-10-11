using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button ReplayButton;

    public void ShowReplay()
    {
        ReplayButton.gameObject.SetActive(true);
    }

    public void HideReplay()
    {
        ReplayButton.gameObject.SetActive(false);
    }
}

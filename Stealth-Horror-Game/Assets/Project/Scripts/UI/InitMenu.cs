using UnityEngine;

public class InitMenu : MonoBehaviour
{
    [SerializeField] private MainMenu mainMenu;

    private void Awake()
    {
        if (mainMenu == null) mainMenu = transform.parent.transform.parent.GetComponentInChildren<MainMenu>();
    }

    public void OnAnimationEnds()
    {
        mainMenu?.ShowOrHideInitMenu(false);
    }
}

using UnityEngine;
using UnityEngine.UI;
using echo17.EndlessBook.Demo02;

public class BookArrowControls : MonoBehaviour
{
    [SerializeField] private Demo02 demo02;
    [SerializeField] private Button leftArrow;
    [SerializeField] private Button rightArrow;

    void OnEnable()
    {
        leftArrow.onClick.AddListener(demo02.GoBackward);
        rightArrow.onClick.AddListener(demo02.GoForward);
    }

    void OnDisable()
    {
        leftArrow.onClick.RemoveListener(demo02.GoBackward);
        rightArrow.onClick.RemoveListener(demo02.GoForward);
    }
}
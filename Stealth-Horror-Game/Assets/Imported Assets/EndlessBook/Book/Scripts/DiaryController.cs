using echo17.EndlessBook;
using echo17.EndlessBook.Demo02;
using UnityEngine;
using Zenject;

public class DiaryController : MonoBehaviour
{
    [System.Serializable]
    public class PageEntry
    {
        public int pageNumber;
        public Material pageMaterial;
        public AnimatedPage animatedPage;
    }

    [SerializeField] private EndlessBook book;
    [SerializeField] private Demo02 demo02;
    [SerializeField] private DiaryAutoPrinter autoPrinter;
    [SerializeField] private PageEntry[] pages;

    [Inject] private WorldSaveGameManager _saveManager;

    void Start()
    {
        DayData day = DiaryLaunchPayload.CurrentDay;
        if (day == null) { Debug.LogError("No day"); }

        ApplyDay(day);
        autoPrinter?.SetDay(day);

        demo02.OpenMiddleFromOutside();
    }

    public void ApplyDay(DayData day)
    {
        int target = Mathf.Max(1, day.diaryTotalPages);

        while (book.LastPageNumber > target)
            book.RemovePageData(book.LastPageNumber);

        for (int n = book.LastPageNumber + 1; n <= target; n++)
        {
            var e = GetEntry(n);
            if (e == null) { Debug.LogError($"No entry for page {n}"); break; }
            book.AddPageData(e.pageMaterial);
        }

        Debug.Log($"[DiaryController] Applied Day {day.chapterNumber}: {book.LastPageNumber} pages");
    }

    private PageEntry GetEntry(int n)
    {
        foreach (var e in pages) if (e.pageNumber == n) return e;
        return null;
    }
}
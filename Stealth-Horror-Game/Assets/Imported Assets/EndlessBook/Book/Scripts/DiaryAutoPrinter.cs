using echo17.EndlessBook;
using echo17.EndlessBook.Demo02;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class DiaryAutoPrinter : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private EndlessBook book;
    [SerializeField] private Demo02 demo02;
    [SerializeField] private AnimatedPage[] allPages;
    [SerializeField] private float turnTime = 0.6f;

    [Inject] private WorldSaveGameManager _saveManager;

    private DayData currentDay;
    private readonly Queue<int> _queue = new();
    private bool _running;

    private void OnEnable()
    {
        foreach (var p in allPages)
            if (p != null) p.OnPrintCompleted += HandlePrintCompleted;
    }

    private void OnDisable()
    {
        foreach (var p in allPages)
            if (p != null) p.OnPrintCompleted -= HandlePrintCompleted;
    }

    public void SetDay(DayData day)
    {
        currentDay = day;
    }

    /// <summary>Вызывать при переходе книги в OpenMiddle.</summary>
    public void StartSequence()
    {
        if (_running) return;
        if (book.CurrentState != EndlessBook.StateEnum.OpenMiddle) return;

        var day = DiaryLaunchPayload.CurrentDay;   // ← берём здесь, а не из поля
        if (day == null || day.diaryPagesToPrint == null || day.diaryPagesToPrint.Count == 0)
            return;

        var printed = _saveManager.currentCharacterSaveData.diaryPrintedSpreads;

        _queue.Clear();
        foreach (var p in day.diaryPagesToPrint)
        {
            if (p < 1 || p > day.diaryTotalPages) continue;
            if (printed.Contains(p)) continue;
            _queue.Enqueue(p);
        }

        if (_queue.Count == 0) return;

        _running = true;

        int openOnPage = _queue.Peek();
        Debug.Log($"[AutoPrinter] Starting: queue=[{string.Join(",", _queue)}], openOn={openOnPage}");

        if (!IsCurrentSpread(openOnPage))
            book.TurnToPage(openOnPage, EndlessBook.PageTurnTimeTypeEnum.TimePerPage, turnTime);
    }

    /// <summary>Вызывать при уходе из OpenMiddle или закрытии дневника.</summary>
    public void CancelSequence()
    {
        if (!_running) return;
        _running = false;
        _queue.Clear();
    }

    private void HandlePrintCompleted(int pageNumber)
    {
        if (currentDay == null)
        {
            Debug.LogWarning("[AutoPrinter] currentDay null");
            return;
        }

        var printed = _saveManager.currentCharacterSaveData.diaryPrintedSpreads;

        // ищем следующую недоигранную страницу, которая идёт после только что напечатанной
        int next = -1;
        foreach (var p in currentDay.diaryPagesToPrint)
        {
            if (p <= pageNumber) continue;
            if (printed.Contains(p)) continue;
            if (next == -1 || p < next) next = p;
        }

        if (next == -1)
        {
            Debug.Log($"[AutoPrinter] No more pages after {pageNumber}, done");
            return;
        }

        Debug.Log($"[AutoPrinter] {pageNumber} done, turning to {next}");
        StartCoroutine(TurnWhenReady(next));
    }

    private System.Collections.IEnumerator TurnWhenReady(int pageNumber)
    {
        while (book.IsTurningPages || book.IsDraggingPage)
            yield return null;

        if (book.CurrentLeftPageNumber == pageNumber || book.CurrentRightPageNumber == pageNumber)
            yield break;

        demo02.TurnToPageExternal(pageNumber);   // ← вот здесь
    }

    private bool IsCurrentSpread(int pageNumber)
    {
        return book.CurrentLeftPageNumber == pageNumber
            || book.CurrentRightPageNumber == pageNumber;
    }
}
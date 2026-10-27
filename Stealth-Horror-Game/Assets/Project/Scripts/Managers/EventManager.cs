using System;

/// <summary>
/// Централизованная система событий для коммуникации между компонентами
/// </summary>
public class EventManager
{
    // Событие начала нового дня или ночи
    public event Action<DayData> OnPhaseStart;
    public void ReportPhaseStart(DayData data) => OnPhaseStart?.Invoke(data);

    // Событие окончания дня или ночи
    public event Action<DayData> OnPhaseEnd;
    public void ReportPhaseEnd(DayData data) => OnPhaseEnd?.Invoke(data);

    // Событие завершения мини-игры. Передаем ItemData, чтобы знать, какой предмет засчитать.
    public event Action OnMiniGameCompleted;

    public void ReportMiniGameCompleted()
    {
        //Debug.Log("!!!ReportMiniGameCompleted!!!");
        OnMiniGameCompleted?.Invoke();
    }

    #region Talk Events
    // Событие завершения и начала разговоров.
    public event Action OnTalkingStarted;
    public event Action OnTalkingEnded;

    public void ReportTalkingStarted() => OnTalkingStarted?.Invoke();
    public void ReportTalkingEnded() => OnTalkingEnded?.Invoke();
    #endregion
}
using System;

/// <summary>
/// Централизованная система событий для коммуникации между компонентами
/// </summary>
public class EventManager
{
    // Событие начала нового дня
    public event Action<DayData> OnDayStart;
    public void ReportDayStart(DayData data) => OnDayStart?.Invoke(data);

    // Событие окончания дня
    public event Action<DayData> OnDayEnd;
    public void ReportDayEnd(DayData data) => OnDayEnd?.Invoke(data);

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
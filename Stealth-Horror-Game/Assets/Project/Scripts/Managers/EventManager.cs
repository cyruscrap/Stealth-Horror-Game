using System;

/// <summary>
/// Централизованная система событий для коммуникации между компонентами
/// </summary>
public class EventManager
{
    // Событие завершения мини-игры. Передаем ItemData, чтобы знать, какой предмет засчитать.
    public event Action OnMiniGameCompleted;

    public void ReportMiniGameCompleted()
    {
        //Debug.Log("!!!ReportMiniGameCompleted!!!");
        OnMiniGameCompleted?.Invoke();
    }
}
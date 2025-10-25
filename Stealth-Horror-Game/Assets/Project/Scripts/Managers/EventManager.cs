using System;

/// <summary>
/// ���������������� ������� ������� ��� ������������ ����� ������������
/// </summary>
public class EventManager
{
    // ������� ���������� ����-����. �������� ItemData, ����� �����, ����� ������� ���������.
    public event Action OnMiniGameCompleted;

    public void ReportMiniGameCompleted()
    {
        //Debug.Log("!!!ReportMiniGameCompleted!!!");
        OnMiniGameCompleted?.Invoke();
    }
}
using UnityEngine;
using Zenject;

public class MiniGameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // ���� Core ������� �� ����������� (����-���� �������� ��������), 
        // ������� �������� ��� �������������
        if (!Container.HasBinding<EventManager>())
        {
            Debug.LogWarning("MiniGameInstaller: EventManager �� ������, ������ �������� ��� �������������");
            Container.Bind<EventManager>().AsSingle();
        }
    }
}

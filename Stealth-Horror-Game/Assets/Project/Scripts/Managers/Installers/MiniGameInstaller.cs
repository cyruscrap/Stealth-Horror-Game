using UnityEngine;
using Zenject;

public class MiniGameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // ≈сли Core системы не установлены (мини-игра запущена отдельно), 
        // создаем заглушки дл€ совместимости
        if (!Container.HasBinding<EventManager>())
        {
            Debug.LogWarning("MiniGameInstaller: EventManager не найден, создаю заглушку дл€ совместимости");
            Container.Bind<EventManager>().AsSingle();
        }
    }
}

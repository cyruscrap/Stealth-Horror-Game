using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class GlobalInstaller : MonoInstaller
{
    [SerializeField] private GameObject eventSystemPrefab;

    public override void InstallBindings()
    {
        // ќсновные системы (ќЅя«ј“≈Ћ№Ќќ!)
        Container.Bind<EventManager>().AsSingle();
        Container.BindInterfacesAndSelfTo<WorldSaveGameManager>().AsSingle().NonLazy();

        Container.Bind<EventSystem>()
            .FromComponentInNewPrefab(eventSystemPrefab)
            .AsSingle()
            .NonLazy(); // —оздастс€ автоматически при запуске игры

        Container.BindExecutionOrder<WorldSaveGameManager>(-100);
    }
}
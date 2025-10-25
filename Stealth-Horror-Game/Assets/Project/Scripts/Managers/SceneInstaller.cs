using UnityTutorial.PlayerControl;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Игровое ядро (специфичное для основной игры)
        Container.BindInterfacesAndSelfTo<GameScenario>().FromNew().AsSingle().NonLazy();

        // Контроллеры
        Container.Bind<PlayerController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PlayerInteractionManager>().FromComponentInHierarchy().AsSingle();

        // UI Managers
        Container.Bind<InteractionPromptManager>().FromInstance(InteractionPromptManager.Instance).AsSingle();
    }
}
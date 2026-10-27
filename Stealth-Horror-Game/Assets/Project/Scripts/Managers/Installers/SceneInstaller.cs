using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityTutorial.Manager;
using UnityTutorial.PlayerControl;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    [SerializeField]
    [Tooltip("Перетащить сюда все ассеты DayData в нужном порядке")]
    private List<DayData> allDaysData;

    [SerializeField] private Volume globalVolume;

    public override void InstallBindings()
    {
        // Данные
        Container.Bind<List<DayData>>().FromInstance(allDaysData).AsSingle();

        // Игровое ядро (специфичное для основной игры)
        Container.BindInterfacesAndSelfTo<GameScenario>().FromNew().AsSingle().NonLazy();

        // Контроллеры
        Container.Bind<PlayerController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PlayerUIManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PlayerInteractionManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PlayerDataSaver>().FromComponentInHierarchy().AsSingle();
        Container.Bind<InputManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<CutsceneManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<OptionsMenu>().FromComponentInHierarchy(includeInactive: true).AsSingle();
        Container.Bind<GlobalInteractionState>().AsSingle();
        Container.Bind<ConditionEvaluator>().AsSingle();

        // Пост-обработка
        Container.Bind<Volume>().FromInstance(globalVolume).AsSingle();
        Container.Bind<PostProcessingManager>().AsSingle();

        // UI Managers
        Container.Bind<InteractionPromptManager>().FromInstance(InteractionPromptManager.Instance).AsSingle();
        Container.Bind<ScreenFaderTransition>().FromInstance(ScreenFaderTransition.Instance).AsSingle();
    }
}
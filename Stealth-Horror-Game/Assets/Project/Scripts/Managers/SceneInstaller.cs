using UnityTutorial.PlayerControl;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // ������� ���� (����������� ��� �������� ����)
        Container.BindInterfacesAndSelfTo<GameScenario>().FromNew().AsSingle().NonLazy();

        // �����������
        Container.Bind<PlayerController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PlayerInteractionManager>().FromComponentInHierarchy().AsSingle();

        // UI Managers
        Container.Bind<InteractionPromptManager>().FromInstance(InteractionPromptManager.Instance).AsSingle();
    }
}
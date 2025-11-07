using Zenject;

public class GlobalInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // ќсновные системы (ќЅя«ј“≈Ћ№Ќќ!)
        Container.Bind<EventManager>().AsSingle();
    }
}
public interface IInteractable
{
    void OnFocus(bool showOutline);      // Вызывается, когда игрок наводит взгляд на объект
    void OnLoseFocus();  // Вызывается, когда игрок отводит взгляд
    void Interact();     // Вызывается при нажатии клавиши взаимодействия
    bool CanBeInteracted();
}
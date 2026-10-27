using UnityEngine;
using Zenject;

public class InteractedWithAnyCondition : InteractionConditionBase
{
    [Tooltip("Список ID объектов, с которыми мог взаимодействовать игрок")]
    [SerializeField] private string[] targetObjectIds;

    [Inject] private WorldSaveGameManager _saveManager;

    public override bool IsMet()
    {
        if (targetObjectIds == null || targetObjectIds.Length == 0)
            return true; // если список пуст, условие считается выполненным (или наоборот, в зависимости от логики)

        var interacted = _saveManager.currentCharacterSaveData.interactedObjects;
        foreach (string id in targetObjectIds)
        {
            if (interacted.Contains(id))
                return true;
        }
        return false;
    }
}
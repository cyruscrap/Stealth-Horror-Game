using UnityEngine;
using Zenject;

public class ItemInteractedCondition : InteractionConditionBase
{
    [SerializeField] private InteractableItem _interactableItem;

    [Inject] private WorldSaveGameManager _saveManager;

    private void Awake()
    {
        if (_interactableItem == null)
        {
            _interactableItem = GetComponent<InteractableItem>();
            if (_interactableItem == null)
                Debug.LogError("InteractableItem reference is not set in ItemInteractedCondition.");
        }
    }

    public override bool IsMet()
    {
        if (_interactableItem == null || _saveManager == null)
            return false;

        var interacted = _saveManager.currentCharacterSaveData?.interactedObjects;
        if (interacted == null) return false;

        return interacted.Contains(_interactableItem.EffectiveObjectId);
    }
}
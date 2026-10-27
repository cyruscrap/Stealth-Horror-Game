using I2.Loc;
using TMPro;
using UnityEngine;

public class SaveSlotButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI slotLabel;
    [SerializeField] private TextMeshProUGUI dayLabel;
    private int slotIndex;
    private System.Action<int> _onSlotClicked;

    public void Initialize(int index, SaveSlotInfo info, WorldSaveGameManager manager, System.Action<int> onSlotClicked)
    {
        slotIndex = index;
        _onSlotClicked = onSlotClicked;

        // "Slot 1", "Slot 2"...
        slotLabel.text = $"{LocalizationManager.GetTranslation("Slot")} {index + 1}";

        if (info.isEmpty)
        {
            dayLabel.text = LocalizationManager.GetTranslation("Empty");
            return;
        }

        // Ключи в I2: "Day" и "Night"
        string phaseKey = info.phaseType == PhaseType.Day ? "Day" : "Night";
        string phaseLabel = LocalizationManager.GetTranslation(phaseKey);

        dayLabel.text = $"{phaseLabel} {info.chapterNumber}";
    }

    public void OnClick()
    {
        _onSlotClicked?.Invoke(slotIndex);
    }
}
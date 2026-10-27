using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Zenject;

[System.Serializable]
public class SaveSlotInfo
{
    public int slotIndex;
    public string fileName;
    public string lastSaveTime;
    public int chapterNumber;
    public PhaseType phaseType;
    public bool isEmpty;
}

public class WorldSaveGameManager : IInitializable
{
    public const int MaxSlots = 3;

    private int currentSlotIndex = 0;
    private string CurrentFileName => $"SaveSlot_{currentSlotIndex}";
    public int CurrentSlotIndex => currentSlotIndex;

    [Header("Save Data Writer")]
    SaveGameDataWriter saveGameDataWriter = new SaveGameDataWriter();

    [Header("Current Character Data")]
    private CharacterSaveData _currentCharacterSaveData;
    public CharacterSaveData currentCharacterSaveData
    {
        get
        {
            if (_currentCharacterSaveData == null)
            {
                _currentCharacterSaveData = new CharacterSaveData();
                if (_currentCharacterSaveData.itemsInWorld == null)
                    _currentCharacterSaveData.itemsInWorld = new SerializableDictionary<string, bool>();
            }
            return _currentCharacterSaveData;
        }
        set
        {
            _currentCharacterSaveData = value;
            if (_currentCharacterSaveData != null && _currentCharacterSaveData.itemsInWorld == null)
                _currentCharacterSaveData.itemsInWorld = new SerializableDictionary<string, bool>();
        }
    }

    public string fileName = "SaveGameDataFile";
    public float loadingProgress;

    public event System.Action OnStateChanged;

    // вызывать после любого изменения данных
    public void NotifyStateChanged() => OnStateChanged?.Invoke();

    void IInitializable.Initialize()
    {
        LoadGame();
    }

    public void SetCurrentSlot(int index)
    {
        if (index < 0 || index >= MaxSlots)
            throw new System.ArgumentOutOfRangeException(nameof(index));
        currentSlotIndex = index;
    }

    public bool ReturnCurrentWorldItemState(string ID)
    {
        if (currentCharacterSaveData.itemsInWorld.ContainsKey(ID) == false)
            currentCharacterSaveData.itemsInWorld.Add(ID, false);
        return currentCharacterSaveData.itemsInWorld[ID];
    }

    public void UpdateCurrentWorldItem(string ID, bool deactivate)
    {
        if (currentCharacterSaveData.itemsInWorld.ContainsKey(ID))
            currentCharacterSaveData.itemsInWorld.Remove(ID);
        currentCharacterSaveData.itemsInWorld.Add(ID, deactivate);
    }

    public bool CheckIfSaveFileExists()
    {
        saveGameDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
        saveGameDataWriter.dataSaveFileName = CurrentFileName;
        return saveGameDataWriter.CheckIfSaveFileExists();
    }

    public void DeleteSaveFile()
    {
        saveGameDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
        saveGameDataWriter.dataSaveFileName = CurrentFileName;
        saveGameDataWriter.DeleteSaveFile();
        // Удаляем метаданные
        PlayerPrefs.DeleteKey($"Slot_{currentSlotIndex}_Meta");
    }

    public void SaveGame(int chapterNumber, PhaseType phaseType)
    {
        PlayerPrefsExtentions.SetBool("isNewGame", false);

        CharacterSaveData dataToSave = currentCharacterSaveData; // гарантирует не-null
        saveGameDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
        saveGameDataWriter.dataSaveFileName = CurrentFileName;
        saveGameDataWriter.WriteCharacterDataToSaveFile(dataToSave);

        // Сохраняем метаданные слота (опционально)
        SaveSlotMetadata(chapterNumber, phaseType);
    }

    public void LoadGame()
    {
        string saveFilePath = Path.Combine(Application.persistentDataPath, CurrentFileName);
        if (!File.Exists(saveFilePath))
        {
            currentCharacterSaveData = new CharacterSaveData();
        }
        else
        {
            saveGameDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
            saveGameDataWriter.dataSaveFileName = CurrentFileName;
            CharacterSaveData loadedData = saveGameDataWriter.LoadCharacterDataFromJson();
            currentCharacterSaveData = loadedData ?? new CharacterSaveData();
        }

        // Гарантируем, что все списки и словари инициализированы
        currentCharacterSaveData.EnsureListsInitialized();

        // Дополнительная проверка для itemsInWorld уже есть, но можно оставить
        if (currentCharacterSaveData.itemsInWorld == null)
            currentCharacterSaveData.itemsInWorld = new SerializableDictionary<string, bool>();
    }

    private void SaveSlotMetadata(int chapterNumber, PhaseType phaseType)
    {
        string key = $"Slot_{currentSlotIndex}_Meta";
        PlayerPrefs.SetString(key + "_Time", System.DateTime.Now.ToString());
        PlayerPrefs.SetInt(key + "_Chapter", chapterNumber);
        PlayerPrefs.SetInt(key + "_PhaseType", (int)phaseType);
        PlayerPrefs.Save();
    }

    public List<SaveSlotInfo> GetSaveSlotsInfo()
    {
        var list = new List<SaveSlotInfo>();
        for (int i = 0; i < MaxSlots; i++)
        {
            var info = new SaveSlotInfo { slotIndex = i, fileName = $"SaveSlot_{i}" };
            string path = Path.Combine(Application.persistentDataPath, info.fileName);
            if (File.Exists(path))
            {
                info.lastSaveTime = PlayerPrefs.GetString($"Slot_{i}_Meta_Time", File.GetLastWriteTime(path).ToString());

                int phaseIndex = PlayerPrefs.GetInt($"Slot_{i}_Meta_PhaseIndex", 0);
                // Если у менеджера есть доступ к списку фаз:
                // var phase = _allPhases[Mathf.Clamp(phaseIndex, 0, _allPhases.Count - 1)];
                // info.chapterNumber = phase.chapterNumber;
                // info.phaseType = phase.phaseType;

                // Или если вы храните эти данные прямо в PlayerPrefs:
                info.chapterNumber = PlayerPrefs.GetInt($"Slot_{i}_Meta_Chapter", 1);
                info.phaseType = (PhaseType)PlayerPrefs.GetInt($"Slot_{i}_Meta_PhaseType", 0);

                info.isEmpty = false;
            }
            else
            {
                info.isEmpty = true;
            }
            list.Add(info);
        }
        return list;
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Вспомогательный класс для настройки спауна предметов в инспекторе.
/// </summary>
[System.Serializable]
public class ItemSpawnInfo
{
    public ItemData itemToSpawn;

    [Header("Transform")]
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale = Vector3.one;
}

public enum PhaseType { Day = 0, Night = 1, Any = -1 }

/// <summary>
/// Хранит всю конфигурацию для одного игрового дня.
/// </summary>
[CreateAssetMenu(fileName = "PhaseData_", menuName = "StealthHorrorGame/PhaseData")]
public class DayData : ScriptableObject
{
    public string phaseID;
    public PhaseType phaseType = PhaseType.Day;
    [Header("Chapter Number")]
    [Tooltip("Номер дня и ночи")]
    [Range(1, 12)]
    public int chapterNumber = 1;

    [Header("Lighting")]
    public LightingPreset lightingPreset;

    [Header("Предметы и их расположение")]
    public List<ItemSpawnInfo> itemsToSpawn;

    [Header("Страницы дневника")]
    [Tooltip("Общее количество страниц дневника, доступных на этот день. Включая уже напечатанные ранее.")]
    [Range(1, 40)]
    public int diaryTotalPages = 2;
    [Tooltip("Какие страницы должны быть напечатаны именно в этот день (первый раз).")]
    public List<int> diaryPagesToPrint = new List<int>();

    [Header("Эмбиент")]
    public AudioClip backgroundMusic;
    [Range(0f, 10f)] public float loopDelay;

    [Header("Позиция и поворот игрока")]
    [Tooltip("Стартовая позиция игрока для этого дня")]
    public Vector3 playerStartPosition;
    [Tooltip("Стартовый поворот игрока (углы Эйлера) для этого день")]
    public Vector3 playerStartRotation;

    [Space(30f)]
    [Header("МИНИ-ИГРЫ")]
    [Header("Плита")]
    [Tooltip("Включить мини-игру готовки при взаимодействии с плитой")]
    public bool enableCooking = false;
    [Tooltip("Предмет, который переносит в мини-игру готовки")]
    public ItemTag tagTransitionToCookingMiniGame = ItemTag.Stove;

    /// <summary>
    /// Возвращает список уникальных предметов, которые нужно собрать в этот день.
    /// </summary>
    public List<ItemData> GetRequiredItems()
    {
        return itemsToSpawn.Select(info => info.itemToSpawn).ToList();
    }
}
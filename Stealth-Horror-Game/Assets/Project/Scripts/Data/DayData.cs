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

/// <summary>
/// Хранит всю конфигурацию для одного игрового дня.
/// </summary>
[CreateAssetMenu(fileName = "DayData_", menuName = "StealthHorrorGame/DayData")]
public class DayData : ScriptableObject
{
    public string dayId;

    [Header("Day Number")]
    [Tooltip("Номер дня (1-5)")]
    [Range(1, 5)]
    public int dayNumber = 1;

    [Header("Предметы и их расположение")]
    public List<ItemSpawnInfo> itemsToSpawn;

    [Header("Эмбиент")]
    public AudioClip backgroundMusic;
    [Range(0f, 10f)] public float loopDelay;

    [Header("Звуки переходов")]
    public AudioClip goOutsideSound;
    public AudioClip goInsideSound;
    public AudioClip transitionSound;

    [Header("Настройки звонка и диалогов")]
    public string ringConversationMark;
    public AudioClip ringAudio;

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
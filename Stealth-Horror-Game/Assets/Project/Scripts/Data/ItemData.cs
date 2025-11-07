using UnityEngine;

public enum ItemTag
{
    Diary,
    Phone,
    Stove
}

/// <summary>
/// Хранит всю информацию о конкретном предмете: его вид, имя и тип (тег).
/// </summary>
[CreateAssetMenu(fileName = "Item_", menuName = "StealthHorrorGame/ItemData")]
public class ItemData : ScriptableObject
{
    [Tooltip("Тег для идентификации типа предмета (например, Key, Document)")]
    public ItemTag tag;

    [Tooltip("Имя, которое будет отображаться в UI")]
    public string displayName = "Новый предмет";

    [Tooltip("Ключ таблицы I2 Localization")]
    public string localizationKey;

    [Tooltip("Префаб 3D-модели, который будет создан в сцене")]
    public GameObject prefab;
}
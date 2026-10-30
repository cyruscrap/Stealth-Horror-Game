using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterSaveData
{
    [Header("Player")]
    [Header("Level")]
    public int phaseIndex = 0;

    [Header("Transform")]
    public Vector3 playerPosition;
    public Vector3 playerRotation;

    [Header("Items Looted From World")]
    public SerializableDictionary<string, bool> itemsInWorld; // The string is the world item ID, the bool is if the item has been looted
    public string lastVisitedMainPathRoomID;

    [Header("Languages")]
    public string language;

    [Header("Interactions")]
    public List<string> interactedObjects = new List<string>(); // ID объектов, с которыми взаимодействовали

    [Header("Item Interactions")]
    public List<string> shownItemInteractions = new List<string>(); // ID показанных одноразовых взаимодействий

    [Header("Zone Comments")]
    public List<string> shownComments = new List<string>();  // ID показанных одноразовых комментариев
    public List<string> visitedZones = new List<string>();   // ID посещённых зон (для OnFirstEnter)

    [Header("Cutscenes")]
    public List<string> playedCutscenes = new List<string>();

    [Header("Events")]
    public List<string> triggeredEvents = new List<string>();

    [Header("Diary")]
    public List<int> diaryPrintedSpreads = new List<int>(); // индексы полностью показанных страниц
    public int diaryLastOpenedPage = 0;                    // последняя открытая страница

    public CharacterSaveData()
    {
        itemsInWorld = new SerializableDictionary<string, bool>();
        interactedObjects = new List<string>();
        shownItemInteractions = new List<string>();
        shownComments = new List<string>();
        visitedZones = new List<string>();
        playedCutscenes = new List<string>();
        triggeredEvents = new List<string>();
        diaryPrintedSpreads = new List<int>();
    }

    public void SetDefaultValues()
    {
        // Сбрасываем простые поля
        phaseIndex = 0;
        playerPosition = Vector3.zero;
        playerRotation = Vector3.zero;
        language = "English";
        diaryLastOpenedPage = 0;

        // Очищаем все списки и словари
        if (itemsInWorld != null) itemsInWorld.Clear();
        if (interactedObjects != null) interactedObjects.Clear();
        if (shownItemInteractions != null) shownItemInteractions.Clear();
        if (shownComments != null) shownComments.Clear();
        if (visitedZones != null) visitedZones.Clear();
        if (playedCutscenes != null) playedCutscenes.Clear();
        if (triggeredEvents != null) triggeredEvents.Clear();
        if (diaryPrintedSpreads != null) diaryPrintedSpreads.Clear();
    }

    public void EnsureListsInitialized()
    {
        if (itemsInWorld == null) itemsInWorld = new SerializableDictionary<string, bool>();
        if (shownComments == null) shownComments = new List<string>();
        if (visitedZones == null) visitedZones = new List<string>();
        if (shownItemInteractions == null) shownItemInteractions = new List<string>();
        if (interactedObjects == null) interactedObjects = new List<string>();
        if (playedCutscenes == null) playedCutscenes = new List<string>();
        if (triggeredEvents == null) triggeredEvents = new List<string>();
        if (diaryPrintedSpreads == null) diaryPrintedSpreads = new List<int>();

        // УБРАНО: diaryLastOpenedPage = 0; 
        // (Он должен загружаться из сейва, а не сбрасываться при каждой загрузке)
    }
}
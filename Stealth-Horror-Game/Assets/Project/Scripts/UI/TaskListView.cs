using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TaskListView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform container;
    [SerializeField] private TaskView taskViewPrefab;

    // ”бираем список из инспектора, он будет заполн€тьс€ кодом
    private List<TaskData> _allTasks = new List<TaskData>();

    [Inject] private WorldSaveGameManager _saveManager;
    [Inject] private ConditionEvaluator _evaluator;

    private Dictionary<string, TaskView> _activeViews = new Dictionary<string, TaskView>();

    private void Awake()
    {
        // јвтоматически загружаем ¬—≈ TaskData из папки Resources/Tasks
        TaskData[] loadedTasks = Resources.LoadAll<TaskData>("Tasks");
        _allTasks.AddRange(loadedTasks);

        Debug.Log($"[TaskList] «агружено тасков: {_allTasks.Count}");
    }

    private void OnEnable()
    {
        _saveManager.OnStateChanged += UpdateTaskList;
        UpdateTaskList();
    }

    private void OnDisable()
    {
        _saveManager.OnStateChanged -= UpdateTaskList;
    }

    private void UpdateTaskList()
    {
        foreach (var task in _allTasks)
        {
            if (task == null || string.IsNullOrEmpty(task.taskID)) continue;

            bool isVisible = _activeViews.ContainsKey(task.taskID);
            bool showMet = _evaluator.AllGroupsMet(task.showConditions);
            bool completeMet = _evaluator.AllGroupsMet(task.completeConditions);

            // 1. ≈сли задача выполнена Ч скрываем еЄ
            if (completeMet)
            {
                if (isVisible) StartCoroutine(RemoveTask(task.taskID));
                continue;
            }

            // 2. ≈—Ћ» ”—Ћќ¬»я ѕќя¬Ћ≈Ќ»я ЅќЋ№Ў≈ Ќ≈ ¬џѕќЋЌяё“—я Ч “ќ∆≈ — –џ¬ј≈ћ
            if (isVisible && !showMet)
            {
                StartCoroutine(RemoveTask(task.taskID));
                continue;
            }

            // 3. ≈сли задача ещЄ не на экране, но услови€ выполнены Ч показываем
            if (!isVisible && showMet)
            {
                AddTask(task);
            }
        }
    }

    private void AddTask(TaskData task)
    {
        TaskView newView = Instantiate(taskViewPrefab, container);
        newView.Initialize(task.taskTextKey);
        _activeViews.Add(task.taskID, newView);
    }

    private IEnumerator RemoveTask(string taskID)
    {
        if (_activeViews.TryGetValue(taskID, out var view))
        {
            _activeViews.Remove(taskID); // ”бираем сразу, чтобы не запустить дважды
            yield return view.FadeOutAndDestroy();
        }
    }
}
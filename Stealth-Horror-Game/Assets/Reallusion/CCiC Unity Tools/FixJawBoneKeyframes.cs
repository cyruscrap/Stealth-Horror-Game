// Copyright(c) 2021 Robin Promesberger (schema_unity on forum.unity3d.com)
// Modified: добавлено авто-дублирование FBX-клипов, сохранение на диск,
//           пропуск лип-синк анимаций.
//
// MIT License — см. оригинал.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class FixJawBoneKeyframes : EditorWindow
{
    [MenuItem("Window/Fix Jawbone Keyframes")]
    public static void OpenWindow()
    {
        var window = GetWindow<FixJawBoneKeyframes>();
        window.titleContent = new GUIContent("Jaw Fixer");
        window.minSize = new Vector2(420, 560);
    }

    public List<AnimationClip> animations = new List<AnimationClip>();

    private float keyValue = 1f;
    private string bindingName = "Jaw Close";
    private string statusText = "Waiting...";
    private GUIContent statusContent = new GUIContent("Waiting...");

    private bool autoDuplicate = true;
    private bool skipLipsync = true;
    private float lipsyncThreshold = 0.1f;
    private string outputFolder = "Assets/Project/FixedAnimations";

    private Vector2 listScroll;
    private Vector2 statusScroll;

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Настройки", EditorStyles.boldLabel);
        keyValue = EditorGUILayout.FloatField("Значение Jaw Close", keyValue);
        bindingName = EditorGUILayout.TextField("Имя биндинга", bindingName);

        skipLipsync = EditorGUILayout.Toggle("Пропускать лип-синк анимации", skipLipsync);
        if (skipLipsync)
        {
            EditorGUI.indentLevel++;
            lipsyncThreshold = EditorGUILayout.Slider(
                new GUIContent("Порог вариации",
                    "Если разница между макс. и мин. значением Jaw Close больше порога — клип считается лип-синком и не трогается"),
                lipsyncThreshold, 0.01f, 1f);
            EditorGUI.indentLevel--;
        }

        autoDuplicate = EditorGUILayout.Toggle(
            new GUIContent("Автосоздавать .anim для FBX",
                "Если клип встроен в FBX (только для чтения) — создать его редактируемую копию в папке ниже"),
            autoDuplicate);

        if (autoDuplicate)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            outputFolder = EditorGUILayout.TextField("Папка для копий", outputFolder);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string abs = EditorUtility.OpenFolderPanel("Папка внутри Assets",
                    Application.dataPath, "");
                if (!string.IsNullOrEmpty(abs) && abs.StartsWith(Application.dataPath))
                    outputFolder = "Assets" + abs.Substring(Application.dataPath.Length);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // === Drag & Drop бокс ===
        GUIStyle dndStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter
        };
        Rect dndRect = GUILayoutUtility.GetRect(0, 42, GUILayout.ExpandWidth(true));
        GUI.Box(dndRect, "Перетащи сюда AnimationClip (можно несколько)", dndStyle);

        if (dndRect.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use();
            }
            else if (Event.current.type == EventType.DragPerform)
            {
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (obj is AnimationClip c && !animations.Contains(c))
                        animations.Add(c);
                }
                Event.current.Use();
            }
        }

        // === Список клипов ===
        EditorGUILayout.LabelField("Список клипов", EditorStyles.boldLabel);
        listScroll = EditorGUILayout.BeginScrollView(listScroll, GUILayout.MaxHeight(180));

        int newCount = Mathf.Max(0, EditorGUILayout.DelayedIntField("Размер", animations.Count));
        while (newCount < animations.Count) animations.RemoveAt(animations.Count - 1);
        while (newCount > animations.Count) animations.Add(null);

        for (int i = 0; i < animations.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            animations[i] = EditorGUILayout.ObjectField(
                animations[i], typeof(AnimationClip), true) as AnimationClip;
            if (GUILayout.Button("X", GUILayout.Width(22)))
            {
                animations.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        // === Кнопка ===
        bool enabled = animations != null && animations.Count > 0 && animations.Any(a => a != null);
        if (!enabled) statusText = "Добавь анимационные клипы для обработки.";

        GUI.enabled = enabled;
        if (GUILayout.Button("Fix Jaw Bones", GUILayout.Height(32)))
        {
            ProcessAnimations();
        }
        GUI.enabled = true;

        // === Статус ===
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Статус", EditorStyles.boldLabel);

        statusContent.text = statusText;
        EditorStyles.label.wordWrap = true;

        statusScroll = EditorGUILayout.BeginScrollView(statusScroll, GUI.skin.box);
        GUILayout.Label(statusContent);
        EditorGUILayout.EndScrollView();
    }

    private void ProcessAnimations()
    {
        statusText = "++ Обработка анимаций... ++\n\n";

        if (autoDuplicate && !AssetDatabase.IsValidFolder(outputFolder))
        {
            string parent = Path.GetDirectoryName(outputFolder).Replace('\\', '/');
            string leaf = Path.GetFileName(outputFolder);
            if (string.IsNullOrEmpty(parent) || !AssetDatabase.IsValidFolder(parent))
            {
                statusText += $"ERROR: родительская папка '{parent}' не существует. Создай её вручную.\n";
                return;
            }
            AssetDatabase.CreateFolder(parent, leaf);
        }

        int processed = 0, skippedLipsync = 0, duplicated = 0, errors = 0;
        var newAssets = new List<AnimationClip>();

        foreach (AnimationClip clip in animations)
        {
            if (clip == null) continue;

            // --- 1. Ищем Jaw Close кривую ---
            AnimationCurve curve = null;
            EditorCurveBinding sourceBinding = default;
            bool found = false;

            foreach (var b in AnimationUtility.GetCurveBindings(clip))
            {
                if (b.propertyName == bindingName)
                {
                    curve = AnimationUtility.GetEditorCurve(clip, b);
                    sourceBinding = b;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                statusText += $"SKIP: '{clip.name}' — биндинг '{bindingName}' не найден (не Humanoid?).\n";
                errors++;
                continue;
            }

            // --- 2. Проверяем, не лип-синк ли это ---
            if (skipLipsync && curve != null && curve.keys.Length > 1)
            {
                float min = curve.keys.Min(k => k.value);
                float max = curve.keys.Max(k => k.value);
                if (max - min > lipsyncThreshold)
                {
                    statusText += $"SKIP (lipsync): '{clip.name}' — Jaw Close меняется от {min:F2} до {max:F2}.\n";
                    skippedLipsync++;
                    continue;
                }
            }

            // --- 3. Read-only? (клип внутри FBX) ---
            string path = AssetDatabase.GetAssetPath(clip);
            bool isReadOnly = AssetDatabase.LoadMainAssetAtPath(path) is GameObject;

            AnimationClip target = clip;
            if (isReadOnly)
            {
                if (!autoDuplicate)
                {
                    statusText += $"SKIP: '{clip.name}' встроен в FBX. Включи 'Автосоздавать .anim' или сделай Duplicate вручную.\n";
                    errors++;
                    continue;
                }

                string newPath = AssetDatabase.GenerateUniqueAssetPath(
                    outputFolder + "/" + clip.name + "_fixed.anim");

                target = new AnimationClip { name = Path.GetFileNameWithoutExtension(newPath) };
                EditorUtility.CopySerialized(clip, target);
                target.name = Path.GetFileNameWithoutExtension(newPath);

                AssetDatabase.CreateAsset(target, newPath);
                newAssets.Add(target);
                duplicated++;
                statusText += $"Создан .anim: {newPath}\n";
            }

            // --- 4. Находим ту же кривую в target и меняем значения ---
            AnimationCurve targetCurve = null;
            EditorCurveBinding targetBinding = default;
            foreach (var b in AnimationUtility.GetCurveBindings(target))
            {
                if (b.propertyName == bindingName)
                {
                    targetCurve = AnimationUtility.GetEditorCurve(target, b);
                    targetBinding = b;
                    break;
                }
            }

            if (targetCurve == null)
            {
                statusText += $"ERROR: не удалось получить кривую в '{target.name}'.\n";
                errors++;
                continue;
            }

            Keyframe[] keys = targetCurve.keys;
            for (int k = 0; k < keys.Length; k++)
                keys[k].value = keyValue;
            targetCurve.keys = keys;

            AnimationUtility.SetEditorCurve(target, targetBinding, targetCurve);
            EditorUtility.SetDirty(target);

            statusText += $"✓ Обработан: {target.name}\n";
            processed++;
        }

        // --- Сохранение на диск ---
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        statusText += "\n=== Итог ===\n";
        statusText += $"Обработано: {processed}\n";
        statusText += $"Пропущено (лип-синк): {skippedLipsync}\n";
        statusText += $"Создано .anim-копий: {duplicated}\n";
        statusText += $"Ошибок/пропусков: {errors}\n";
        statusText += "Изменения сохранены на диск — не исчезнут после перезапуска.\n";

        if (newAssets.Count > 0)
            Selection.objects = newAssets.ToArray();

        animations.Clear();
    }
}
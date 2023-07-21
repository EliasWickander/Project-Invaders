using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class GameEventGenerator : EditorWindow
{
    private string m_gameEventName;
    private string m_gameEventType;
    private string m_targetDir;

    private const string c_defaultTargetDir = "Assets/Scripts/Util/GameEvents";
    
    [MenuItem("Custom Tools/Game Event Generator")]
    public static void ShowWindow()
    {
        GetWindow(typeof(GameEventGenerator));
    }

    private void OnGUI()
    {
        m_gameEventName = EditorGUILayout.TextField("Game Event Name", m_gameEventName);
        m_gameEventType = EditorGUILayout.TextField("Game Event Type", m_gameEventType);

        if (string.IsNullOrEmpty(m_targetDir))
            m_targetDir = c_defaultTargetDir;
        
        EditorGUILayout.BeginHorizontal();
        m_targetDir = EditorGUILayout.TextField("Target Directory", m_targetDir);

        if (GUILayout.Button(EditorGUIUtility.FindTexture("Folder Icon"), GetTargetDirButtonStyle()))
        {
            string path = EditorUtility.OpenFolderPanel("Set Target Directory", "", "");

            if (path.StartsWith(Application.dataPath) && path != Application.dataPath)
            {
                m_targetDir = "Assets" + path.Remove(0, Application.dataPath.Length);
            }
            else
            {
                m_targetDir = c_defaultTargetDir;
            }

            Debug.Log(path);
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Generate"))
        {
            Generate();
        }
    }

    private void Generate()
    {
        if (string.IsNullOrEmpty(m_gameEventName))
        {
            Debug.LogError("GameEventGenerator Error: Failed to generate. Reason: Invalid Game Event Name");
            return;
        }

        if (string.IsNullOrEmpty(m_targetDir))
        {
            Debug.LogError("GameEventGenerator Error: Failed to generate. Reason: Invalid Target Directory");
            return;
        }
        
        if (string.IsNullOrEmpty(m_gameEventType))
            GenerateVoidEvent();
        else
            GenerateTypedEvent();
    }

    private void GenerateTypedEvent()
    {
        //Event
        string gameEventAsText = $"using CustomToolkit.Events;\nusing UnityEngine;\n\n[CreateAssetMenu(fileName = \"{m_gameEventName}Event\", menuName = \"Custom/Game Events/{m_gameEventName}Event\")]\npublic class {m_gameEventName}Event : GameEvent<{m_gameEventType}> {{ }}";
        CreateFileFromText(gameEventAsText, $"{m_gameEventName}Event", $"{m_targetDir}/{m_gameEventName}");

        //Event Listener
        string gameEventListenerAsText = $"using CustomToolkit.Events;\n\npublic class {m_gameEventName}EventListener : GameEventListener<{m_gameEventType}, {m_gameEventName}Event> {{ }}";
        CreateFileFromText(gameEventListenerAsText, $"{m_gameEventName}EventListener", $"{m_targetDir}/{m_gameEventName}");
        
        //Event Trigger
        string gameEventTriggerAsText = $"using CustomToolkit.Events;\n\npublic class {m_gameEventName}EventTrigger : GameEventTrigger<{m_gameEventType}, {m_gameEventName}Event> {{ }}";
        CreateFileFromText(gameEventTriggerAsText, $"{m_gameEventName}EventTrigger", $"{m_targetDir}/{m_gameEventName}");
    }

    private void GenerateVoidEvent()
    {
        //Event
        string gameEventAsText = $"using CustomToolkit.Events;\nusing UnityEngine;\n\n[CreateAssetMenu(fileName = \"{m_gameEventName}Event\", menuName = \"Custom/Game Events/{m_gameEventName}Event\")]\npublic class {m_gameEventName}Event : GameEvent {{ }}";
        CreateFileFromText(gameEventAsText, $"{m_gameEventName}Event", $"{m_targetDir}/{m_gameEventName}");

        //Event Listener
        string gameEventListenerAsText = $"using CustomToolkit.Events;\n\npublic class {m_gameEventName}EventListener : GameEventListener<{m_gameEventName}Event> {{ }}";
        CreateFileFromText(gameEventListenerAsText, $"{m_gameEventName}EventListener", $"{m_targetDir}/{m_gameEventName}");
        
        //Event Trigger
        string gameEventTriggerAsText = $"using CustomToolkit.Events;\n\npublic class {m_gameEventName}EventTrigger : GameEventTrigger<{m_gameEventName}Event> {{ }}";
        CreateFileFromText(gameEventTriggerAsText, $"{m_gameEventName}EventTrigger", $"{m_targetDir}/{m_gameEventName}");
    }
    private void CreateFileFromText(string fileAsText, string fileName, string directory)
    {
        string filePath = $"{directory}/{fileName}.cs";
        
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        File.WriteAllText(filePath, fileAsText, Encoding.ASCII);
        AssetDatabase.ImportAsset(filePath);
    }
    private GUIStyle GetTargetDirButtonStyle()
    {
        GUIStyle targetDirButtonStyle = EditorStyles.iconButton;
        targetDirButtonStyle.fixedHeight = 20;
        targetDirButtonStyle.fixedWidth = 20;

        return targetDirButtonStyle;
    }
}

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

    private bool m_serverEvent = true;
    private bool m_clientEvent = true;
    
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
        }
        
        EditorGUILayout.EndHorizontal();

        float originalLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 50;
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        m_serverEvent = EditorGUILayout.Toggle("Server", m_serverEvent);
        m_clientEvent = EditorGUILayout.Toggle("Client", m_clientEvent);
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = originalLabelWidth;

        bool canGenerate = m_serverEvent || m_clientEvent;
        
        EditorGUI.BeginDisabledGroup(!canGenerate);
        
        if (GUILayout.Button("Generate"))
        {
            Generate();
        }
        
        EditorGUI.EndDisabledGroup();
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
        {
            if(m_serverEvent)
                GenerateVoidServerEvent();
            
            if(m_clientEvent)
                GenerateVoidClientEvent();   
        }
        else
        {
            if(m_serverEvent)
                GenerateTypedServerEvent();
            
            if(m_clientEvent)
                GenerateTypedClientEvent();   
        }
    }

    private void GenerateTypedClientEvent()
    {
        string dirPath = $"{m_targetDir}/{m_gameEventName}/Client";
        
        //Event
        string gameEventAsText = $"using CustomToolkit.Events;\nusing UnityEngine;\n\n[CreateAssetMenu(fileName = \"Client_{m_gameEventName}Event\", menuName = \"Custom/Game Events/Client/{m_gameEventName}Event\")]\npublic class Client_{m_gameEventName}Event : GameEvent<{m_gameEventType}> {{ }}";
        CreateFileFromText(gameEventAsText, $"Client_{m_gameEventName}Event", dirPath);

        //Event Listener
        string gameEventListenerAsText = $"using CustomToolkit.Events;\n\npublic class Client_{m_gameEventName}EventListener : GameEventListener<{m_gameEventType}, Client_{m_gameEventName}Event> {{ }}";
        CreateFileFromText(gameEventListenerAsText, $"Client_{m_gameEventName}EventListener", dirPath);
        
        //Event Trigger
        string gameEventTriggerAsText = $"using CustomToolkit.Events;\n\npublic class Client_{m_gameEventName}EventTrigger : GameEventTrigger<{m_gameEventType}, Client_{m_gameEventName}Event> {{ }}";
        CreateFileFromText(gameEventTriggerAsText, $"Client_{m_gameEventName}EventTrigger", dirPath);
    }

    private void GenerateVoidClientEvent()
    {
        string dirPath = $"{m_targetDir}/{m_gameEventName}/Client";
        
        //Event
        string gameEventAsText = $"using CustomToolkit.Events;\nusing UnityEngine;\n\n[CreateAssetMenu(fileName = \"Client_{m_gameEventName}Event\", menuName = \"Custom/Game Events/Client/{m_gameEventName}Event\")]\npublic class Client_{m_gameEventName}Event : GameEvent {{ }}";
        CreateFileFromText(gameEventAsText, $"Client_{m_gameEventName}Event", dirPath);

        //Event Listener
        string gameEventListenerAsText = $"using CustomToolkit.Events;\n\npublic class Client_{m_gameEventName}EventListener : GameEventListener<Client_{m_gameEventName}Event> {{ }}";
        CreateFileFromText(gameEventListenerAsText, $"Client_{m_gameEventName}EventListener", dirPath);
        
        //Event Trigger
        string gameEventTriggerAsText = $"using CustomToolkit.Events;\n\npublic class Client_{m_gameEventName}EventTrigger : GameEventTrigger<Client_{m_gameEventName}Event> {{ }}";
        CreateFileFromText(gameEventTriggerAsText, $"Client_{m_gameEventName}EventTrigger", dirPath);
    }
    
    private void GenerateTypedServerEvent()
    {
        string dirPath = $"{m_targetDir}/{m_gameEventName}/Server";
        
        //Event
        string gameEventAsText = $"using CustomToolkit.Events;\nusing UnityEngine;\n\n[CreateAssetMenu(fileName = \"Server_{m_gameEventName}Event\", menuName = \"Custom/Game Events/Server/{m_gameEventName}Event\")]\npublic class Server_{m_gameEventName}Event : GameEvent<{m_gameEventType}> {{ }}";
        CreateFileFromText(gameEventAsText, $"Server_{m_gameEventName}Event", dirPath);

        //Event Listener
        string gameEventListenerAsText = $"using CustomToolkit.Events;\n\npublic class Server_{m_gameEventName}EventListener : GameEventListener<{m_gameEventType}, Server_{m_gameEventName}Event> {{ }}";
        CreateFileFromText(gameEventListenerAsText, $"Server_{m_gameEventName}EventListener", dirPath);
        
        //Event Trigger
        string gameEventTriggerAsText = $"using CustomToolkit.Events;\n\npublic class Server_{m_gameEventName}EventTrigger : GameEventTrigger<{m_gameEventType}, Server_{m_gameEventName}Event> {{ }}";
        CreateFileFromText(gameEventTriggerAsText, $"Server_{m_gameEventName}EventTrigger", dirPath);
    }

    private void GenerateVoidServerEvent()
    {
        string dirPath = $"{m_targetDir}/{m_gameEventName}/Server";
        
        //Event
        string gameEventAsText = $"using CustomToolkit.Events;\nusing UnityEngine;\n\n[CreateAssetMenu(fileName = \"Server_{m_gameEventName}Event\", menuName = \"Custom/Game Events/Server/{m_gameEventName}Event\")]\npublic class Server_{m_gameEventName}Event : GameEvent {{ }}";
        CreateFileFromText(gameEventAsText, $"Server_{m_gameEventName}Event", dirPath);

        //Event Listener
        string gameEventListenerAsText = $"using CustomToolkit.Events;\n\npublic class Server_{m_gameEventName}EventListener : GameEventListener<Server_{m_gameEventName}Event> {{ }}";
        CreateFileFromText(gameEventListenerAsText, $"Server_{m_gameEventName}EventListener", dirPath);
        
        //Event Trigger
        string gameEventTriggerAsText = $"using CustomToolkit.Events;\n\npublic class Server_{m_gameEventName}EventTrigger : GameEventTrigger<Server_{m_gameEventName}Event> {{ }}";
        CreateFileFromText(gameEventTriggerAsText, $"Server_{m_gameEventName}EventTrigger", dirPath);
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

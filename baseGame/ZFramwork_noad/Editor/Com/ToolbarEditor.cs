using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.SceneManagement;
using System.IO;

[InitializeOnLoad]
public static class ToolbarEditor
{
        private static readonly Type kToolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static ScriptableObject sCurrentToolbar;


        static ToolbarEditor()
        {
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (sCurrentToolbar == null)
            {
                UnityEngine.Object[] toolbars = Resources.FindObjectsOfTypeAll(kToolbarType);
                sCurrentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
                if (sCurrentToolbar != null)
                {
                    FieldInfo root = sCurrentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                    VisualElement concreteRoot = root.GetValue(sCurrentToolbar) as VisualElement;

                    VisualElement toolbarZone = concreteRoot.Q("ToolbarZoneRightAlign");
                    VisualElement parent = new VisualElement()
                    {
                        style = {
                                flexGrow = 1,
                                flexDirection = FlexDirection.Row,
                            }
                    };
                    IMGUIContainer container = new IMGUIContainer();
                    container.onGUIHandler += OnGuiBody;
                    parent.Add(container);
                    toolbarZone.Add(parent);
                }
            }
        }

        private static void OnGuiBody()
        {
            //�Զ��尴ť���ڴ˴�
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("GamePlay", EditorGUIUtility.FindTexture("PlayButton"))))
            {
               OnSceneOpenOrPlay("Assets/uiMain.unity");
             }
            if (GUILayout.Button(new GUIContent("GameResPlay", EditorGUIUtility.FindTexture("PlayButton"))))
            {
               OnSceneOpenOrPlay("Assets/Res.unity");
             }
            GUILayout.EndHorizontal();
        }

    private static void OnSceneOpenOrPlay(string sPath)
    {
        string sSceneName = Path.GetFileNameWithoutExtension(sPath);
        bool bIsCurScene = EditorSceneManager.GetActiveScene().name.Equals(sSceneName);//�Ƿ�Ϊ��ǰ����
        if (!Application.isPlaying)
        {
            if (bIsCurScene)
            {
                Debug.Log($"���г�����{sSceneName}");
                EditorApplication.ExecuteMenuItem("Edit/Play");
            }
            else
            {
                Debug.Log($"�򿪳�����{sSceneName}");
                EditorSceneManager.OpenScene(sPath);
                Debug.Log($"���г�����{sSceneName}");
                EditorApplication.ExecuteMenuItem("Edit/Play");
            }
        }
        else
        {
            if (bIsCurScene)
            {
                Debug.Log($"�˳�������{sSceneName}");
                EditorApplication.ExecuteMenuItem("Edit/Play");
            }
        }
    }

}

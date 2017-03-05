﻿using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Reflection;
using System.Collections.Generic;

namespace SequencedActionCreator
{
    public class SequencedActionEditor : EditorWindow
    {
        private readonly static string m_DefaultSequencedActionName = "SequencedAction";

        private SequencedActionController m_Controller;
        private SerializedObject m_SerializedController;
        private SerializedProperty m_SequencedActionList;

        private int m_SelectedSequencedAction;

        [MenuItem("Window/SequencedAction Editor")]
        static void Init()
        {
            SequencedActionEditor window = (SequencedActionEditor)GetWindow(typeof(SequencedActionEditor));
            window.minSize = new Vector2(600, 600);
            window.Show();
        }

        private void OnEnable()
        {
            // Find the SequencedActionController in the Scene or create a new one if it doesn't exist
            SequencedActionController[] controllers = (SequencedActionController[])Resources.FindObjectsOfTypeAll(typeof(SequencedActionController));
            if (controllers.Length > 0)
                m_Controller = controllers[0];
            else
            {
                GameObject controllerHolder = new GameObject("SequencedActionController");
                m_Controller = controllerHolder.AddComponent<SequencedActionController>() as SequencedActionController;
            }

            m_SerializedController = new SerializedObject(m_Controller);
            m_SequencedActionList = m_SerializedController.FindProperty("m_SequencedActions");
        }

        void OnGUI()
        {
            m_SerializedController.Update();
            GUI.changed = false;

            BuildSequencedActionSelection();
            BuildActions();

            if (GUI.changed)
            {
                // Mark the scene as changed when values are modified
                m_SerializedController.ApplyModifiedProperties();
                EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }
        }

        private void BuildSequencedActionSelection()
        {
            // Get a List of all SequencedActions
            string[] sequencedActions = new string[m_SequencedActionList.arraySize];
            for (int i = 0; i <= m_SequencedActionList.arraySize - 1; i++)
            {
                SerializedProperty action = m_SequencedActionList.GetArrayElementAtIndex(i);
                sequencedActions[i] = action.FindPropertyRelative("m_Name").stringValue;
            }
            GUILayout.BeginHorizontal();
            // Select a SequencedAction element
            m_SelectedSequencedAction = EditorGUILayout.Popup(m_SelectedSequencedAction, sequencedActions, GUILayout.Width(200));
            // Add a new SequencedAction element
            if (GUILayout.Button("Add Sequenced Action", GUILayout.Width(200)))
            {
                m_SequencedActionList.arraySize++;
                m_SelectedSequencedAction = m_SequencedActionList.arraySize - 1;
                // Set a default name for the new element
                m_SequencedActionList.GetArrayElementAtIndex(m_SelectedSequencedAction).FindPropertyRelative("m_Name").stringValue =
                    m_DefaultSequencedActionName + m_SelectedSequencedAction;
            }

            if (m_SequencedActionList.arraySize > m_SelectedSequencedAction)
            {
                // Display the TextField to change the name of the SequencedAction
                SerializedProperty currentSequencedAction = m_SequencedActionList.GetArrayElementAtIndex(m_SelectedSequencedAction);
                currentSequencedAction.FindPropertyRelative("m_Name").stringValue =
                    EditorGUILayout.TextField(currentSequencedAction.FindPropertyRelative("m_Name").stringValue, GUILayout.Width(200));
            }

            GUILayout.EndHorizontal();
        }

        private void BuildActions()
        {
            SerializedProperty actions = m_SequencedActionList.GetArrayElementAtIndex(m_SelectedSequencedAction).FindPropertyRelative("m_ActionEvents");
            for (int i = 0; i < actions.arraySize; i++)
            {
                EditorGUILayout.Separator();
                GUILayout.BeginVertical(EditorStyles.helpBox);
                BuildActionSettings(actions.GetArrayElementAtIndex(i));
                GUILayout.EndVertical();
            }
        }

        private void BuildActionSettings(SerializedProperty action)
        {
            action.FindPropertyRelative("m_GameObject").objectReferenceValue =
                (GameObject)EditorGUILayout.ObjectField((GameObject)action.FindPropertyRelative("m_GameObject").objectReferenceValue, typeof(GameObject), true);

            GameObject c_GameObject = (GameObject)action.FindPropertyRelative("m_GameObject").objectReferenceValue;

            if (c_GameObject != null)
            {
                List<string> methods = new List<string>();
                List<string> mbNames = new List<string>();

                MonoBehaviour monoBehaviour = (MonoBehaviour)action.FindPropertyRelative("m_ScriptObject").objectReferenceValue;
                int s_MonoBehaviour = 0, s_Method = 0;

                // Find all scripts (MonoBehaviours) which are attached to the selected GameObject
                MonoBehaviour[] mbs = c_GameObject.GetComponents<MonoBehaviour>();
                for (int i = 0; i < mbs.Length; i++)
                {
                    MonoBehaviour mb = mbs[i];
                    mbNames.Add(mb.GetType().Name);

                    if (monoBehaviour != null && mb.GetType().Name == monoBehaviour.GetType().Name)
                        s_MonoBehaviour = i;
                }
                int s_MonoBehaviourTemp = EditorGUILayout.Popup(s_MonoBehaviour, mbNames.ToArray());

                // When the MonoBehaviour has been changed, reset the selected method to avoid a nullpointer
                if (s_MonoBehaviour != s_MonoBehaviourTemp)
                {
                    s_Method = 0;
                    s_MonoBehaviour = s_MonoBehaviourTemp;
                }
                if (mbs.Length > s_MonoBehaviour)
                    action.FindPropertyRelative("m_ScriptObject").objectReferenceValue = mbs[s_MonoBehaviour];
                else
                    action.FindPropertyRelative("m_ScriptObject").objectReferenceValue = null;

                // Find all methods in the selected MonoBehaviour
                BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

                if (monoBehaviour != null)
                {
                    MethodInfo[] methodInfos = monoBehaviour.GetType().GetMethods(flags);

                    for (int i = 0; i < methodInfos.Length; i++)
                    {
                        MethodInfo info = methodInfos[i];
                        methods.Add(info.Name);

                        if (info.Name == action.FindPropertyRelative("m_MethodName").stringValue)
                            s_Method = i;
                    }
                }
                s_Method = EditorGUILayout.Popup(s_Method, methods.ToArray());

                if (methods.Count > s_Method)
                    action.FindPropertyRelative("m_MethodName").stringValue = methods[s_Method];
                else
                    action.FindPropertyRelative("m_MethodName").stringValue = null;
            }
        }

    }
}
using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using HHMusicLayer.Editor;

namespace Examples.Editor
{
    [CustomEditor(typeof(CustomSoundCoordinator))]
    public class CustomSoundCoordinatorEditor : SoundCoordinatorEditor
    {

        private CustomSoundCoordinator Coordinator => target as CustomSoundCoordinator;


        #region Editor functionality methods


        private void IncreaseWantedAction()
        {
            var currentWantedLevel = Coordinator.CurrentWantedLevel;
            Coordinator.TransitionToWantedLevel(currentWantedLevel + 1);
        }

        private void DecreaseWantedAction()
        {
            var currentWantedLevel = Coordinator.CurrentWantedLevel;
            Coordinator.TransitionToWantedLevel(currentWantedLevel - 1);
        }

        #endregion

        #region Inspector Drawing

        protected override void DrawCustomSoundGroups()
        {
            var wantedSoundGroups = serializedObject.FindProperty("wantedLevelSoundConfigurations");
            for (int i = 0; i < wantedSoundGroups.arraySize; i++)
            {
                var sg = wantedSoundGroups.GetArrayElementAtIndex(i);
                var wantedLvl = sg.FindPropertyRelative("WantedLevel");
                var soundGroup = sg.FindPropertyRelative("SoundGroup");
                var title = $"Wanted Level: {wantedLvl.intValue}";
                DrawSingleSoundGroup(title, soundGroup);
            }


            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Special Sound Groups", GUI.skin.box, GUILayout.ExpandWidth(true));
            DrawSingleSoundGroup("Intro Music Settings", serializedObject.FindProperty("firstSoundGroup"));
            DrawSingleSoundGroup("Powerup Music Settings", serializedObject.FindProperty("powerupSoundGroup"));
            DrawSingleSoundGroup("GameOver Music Settings", serializedObject.FindProperty("gameoverSoundGroup"));
        }

        protected override void DrawCustomInfo()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Current Wanted Level");
            EditorGUILayout.TextField($"{Coordinator.CurrentWantedLevel}", GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();
        }

        protected override void DrawCustomControls()
        {
            DrawColoredButton("Wanted+", _defaultButtonColor, IncreaseWantedAction, EditorStyles.miniButtonLeft);
            DrawColoredButton("Wanted-", _defaultButtonColor, DecreaseWantedAction, EditorStyles.miniButtonMid);
            DrawColoredButton("Powerup", _blueButtonColor, () => { }, EditorStyles.miniButtonRight);
        }

        #endregion

    }
}
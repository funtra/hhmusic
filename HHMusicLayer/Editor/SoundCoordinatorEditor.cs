using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

namespace HHMusicLayer.Editor
{
    [CustomEditor(typeof(SoundCoordinator), true)]
    public class SoundCoordinatorEditor : UnityEditor.Editor
    {
        protected Color _defaultButtonColor = new Color32(120, 120, 120, 255);
        protected Color _greenButtonColor = new Color32(80, 180, 80, 255);
        protected Color _redButtonColor = new Color32(200, 80, 80, 255);
        protected Color _yellowButtonColor = new Color32(200, 150, 80, 255);
        protected Color _blueButtonColor = new Color32(80, 80, 180, 255);

        protected Color _originalBackgroundColor;

        private SoundCoordinator Coordinator => target as SoundCoordinator;


        private void OnValidate()
        {
            Precompute();
        }

        public override void OnInspectorGUI()
        {

            DrawControls();
            DrawCurrentInfo();

            DrawSettings();
            DrawSoundGroups();

            if (GUILayout.Button("Precompute Metadata (click after changing anything)"))
            {
                Precompute();
            }

            var audioSwitchProperty = serializedObject.FindProperty("audioSourceSwitch");
            EditorGUILayout.PropertyField(audioSwitchProperty);
        }

        #region Editor functionality methods

        private void Precompute()
        {
            Undo.RecordObject(Coordinator, "Precompute Musical Properties");
            Coordinator.PrecomputeSoundsValues();
            serializedObject.ApplyModifiedProperties();
        }


        private void NextTrack() => Coordinator.NextTrack();


        #endregion

        #region Inspector Drawing

        private void DrawSettings()
        {
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Music Configuration", EditorStyles.whiteLargeLabel);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.HelpBox(new GUIContent("These settings should be applicable to all tracks/clips!"));
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            var BPMProperty = serializedObject.FindProperty("BPM");
            EditorGUILayout.PropertyField(BPMProperty);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var beatsPerBarProperty = serializedObject.FindProperty("beatsPerBar");
            EditorGUILayout.PropertyField(beatsPerBarProperty, new GUIContent("Beats Per Bar"));
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        protected virtual void DrawCustomSoundGroups() { }
        protected virtual void DrawCustomControls() { }

        protected virtual void DrawCustomInfo() { }


        private void DrawSoundGroups()
        {
            EditorGUILayout.Space(20);
            EditorGUI.BeginChangeCheck();
            GUILayout.Box("Sound Groups", GUILayout.ExpandWidth(true));
            EditorGUILayout.Space(15);
            DrawCustomSoundGroups();
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawCurrentInfo()
        {
            EditorGUILayout.Space(20);
            GUILayout.Label("Current Info:", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Currently Playing");
            string clipName = $"{(Coordinator.CurrentSource && Coordinator.CurrentSource.clip ? Coordinator.CurrentSource.clip.name : "unknown")}";
            EditorGUILayout.TextField(clipName);
            EditorGUILayout.EndHorizontal();
            //
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("nextStartTime (Next Transition):");
            EditorGUILayout.TextField($"{Coordinator.NextStartTime}", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
            //
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Transition In:");
            EditorGUILayout.TextField($"{Coordinator.NextStartTime - AudioSettings.dspTime}", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
            //
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Toggle", $"{Coordinator.Toggle}");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();
            GUILayout.Label("Static Info:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Bar Length (computed):");
            EditorGUILayout.TextField($"{Coordinator.BarLength}", GUILayout.Width(300));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Beat Length (computed):");
            EditorGUILayout.TextField($"{Coordinator.BeatLength}", GUILayout.Width(300));
            EditorGUILayout.EndHorizontal();

            DrawCustomInfo();
            EditorGUI.EndDisabledGroup();

        }

        private void DrawControls()
        {
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            GUILayout.Space(10);
            GUILayout.Label("Controls", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            var playOrPause = Coordinator.IsPlaying ? Tuple.Create<string, Color, Action>("Stop", _redButtonColor, Coordinator.Stop) : Tuple.Create<string, Color, Action>("Play", _greenButtonColor, Coordinator.Play);
            DrawColoredButton(playOrPause.Item1, playOrPause.Item2, playOrPause.Item3, EditorStyles.miniButtonLeft);
            DrawColoredButton("Next Track", _yellowButtonColor, NextTrack, EditorStyles.miniButtonRight);
            EditorGUILayout.Space(4);
            DrawCustomControls();
            EditorGUILayout.Space(4);
            DrawColoredButton("Reset", _redButtonColor, Coordinator.Reset, EditorStyles.miniButton);
            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }


        protected void DrawSingleSoundGroup(string boxTitle, SerializedProperty property)
        {

            GUILayout.BeginVertical(boxTitle, GUI.skin.box);
            GUILayout.Space(EditorGUIUtility.singleLineHeight + 5);
            if (null == property) return;
            var maxDepth = property.depth + 1;
            EditorGUI.indentLevel++;
            foreach (SerializedProperty child in property)
            {
                if (child.depth > maxDepth) continue;
                EditorGUILayout.PropertyField(child, true);
            }
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();
        }

        protected void DrawColoredButton(string buttonLabel, Color buttonColor, Action action, GUIStyle styles)
        {
            _originalBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = buttonColor;
            if (GUILayout.Button(buttonLabel, styles))
            {
                action.Invoke();
            }
            GUI.backgroundColor = _originalBackgroundColor;
        }

        #endregion

    }
}
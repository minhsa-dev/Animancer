// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor.Previews
{
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Previews/TransitionPreviewWindow
    partial class TransitionPreviewWindow
    {
        /// <summary>[Internal] Custom Inspector for the <see cref="TransitionPreviewWindow"/>.</summary>
        /// <remarks>
        /// <strong>Documentation:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/transitions#previews">
        /// Previews</see>
        /// </remarks>
        [CustomEditor(typeof(TransitionPreviewWindow))]
        internal class Inspector : UnityEditor.Editor
        {
            /************************************************************************************************************************/

            private static readonly string[]
                TabNames = { "Preview", "Settings" };

            private const int
                PreviewTab = 0,
                SettingsTab = 1;

            /************************************************************************************************************************/

            [SerializeField]
            private int _CurrentTab;

            private readonly AnimancerGraphDrawer
                PlayableDrawer = new();

            public TransitionPreviewWindow Target { get; private set; }

            /************************************************************************************************************************/

            public override bool UseDefaultMargins()
                => false;

            /************************************************************************************************************************/

            public override void OnInspectorGUI()
            {
                GUILayout.Space(AnimancerGUI.StandardSpacing * 2);

                Target = (TransitionPreviewWindow)target;

                if (Event.current.type == EventType.MouseDown)
                    Target.ShowTab();

                _CurrentTab = GUILayout.Toolbar(_CurrentTab, TabNames);
                _CurrentTab = Mathf.Clamp(_CurrentTab, 0, TabNames.Length - 1);

                switch (_CurrentTab)
                {
                    case PreviewTab: DoPreviewInspectorGUI(); break;
                    case SettingsTab: TransitionPreviewSettings.DoInspectorGUI(); break;
                    default: GUILayout.Label("Tab index is out of bounds"); break;
                }
            }

            /************************************************************************************************************************/

            private void DoPreviewInspectorGUI()
            {
                if (!Target._TransitionProperty.IsValid())
                {
                    EditorGUILayout.HelpBox("No target property", MessageType.Info, true);
                    Target.DestroyTransitionProperty();
                    return;
                }

                DoTransitionPropertyGUI();
                DoTransitionGUI();
                Target._Animations.DoGUI();

                var animancer = Target._Scene.PreviewObject.Graph;
                if (animancer != null)
                {
                    PlayableDrawer.DoGUI(animancer.Component);
                    AnimancerGUI.SetGuiChanged(animancer.IsGraphPlaying);
                }
            }

            /************************************************************************************************************************/

            /// <summary>The tooltip used for the Close button.</summary>
            public const string CloseTooltip = "Close the Transition Preview Window";

            /// <summary>Draws the target object and path of the <see cref="TransitionProperty"/>.</summary>
            private void DoTransitionPropertyGUI()
            {
                var property = Target._TransitionProperty;
                property.Update();

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.showMixedValue = property.TargetObjects.Length > 1;
                    EditorGUILayout.ObjectField(property.TargetObject, typeof(Object), true);
                    EditorGUI.showMixedValue = false;

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(property.Property.GetFriendlyPath());

                        GUI.enabled = true;

                        using (var label = PooledGUIContent.Acquire("Close", CloseTooltip))
                        {
                            if (GUILayout.Button(label, EditorStyles.miniButton, AnimancerGUI.DontExpandWidth))
                            {
                                Target.Close();
                                GUIUtility.ExitGUI();
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }

            /************************************************************************************************************************/

            private void DoTransitionGUI()
            {
                var property = Target._TransitionProperty;

                var isExpanded = property.Property.isExpanded;
                property.Property.isExpanded = true;
                var height = EditorGUI.GetPropertyHeight(property, true);

                const float Indent = 12;

                var padding = GUI.skin.box.padding;

                var area = AnimancerGUI.LayoutRect(height + padding.horizontal - padding.bottom);
                area.x += Indent + padding.left;
                area.width -= Indent + padding.horizontal;

                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(area, property, true);
                property.Property.isExpanded = isExpanded;
                if (EditorGUI.EndChangeCheck())
                    property.ApplyModifiedProperties();
            }

            /************************************************************************************************************************/
        }
    }
}

#endif


using UnityEditor;
using UnityEngine.UIElements;

namespace Mert.DialogueSystem.Windows
{
    public class DialogueSystemEditorWindow : EditorWindow
    {
        [MenuItem("Window/DialogueSystem/Dialogue Graph")]
        public static void Open()
        {
            GetWindow<DialogueSystemEditorWindow>("Dialogue Graph");
        }

        private void OnEnable()
        {
            AddGraphView();

            AddStyles();
        }

        private void AddGraphView()
        {
            DialogueSystemGraphView graphView = new DialogueSystemGraphView();
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void AddStyles()
        {
            StyleSheet styleSheet = EditorGUIUtility.Load("DialogueSystem/DialogueSystemVariables.uss") as StyleSheet;

            rootVisualElement.styleSheets.Add(styleSheet);
        }
    }
}

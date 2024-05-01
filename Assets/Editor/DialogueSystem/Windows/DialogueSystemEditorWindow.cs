using UnityEditor;
using UnityEngine.UIElements;

namespace Mert.DialogueSystem.Windows
{
    using Utilities;
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

        #region Elements Insertion
        private void AddGraphView()
        {
            DialogueSystemGraphView graphView = new DialogueSystemGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DialogueSystem/DialogueSystemVariables.uss");
        }
        #endregion
    }
}

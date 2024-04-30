using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mert.DialogueSystem.Windows
{
    public class DialogueSystemEditor : EditorWindow
    {
        [MenuItem("Window/DialogueSystem/DialogueSystemEditor")]
        public static void ShowExample()
        {
            DialogueSystemEditor wnd = GetWindow<DialogueSystemEditor>();
            wnd.titleContent = new GUIContent("DialogueSystemEditor");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // VisualElements objects can contain other VisualElement following a tree hierarchy.
            VisualElement label = new Label("Mert");
            root.Add(label);

        }
    }
}
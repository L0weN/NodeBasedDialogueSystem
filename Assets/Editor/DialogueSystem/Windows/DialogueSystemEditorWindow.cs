using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Mert.DialogueSystem.Windows
{
    using Utilities;
    public class DialogueSystemEditorWindow : EditorWindow
    {
        private readonly string defaultFileName = "DialoguesFileName";
        private TextField fileNameTextField;
        private Button saveButton;

        [MenuItem("Window/DialogueSystem/Dialogue Graph")]
        public static void Open()
        {
            GetWindow<DialogueSystemEditorWindow>("Dialogue Graph");
        }

        private void OnEnable()
        {
            AddGraphView();
            AddToolBar();

            AddStyles();
        }

        #region Elements Insertion
        private void AddGraphView()
        {
            DialogueSystemGraphView graphView = new DialogueSystemGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void AddToolBar()
        {
            Toolbar toolbar = new Toolbar();

            fileNameTextField = DialogueSystemElementUtility.CreateTextField(defaultFileName, "File Name:", callback =>
            {
                fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });

            saveButton = DialogueSystemElementUtility.CreateButton("Save");

            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);

            toolbar.AddStyleSheets("DialogueSystem/DialogueSystemToolbarSS.uss");

            rootVisualElement.Add(toolbar);
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DialogueSystem/DialogueSystemVariables.uss");
        }
        #endregion

        #region Utility Methods
        public void EnableSaving()
        {
            saveButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            saveButton.SetEnabled(false);
        }
        #endregion
    }
}

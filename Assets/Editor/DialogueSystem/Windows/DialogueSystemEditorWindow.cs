using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Mert.DialogueSystem.Windows
{
    using System;
    using Utilities;
    public class DialogueSystemEditorWindow : EditorWindow
    {
        private DialogueSystemGraphView graphView;
        private readonly string defaultFileName = "DialoguesFileName";
        private static TextField fileNameTextField;
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
            graphView = new DialogueSystemGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void AddToolBar()
        {
            Toolbar toolbar = new Toolbar();

            fileNameTextField = ElementUtility.CreateTextField(defaultFileName, "File Name:", callback =>
            {
                fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });

            saveButton = ElementUtility.CreateButton("Save", () => Save());

            Button clearButton = ElementUtility.CreateButton("Clear", () => Clear());
            Button resetButton = ElementUtility.CreateButton("Reset", () => Reset());

            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);
            toolbar.Add(clearButton);
            toolbar.Add(resetButton);

            toolbar.AddStyleSheets("DialogueSystem/DialogueSystemToolbarSS.uss");

            rootVisualElement.Add(toolbar);
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DialogueSystem/DialogueSystemVariablesSS.uss");
        }
        #endregion

        #region Toolbar Actions
        private void Save()
        {
            if (string.IsNullOrEmpty(fileNameTextField.value))
            {
                EditorUtility.DisplayDialog(
                    "Invalid File Name",
                    "Please enter a valid file name.",
                    "OK"
                );
            }

            IOUtility.Initialize(graphView, fileNameTextField.value);
            IOUtility.Save();
        }

        private void Clear()
        {
            graphView.ClearGraph();
        }

        private void Reset()
        {
            Clear();

            UpdateFileName(defaultFileName);
        }
        #endregion

        #region Utility Methods
        public static void UpdateFileName(string newFileName)
        {
            fileNameTextField.value = newFileName;
        }

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

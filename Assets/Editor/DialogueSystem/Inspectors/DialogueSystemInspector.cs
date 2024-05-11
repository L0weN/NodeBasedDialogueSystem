using UnityEditor;
using System.Collections.Generic;

namespace Mert.DialogueSystem.Inspectors
{
    using ScriptableObjects;
    using Utilities;
    [CustomEditor(typeof(Dialogue))]
    public class DialogueSystemInspector : Editor
    {
        private SerializedProperty dialogueContainerProperty;
        private SerializedProperty dialogueGroupProperty;
        private SerializedProperty dialogueProperty;

        private SerializedProperty groupedDialoguesProperty;
        private SerializedProperty startingDialoguesOnlyProperty;

        private SerializedProperty selectedDialogueGroupIndexProperty;
        private SerializedProperty selectedDialogueIndexProperty;

        private void OnEnable()
        {
            dialogueContainerProperty = serializedObject.FindProperty("dialogueContainer");
            dialogueGroupProperty = serializedObject.FindProperty("dialogueGroup");
            dialogueProperty = serializedObject.FindProperty("dialogue");

            groupedDialoguesProperty = serializedObject.FindProperty("groupedDialogues");
            startingDialoguesOnlyProperty = serializedObject.FindProperty("startingDialoguesOnly");

            selectedDialogueGroupIndexProperty = serializedObject.FindProperty("selectedDialogueGroupIndex");
            selectedDialogueIndexProperty = serializedObject.FindProperty("selectedDialogueIndex");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDialogueContainerArea();

            DialogueContainerSO dialogueContainer = dialogueContainerProperty.objectReferenceValue as DialogueContainerSO;

            if (dialogueContainer == null)
            {
                StopDrawing("Select a dialogue container!");
                return;
            }

            DrawFiltersArea();

            bool currentStartingDialoguesOnly = startingDialoguesOnlyProperty.boolValue;

            List<string> dialogueNames;

            string dialogueFolderPath = $"Assets/DialogueSystem/Dialogues/{dialogueContainer.FileName}";
            string dialogueInfoMassage;

            if (groupedDialoguesProperty.boolValue)
            {
                List<string> dialogueGroupNames = dialogueContainer.GetDialogueGroupNames();

                if (dialogueGroupNames.Count == 0)
                {
                    StopDrawing("There are no dialogue groups in the container!");

                    return;
                }

                DrawDialogueGroupArea(dialogueContainer, dialogueGroupNames);

                DialogueGroupSO dialogueGroup = dialogueGroupProperty.objectReferenceValue as DialogueGroupSO;

                dialogueNames = dialogueContainer.GetGroupedDialogueNames(dialogueGroup, currentStartingDialoguesOnly);

                dialogueFolderPath += $"/Groups/{dialogueGroup.GroupName}/Dialogues";

                dialogueInfoMassage = "There are no" + (currentStartingDialoguesOnly ? " Starting" : "") + "dialogues in the selected group!";
            }
            else
            {
                dialogueNames = dialogueContainer.GetUngroupedDialogueNames(currentStartingDialoguesOnly);

                dialogueFolderPath += "/Global/Dialogues";

                dialogueInfoMassage = "There are no" + (currentStartingDialoguesOnly ? " Starting" : "") + "ungrouped dialogues in the container!";
            }

            if (dialogueNames.Count == 0)
            {
                StopDrawing(dialogueInfoMassage);
                return;
            }

            DrawDialogueArea(dialogueNames, dialogueFolderPath);

            serializedObject.ApplyModifiedProperties();
        }

        #region Draw Methods
        private void DrawDialogueContainerArea()
        {
            InspectorUtility.DrawHeader("Dialogue Container");

            dialogueContainerProperty.DrawPropertyField();

            InspectorUtility.DrawSpace();
        }

        private void DrawFiltersArea()
        {
            InspectorUtility.DrawHeader("Filters");

            groupedDialoguesProperty.DrawPropertyField();
            startingDialoguesOnlyProperty.DrawPropertyField();

            InspectorUtility.DrawSpace();
        }

        private void DrawDialogueGroupArea(DialogueContainerSO dialogueContainer, List<string> dialogueGroupNames)
        {
            InspectorUtility.DrawHeader("Dialogue Group");

            int oldSelectedDialogueGroupIndex = selectedDialogueGroupIndexProperty.intValue;

            DialogueGroupSO oldDialogueGroup = dialogueGroupProperty.objectReferenceValue as DialogueGroupSO;

            bool isOldDialogueGroupNull = oldDialogueGroup == null;

            string oldDialogueGroupName = isOldDialogueGroupNull ? "" : oldDialogueGroup.GroupName;

            UpdateIndexOnNamesListUpdate(dialogueGroupNames, selectedDialogueGroupIndexProperty, oldSelectedDialogueGroupIndex, oldDialogueGroupName, isOldDialogueGroupNull);

            selectedDialogueGroupIndexProperty.intValue = InspectorUtility.DrawPopup("Dialogue Group", selectedDialogueGroupIndexProperty.intValue, dialogueGroupNames.ToArray());
            string selectedDialogueGroupName = dialogueGroupNames[selectedDialogueGroupIndexProperty.intValue];

            DialogueGroupSO selectedDialogueGroup = IOUtility.LoadAsset<DialogueGroupSO>($"Assets/DialogueSystem/Dialogues/{dialogueContainer.FileName}/Groups/{selectedDialogueGroupName}", selectedDialogueGroupName);
            dialogueGroupProperty.objectReferenceValue = selectedDialogueGroup;

            InspectorUtility.DrawDisabledFields(() => dialogueGroupProperty.DrawPropertyField());

            InspectorUtility.DrawSpace();
        }

        private void DrawDialogueArea(List<string> dialogueNames, string dialogueFolderPath)
        {
            InspectorUtility.DrawHeader("Dialogue");

            int oldSelectedDialogueIndex = selectedDialogueIndexProperty.intValue;

            DialogueSO oldDialogue = dialogueProperty.objectReferenceValue as DialogueSO;

            bool isOldDialogueNull = oldDialogue == null;

            string oldDialogueName = isOldDialogueNull ? "" : oldDialogue.DialogueName;

            UpdateIndexOnNamesListUpdate(dialogueNames, selectedDialogueIndexProperty, oldSelectedDialogueIndex, oldDialogueName, isOldDialogueNull);

            selectedDialogueIndexProperty.intValue = InspectorUtility.DrawPopup("Dialogue", selectedDialogueIndexProperty.intValue, new string[] { });

            string selectedDialogueName = dialogueNames[selectedDialogueIndexProperty.intValue];

            DialogueSO selectedDialogue = IOUtility.LoadAsset<DialogueSO>(dialogueFolderPath, selectedDialogueName);

            dialogueProperty.objectReferenceValue = selectedDialogue;

            InspectorUtility.DrawDisabledFields(() => dialogueProperty.DrawPropertyField());
        }

        private void StopDrawing(string reason, MessageType messageType = MessageType.Info)
        {
            InspectorUtility.DrawHelpBox(reason, messageType);

            InspectorUtility.DrawSpace();

            InspectorUtility.DrawHelpBox("Please select a dialogue container first!", MessageType.Warning);

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region  Index Methods
        private void UpdateIndexOnNamesListUpdate(List<string> optionNames, SerializedProperty indexProperty, int oldSelectedPropertyIndex, string oldPropertyName, bool isOldPropertyNull)
        {
            if (isOldPropertyNull)
            {
                indexProperty.intValue = 0;

                return;
            }

            bool oldIndexIsOutOfBoundsOfNamesListCount = oldSelectedPropertyIndex > optionNames.Count - 1;
            bool oldNameIsDifferentThanSelectedName = oldIndexIsOutOfBoundsOfNamesListCount || oldPropertyName != optionNames[oldSelectedPropertyIndex];

            if (oldNameIsDifferentThanSelectedName)
            {
                if (optionNames.Contains(oldPropertyName))
                {
                    indexProperty.intValue = optionNames.IndexOf(oldPropertyName);
                }
                else
                {
                    indexProperty.intValue = 0;
                }
            }
        }
        #endregion
    }
}

using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace Mert.DialogueSystem.Windows
{
    using Elements;
    using Enumerations;

    public class DialogueSystemGraphView : GraphView
    {
        public DialogueSystemGraphView()
        {
            AddManipulators();
            AddGridBackground();
            AddStyles();
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu("Add Node(Single Choice)", DialogueType.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Add Node(Multiple Choice)", DialogueType.MultipleChoice));
        }

        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddStyles()
        {
            StyleSheet graphViewStyleSheet = EditorGUIUtility.Load("DialogueSystem/DialogueSystemGraphViewSS.uss") as StyleSheet;
            StyleSheet nodeStyleSheet = EditorGUIUtility.Load("DialogueSystem/DialogueSystemNodeSS.uss") as StyleSheet;
            styleSheets.Add(graphViewStyleSheet);
            styleSheets.Add(nodeStyleSheet);
        }

        private DialogueSystemNode CreateNode(DialogueType dialogueType, Vector2 position)
        {
            Type nodeType = Type.GetType($"Mert.DialogueSystem.Elements.{dialogueType}Node");

            DialogueSystemNode node = Activator.CreateInstance(nodeType) as DialogueSystemNode;

            node.Initialize(position);
            node.Draw();

            return node;
        }

        private IManipulator CreateNodeContextualMenu(string actionTitle, DialogueType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, menuActionEvent => AddElement(CreateNode(dialogueType, menuActionEvent.eventInfo.localMousePosition)))
                );

            return contextualMenuManipulator;
        }
    }
}

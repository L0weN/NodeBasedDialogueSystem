using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;


namespace Mert.DialogueSystem.Windows
{
    using Elements;
    using Enumerations;
    using Utilities;

    public class DialogueSystemGraphView : GraphView
    {
        private DialogueSystemEditorWindow editorWindow;
        private DialogueSystemSearchWindow searchWindow;

        public DialogueSystemGraphView(DialogueSystemEditorWindow dialogueSystemEditorWindow)
        {
            editorWindow = dialogueSystemEditorWindow;
            AddManipulators();
            AddGridBackground();
            AddSearchWindow();
            AddStyles();
        }

        #region Override Methods
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort == port) { return; }
                if (startPort.node == port.node) { return; }
                if (startPort.direction == port.direction) { return; }
                compatiblePorts.Add(port);
            });
            return compatiblePorts;
        }
        #endregion

        #region Manipulators
        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu("Add Node(Single Choice)", DialogueType.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Add Node(Multiple Choice)", DialogueType.MultipleChoice));

            this.AddManipulator(CreateGroupContextualMenu());
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", menuActionEvent => AddElement(CreateGroup("DialogueGroup", GetLocalMousePosition(menuActionEvent.eventInfo.localMousePosition))))
                );

            return contextualMenuManipulator;
        }
        private IManipulator CreateNodeContextualMenu(string actionTitle, DialogueType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, menuActionEvent => AddElement(CreateNode(dialogueType, GetLocalMousePosition(menuActionEvent.eventInfo.localMousePosition))))
                );

            return contextualMenuManipulator;
        }
        #endregion
        
        #region Element Creation
        public DialogueSystemNode CreateNode(DialogueType dialogueType, Vector2 position)
        {
            Type nodeType = Type.GetType($"Mert.DialogueSystem.Elements.{dialogueType}Node");

            DialogueSystemNode node = Activator.CreateInstance(nodeType) as DialogueSystemNode;

            node.Initialize(position);
            node.Draw();

            return node;
        }

        public Group CreateGroup(string title, Vector2 position)
        {
            Group group = new Group()
            {
                title = title,
            };

            group.SetPosition(new Rect(position, Vector2.zero));

            return group;
        }
        #endregion

        #region Element Insertion
        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddSearchWindow()
        {
            if (searchWindow == null)
            {
                searchWindow = ScriptableObject.CreateInstance<DialogueSystemSearchWindow>();

                searchWindow.Initialize(this);
            }

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        private void AddStyles()
        {
            this.AddStyleSheets(
                "DialogueSystem/DialogueSystemGraphViewSS.uss",
                "DialogueSystem/DialogueSystemNodeSS.uss"
                );
        }
        #endregion

        #region Utility Methods
        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                worldMousePosition -= editorWindow.position.position;
            }

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);

            return localMousePosition;
        }
        #endregion
    }
}

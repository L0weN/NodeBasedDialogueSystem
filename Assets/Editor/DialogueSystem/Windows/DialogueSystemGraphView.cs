using System;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;


namespace Mert.DialogueSystem.Windows
{
    using Data.Error;
    using Elements;
    using Enumerations;
    using Utilities;

    public class DialogueSystemGraphView : GraphView
    {
        private DialogueSystemEditorWindow editorWindow;
        private DialogueSystemSearchWindow searchWindow;

        private SerializableDictionary<string, DialogueSystemNodeErrorData> ungroupedNodes;
        private SerializableDictionary<string, DialogueSystemGroupErrorData> groups;
        private SerializableDictionary<Group, SerializableDictionary<string, DialogueSystemNodeErrorData>> groupedNodes;

        private int repeatedNamesAmount = 0;

        public int RepeatedNamesAmout
        {
            get
            {
                return repeatedNamesAmount;
            }
            set
            {
                repeatedNamesAmount = value;

                if (repeatedNamesAmount == 0)
                {
                    editorWindow.EnableSaving();
                }

                if (repeatedNamesAmount == 1)
                {
                    editorWindow.DisableSaving();
                }
            }
        }

        public DialogueSystemGraphView(DialogueSystemEditorWindow dialogueSystemEditorWindow)
        {
            editorWindow = dialogueSystemEditorWindow;

            ungroupedNodes = new SerializableDictionary<string, DialogueSystemNodeErrorData>();
            groups = new SerializableDictionary<string, DialogueSystemGroupErrorData>();
            groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, DialogueSystemNodeErrorData>>();

            AddManipulators();
            AddGridBackground();
            AddSearchWindow();

            OnElementsDeleted();
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();

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
                menuEvent => menuEvent.menu.AppendAction("Add Group", menuActionEvent => CreateGroup("DialogueGroup", GetLocalMousePosition(menuActionEvent.eventInfo.localMousePosition)))
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
        
        #region Elements Creation
        public DialogueSystemNode CreateNode(DialogueType dialogueType, Vector2 position)
        {
            Type nodeType = Type.GetType($"Mert.DialogueSystem.Elements.{dialogueType}Node");

            DialogueSystemNode node = Activator.CreateInstance(nodeType) as DialogueSystemNode;

            node.Initialize(this, position);
            node.Draw();

            AddUngroupedNode(node);

            return node;
        }

        public DialogueSystemGroup CreateGroup(string title, Vector2 position)
        {
            DialogueSystemGroup group = new DialogueSystemGroup(title, position);

            AddGroup(group);

            AddElement(group);

            foreach (GraphElement selectedElement in selection)
            {
                if (!(selectedElement is DialogueSystemNode))
                {
                    continue;
                }

                DialogueSystemNode node = selectedElement as DialogueSystemNode;

                group.AddElement(node);
            }

            return group;
        }
        #endregion

        #region Callbacks
        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                Type groupType = typeof(DialogueSystemGroup);
                Type edgeType = typeof(Edge);

                List<DialogueSystemGroup> groupsToDelete = new List<DialogueSystemGroup>();
                List<Edge> edgesToDelete = new List<Edge>();
                List<DialogueSystemNode> nodesToDelete = new List<DialogueSystemNode>();

                foreach (GraphElement element in selection)
                {
                    if (element is DialogueSystemNode node)
                    {
                        nodesToDelete.Add(node);

                        continue;
                    }

                    if (element.GetType() == edgeType)
                    {
                        Edge edge = element as Edge;

                        edgesToDelete.Add(edge);

                        continue;
                    }

                    if (element.GetType() != groupType)
                    {
                        continue;
                    }

                    DialogueSystemGroup group = element as DialogueSystemGroup;

                    groupsToDelete.Add(group);
                }

                foreach (DialogueSystemGroup group in groupsToDelete)
                {
                    List<DialogueSystemNode> groupNodes = new List<DialogueSystemNode>();
                    foreach (GraphElement groupElement in group.containedElements)
                    {
                        if (!(groupElement is DialogueSystemNode))
                        {
                            continue;
                        }

                        DialogueSystemNode groupNode = groupElement as DialogueSystemNode;

                        groupNodes.Add(groupNode);
                    }

                    group.RemoveElements(groupNodes);

                    RemoveGroup(group);

                    RemoveElement(group);
                }

                DeleteElements(edgesToDelete);

                foreach (DialogueSystemNode node in nodesToDelete)
                {
                    if (node.Group != null)
                    {
                        node.Group.RemoveElement(node);
                    }
                    RemoveUngroupedNode(node);

                    node.DisconnectAllPorts();

                    RemoveElement(node);
                }
            };
        }

        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DialogueSystemNode))
                    {
                        continue;
                    }

                    DialogueSystemGroup nodeGroup = group as DialogueSystemGroup;
                    DialogueSystemNode node = element as DialogueSystemNode;

                    RemoveUngroupedNode(node);
                    AddGroupedNode(node, nodeGroup);
                }
            };
        }

        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DialogueSystemNode))
                    {
                        continue;
                    }

                    DialogueSystemGroup nodeGroup = group as DialogueSystemGroup;
                    DialogueSystemNode node = element as DialogueSystemNode;

                    RemoveGroupedNode(node, nodeGroup);

                    AddUngroupedNode(node);
                }
            };
        }

        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                DialogueSystemGroup dialogueSystemGroup = group as DialogueSystemGroup;

                dialogueSystemGroup.title = newTitle.RemoveWhitespaces().RemoveSpecialCharacters();

                RemoveGroup(dialogueSystemGroup);

                dialogueSystemGroup.oldTitle = dialogueSystemGroup.title;

                AddGroup(dialogueSystemGroup);
            };
        }
        #endregion

        #region Repeated Elements
        public void AddUngroupedNode(DialogueSystemNode node)
        {
            string nodeName = node.DialogueName.ToLower();

            if (!ungroupedNodes.ContainsKey(nodeName))
            {
                DialogueSystemNodeErrorData nodeErrorData = new DialogueSystemNodeErrorData();

                nodeErrorData.Nodes.Add(node);

                ungroupedNodes.Add(nodeName, nodeErrorData);

                return;
            }

            List<DialogueSystemNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;

            ungroupedNodesList.Add(node);

            Color errorColor = ungroupedNodes[nodeName].ErrorData.Color;

            node.SetErrorStyle(errorColor);

            if (ungroupedNodesList.Count == 2)
            {
                ++RepeatedNamesAmout;
                ungroupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveUngroupedNode(DialogueSystemNode node)
        {
            string nodeName = node.DialogueName.ToLower();

            List<DialogueSystemNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;

            ungroupedNodesList.Remove(node);

            node.ResetStyle();

            if (ungroupedNodesList.Count == 1)
            {
                --RepeatedNamesAmout;
                ungroupedNodesList[0].ResetStyle();

                return;
            }

            if (ungroupedNodesList.Count == 0)
            {
                ungroupedNodes.Remove(nodeName);
            }
        }

        public void AddGroupedNode(DialogueSystemNode node, DialogueSystemGroup group)
        {
            string nodeName = node.DialogueName.ToLower();

            node.Group = group;

            if (!groupedNodes.ContainsKey(group))
            {
                groupedNodes.Add(group, new SerializableDictionary<string, DialogueSystemNodeErrorData>());
            }

            if (!groupedNodes[group].ContainsKey(nodeName))
            {
                DialogueSystemNodeErrorData nodeErrorData = new DialogueSystemNodeErrorData();

                nodeErrorData.Nodes.Add(node);

                groupedNodes[group].Add(nodeName, nodeErrorData);

                return;
            }

            List<DialogueSystemNode> groupedNodeList = groupedNodes[group][nodeName].Nodes;

            groupedNodeList.Add(node);

            Color errorColor = groupedNodes[group][nodeName].ErrorData.Color;

            node.SetErrorStyle(errorColor);

            if (groupedNodeList.Count == 2)
            {
                ++RepeatedNamesAmout;
                groupedNodeList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveGroupedNode(DialogueSystemNode node, DialogueSystemGroup group)
        {
            string nodeName = node.DialogueName.ToLower();

            node.Group = null;

            List<DialogueSystemNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            groupedNodesList.Remove(node);

            node.ResetStyle();

            if (groupedNodesList.Count == 1)
            {
                --RepeatedNamesAmout;
                groupedNodesList[0].ResetStyle();

                return;
            }

            if (groupedNodesList.Count == 0)
            {
                groupedNodes[group].Remove(nodeName);

                if (groupedNodes[group].Count == 0)
                {
                    groupedNodes.Remove(group);
                }
            }
        }

        public void AddGroup(DialogueSystemGroup group)
        {
            string groupName = group.title.ToLower();

            if (!groups.ContainsKey(groupName))
            {
                DialogueSystemGroupErrorData groupErrorData = new DialogueSystemGroupErrorData();

                groupErrorData.Groups.Add(group);

                groups.Add(groupName, groupErrorData);

                return;
            }

            List<DialogueSystemGroup> groupsList = groups[groupName].Groups;

            groupsList.Add(group);

            Color errorColor = groups[groupName].ErrorData.Color;

            group.SetErrorStyle(errorColor);

            if (groupsList.Count == 2)
            {
                ++RepeatedNamesAmout;
                groupsList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveGroup(DialogueSystemGroup group)
        {
            string oldGroupName = group.oldTitle.ToLower();

            List<DialogueSystemGroup> groupsList = groups[oldGroupName].Groups;

            groupsList.Remove(group);

            group.ResetStyle();

            if (groupsList.Count == 1)
            {
                --RepeatedNamesAmout;
                groupsList[0].ResetStyle();

                return;
            }

            if (groupsList.Count == 0)
            {
                groups.Remove(oldGroupName);
            }
        }
        #endregion

        #region Elements Insertion
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
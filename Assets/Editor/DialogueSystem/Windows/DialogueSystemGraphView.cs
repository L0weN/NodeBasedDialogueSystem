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
    using Data.Save;
    using Utilities;

    public class DialogueSystemGraphView : GraphView
    {
        private DialogueSystemEditorWindow editorWindow;
        private DialogueSystemSearchWindow searchWindow;

        private MiniMap miniMap;

        private SerializableDictionary<string, NodeErrorData> ungroupedNodes;
        private SerializableDictionary<string, GroupErrorData> groups;
        private SerializableDictionary<Group, SerializableDictionary<string, NodeErrorData>> groupedNodes;

        private int nameErrorsAmount = 0;

        public int NameErrorsAmount
        {
            get
            {
                return nameErrorsAmount;
            }
            set
            {
                nameErrorsAmount = value;

                if (nameErrorsAmount == 0)
                {
                    editorWindow.EnableSaving();
                }

                if (nameErrorsAmount == 1)
                {
                    editorWindow.DisableSaving();
                }
            }
        }

        public DialogueSystemGraphView(DialogueSystemEditorWindow dialogueSystemEditorWindow)
        {
            editorWindow = dialogueSystemEditorWindow;

            ungroupedNodes = new SerializableDictionary<string, NodeErrorData>();
            groups = new SerializableDictionary<string, GroupErrorData>();
            groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, NodeErrorData>>();

            AddManipulators();
            AddGridBackground();
            AddMiniMap();
            AddSearchWindow();

            OnElementsDeleted();
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();
            OnGraphViewChanged();

            AddStyles();
            AddMiniMapStyles();
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
                menuEvent => menuEvent.menu.AppendAction(actionTitle, menuActionEvent => AddElement(CreateNode("DialogueName", dialogueType, GetLocalMousePosition(menuActionEvent.eventInfo.localMousePosition))))
                );

            return contextualMenuManipulator;
        }
        #endregion
        
        #region Elements Creation
        public DialogueSystemNode CreateNode(string nodeName, DialogueType dialogueType, Vector2 position, bool shouldDraw = true)
        {
            Type nodeType = Type.GetType($"Mert.DialogueSystem.Elements.{dialogueType}Node");

            DialogueSystemNode node = Activator.CreateInstance(nodeType) as DialogueSystemNode;

            node.Initialize(nodeName, this, position);

            if (shouldDraw)
            {
                node.Draw();
            }
            
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

                if (string.IsNullOrEmpty(dialogueSystemGroup.title))
                {
                    if (!string.IsNullOrEmpty(dialogueSystemGroup.OldTitle))
                    {
                        ++NameErrorsAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(dialogueSystemGroup.OldTitle))
                    {
                        --NameErrorsAmount;
                    }
                }

                RemoveGroup(dialogueSystemGroup);

                dialogueSystemGroup.OldTitle = dialogueSystemGroup.title;

                AddGroup(dialogueSystemGroup);
            };
        }

        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        DialogueSystemNode nextNode = edge.input.node as DialogueSystemNode;

                        ChoiceSaveData choiceData = edge.output.userData as ChoiceSaveData;

                        choiceData.NodeID = nextNode.ID;
                    }
                }

                if (changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);
                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element.GetType() != edgeType)
                        {
                            continue;
                        }

                        Edge edge = element as Edge;

                        ChoiceSaveData choiceData = edge.output.userData as ChoiceSaveData;

                        choiceData.NodeID = "";
                    }
                }

                return changes;
            };
        }
        #endregion

        #region Repeated Elements
        public void AddUngroupedNode(DialogueSystemNode node)
        {
            string nodeName = node.DialogueName.ToLower();

            if (!ungroupedNodes.ContainsKey(nodeName))
            {
                NodeErrorData nodeErrorData = new NodeErrorData();

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
                ++NameErrorsAmount;
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
                --NameErrorsAmount;
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
                groupedNodes.Add(group, new SerializableDictionary<string, NodeErrorData>());
            }

            if (!groupedNodes[group].ContainsKey(nodeName))
            {
                NodeErrorData nodeErrorData = new NodeErrorData();

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
                ++NameErrorsAmount;
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
                --NameErrorsAmount;
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
                GroupErrorData groupErrorData = new GroupErrorData();

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
                ++NameErrorsAmount;
                groupsList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveGroup(DialogueSystemGroup group)
        {
            string oldGroupName = group.OldTitle.ToLower();

            List<DialogueSystemGroup> groupsList = groups[oldGroupName].Groups;

            groupsList.Remove(group);

            group.ResetStyle();

            if (groupsList.Count == 1)
            {
                --NameErrorsAmount;
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

        private void AddMiniMap()
        {
            miniMap = new MiniMap { anchored = true };

            miniMap.SetPosition(new Rect(15, 50, 200, 180));
            Add(miniMap);

            miniMap.visible = false;
        }

        private void AddStyles()
        {
            this.AddStyleSheets(
                "DialogueSystem/DialogueSystemGraphViewSS.uss",
                "DialogueSystem/DialogueSystemNodeSS.uss"
                );
        }

        private void AddMiniMapStyles()
        {
            StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));

            miniMap.style.backgroundColor = backgroundColor;
            miniMap.style.borderTopColor = borderColor;
            miniMap.style.borderRightColor = borderColor;
            miniMap.style.borderBottomColor = borderColor;
            miniMap.style.borderLeftColor = borderColor;
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

        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));

            groups.Clear();
            groupedNodes.Clear();
            ungroupedNodes.Clear();

            NameErrorsAmount = 0;
        }

        public void ToggleMiniMap()
        {
            miniMap.visible = !miniMap.visible;
        }
        #endregion
    }
}
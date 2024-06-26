using System;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Mert.DialogueSystem.Utilities
{
    using Windows;
    using Elements;
    using Data.Save;
    using ScriptableObjects;
    using Data;
    using UnityEditor.Experimental.GraphView;

    public static class IOUtility
    {
        private static DialogueSystemGraphView graphView;

        private static string graphFileName;
        private static string containerFolderPath;

        private static List<DialogueSystemGroup> groups;
        private static List<DialogueSystemNode> nodes;

        private static Dictionary<string, DialogueGroupSO> createdDialogueGroups;
        private static Dictionary<string, DialogueSO> createdDialogues;

        private static Dictionary<string, DialogueSystemGroup> loadedGroups;
        private static Dictionary<string, DialogueSystemNode> loadedNodes;

        public static void Initialize(DialogueSystemGraphView dialogueSystemGraphView, string graphName)
        {
            graphView = dialogueSystemGraphView;
            graphFileName = graphName;

            containerFolderPath = $"Assets/DialogueSystem/Dialogues/{graphFileName}";

            groups = new List<DialogueSystemGroup>();
            nodes = new List<DialogueSystemNode>();

            createdDialogueGroups = new Dictionary<string, DialogueGroupSO>();
            createdDialogues = new Dictionary<string, DialogueSO>();

            loadedGroups = new Dictionary<string, DialogueSystemGroup>();
            loadedNodes = new Dictionary<string, DialogueSystemNode>();
        }

        #region Save Methods
        public static void Save()
        {
            CreateStaticFolders();

            GetElementsFromGraphView();

            GraphSaveDataSO graphData = CreateAsset<GraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", $"{graphFileName}Graph");

            graphData.Initialize(graphFileName);

            DialogueContainerSO dialogueContainer = CreateAsset<DialogueContainerSO>(containerFolderPath, graphFileName);

            dialogueContainer.Initialize(graphFileName);

            SaveGroups(graphData, dialogueContainer);
            SaveNodes(graphData, dialogueContainer);

            SaveAsset(graphData);
            SaveAsset(dialogueContainer);
        }

        #region Groups
        private static void SaveGroups(GraphSaveDataSO graphData, DialogueContainerSO dialogueContainer)
        {
            List<string> groupNames = new List<string>();
            foreach (DialogueSystemGroup group in groups)
            {
                SaveGroupToGraph(group, graphData);
                SaveGroupToScriptableObject(group, dialogueContainer);

                groupNames.Add(group.title);
            }

            UpdateOldGroups(groupNames, graphData);
        }

        private static void UpdateOldGroups(List<string> currentGroupNames, GraphSaveDataSO graphData)
        {
            if (graphData.OldGroupNames != null && graphData.OldGroupNames.Count != 0)
            {
                List<string> groupsToRemove = graphData.OldGroupNames.Except(currentGroupNames).ToList();

                foreach (string groupToRemove in groupsToRemove)
                {
                    RemoveFolder($"{containerFolderPath}/Groups/{groupToRemove}");
                }
            }

            graphData.OldGroupNames = new List<string>(currentGroupNames);
        }

        private static void SaveGroupToScriptableObject(DialogueSystemGroup group, DialogueContainerSO dialogueContainer)
        {
            string groupName = group.title;

            CreateFolder($"{containerFolderPath}/Groups", groupName);
            CreateFolder($"{containerFolderPath}/Groups/{groupName}", "Dialogues");

            DialogueGroupSO dialogueGroup = CreateAsset<DialogueGroupSO>($"{containerFolderPath}/Groups/{groupName}", groupName);
            dialogueGroup.Initialize(groupName);

            createdDialogueGroups.Add(group.ID, dialogueGroup);

            dialogueContainer.DialogueGroups.Add(dialogueGroup, new List<DialogueSO>());

            SaveAsset(dialogueGroup);
        }

        private static void SaveGroupToGraph(DialogueSystemGroup group, GraphSaveDataSO graphData)
        {
            GroupSaveData groupData = new GroupSaveData()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            };

            graphData.Groups.Add(groupData);
        }
        #endregion

        #region Nodes
        private static void SaveNodes(GraphSaveDataSO graphData, DialogueContainerSO dialogueContainer)
        {
            SerializableDictionary<string, List<string>> groupedNodeNames = new SerializableDictionary<string, List<string>>();
            List<string> ungroupedNodeNames = new List<string>();

            foreach (DialogueSystemNode node in nodes)
            {
                SaveNodeToGraph(node, graphData);
                SaveNodeToScriptableObject(node, dialogueContainer);

                if (node.Group != null)
                {
                    groupedNodeNames.AddItem(node.Group.title, node.DialogueName);
                    continue;
                }

                ungroupedNodeNames.Add(node.DialogueName);
            }

            UpdateDialoguesChoicesConnections();

            UpdateOldGroupedNodes(groupedNodeNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
        }

        private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, GraphSaveDataSO graphData)
        {
            if (graphData.OldGroupedNodeNames != null && graphData.OldGroupedNodeNames.Count != 0)
            {
                foreach (KeyValuePair<string, List<string>> oldGroupedNode in graphData.OldGroupedNodeNames)
                {
                    List<string> nodesToRemove = new List<string>();

                    if (currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                    {
                        nodesToRemove = oldGroupedNode.Value.Except(currentGroupedNodeNames[oldGroupedNode.Key]).ToList();
                    }

                    foreach (string nodeToRemove in nodesToRemove)
                    {
                        RemoveAsset($"{containerFolderPath}/Groups/{oldGroupedNode.Key}/Dialogues", nodeToRemove);
                    }
                }
            }

            graphData.OldGroupedNodeNames = new SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
        }

        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, GraphSaveDataSO graphData)
        {
            if (graphData.OldUngroupedNodeNames != null && graphData.OldUngroupedNodeNames.Count != 0)
            {
                List<string> nodesToRemove = graphData.OldUngroupedNodeNames.Except(currentUngroupedNodeNames).ToList();

                foreach (string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{containerFolderPath}/Global/Dialogues", nodeToRemove);
                }
            }

            graphData.OldUngroupedNodeNames = new List<string>(currentUngroupedNodeNames);
        }

        private static void UpdateDialoguesChoicesConnections()
        {
            foreach (DialogueSystemNode node in nodes)
            {
                DialogueSO dialogue = createdDialogues[node.ID];

                for (int choiceIndex = 0; choiceIndex < node.Choices.Count; ++choiceIndex)
                {
                    ChoiceSaveData nodeChoice = node.Choices[choiceIndex];

                    if (string.IsNullOrEmpty(nodeChoice.NodeID))
                    {
                        continue;
                    }

                    dialogue.Choices[choiceIndex].NextDialogue = createdDialogues[nodeChoice.NodeID];

                    SaveAsset(dialogue);
                }
            }
        }

        private static void SaveNodeToScriptableObject(DialogueSystemNode node, DialogueContainerSO dialogueContainer)
        {
            DialogueSO dialogue;

            if (node.Group != null)
            {
                dialogue = CreateAsset<DialogueSO>($"{containerFolderPath}/Groups/{node.Group.title}/Dialogues", node.DialogueName);

                dialogueContainer.DialogueGroups.AddItem(createdDialogueGroups[node.Group.ID], dialogue);
            }
            else
            {
                dialogue = CreateAsset<DialogueSO>($"{containerFolderPath}/Global/Dialogues", node.DialogueName);

                dialogueContainer.UngroupedDialogues.Add(dialogue);
            }

            dialogue.Initialize(
                node.DialogueName,
                node.Text,
                ConvertNodeChoicesToDialogueChoices(node.Choices),
                node.DialogueType,
                node.IsStartingNode()
            );

            createdDialogues.Add(node.ID, dialogue);

            SaveAsset(dialogue);
        }

        private static List<DialogueChoiceData> ConvertNodeChoicesToDialogueChoices(List<ChoiceSaveData> nodeChoices)
        {
            List<DialogueChoiceData> dialogueChoices = new List<DialogueChoiceData>();

            foreach (ChoiceSaveData nodeChoice in nodeChoices)
            {
                DialogueChoiceData choiceData = new DialogueChoiceData()
                {
                    Text = nodeChoice.Text
                };

                dialogueChoices.Add(choiceData);
            }

            return dialogueChoices;
        }

        private static void SaveNodeToGraph(DialogueSystemNode node, GraphSaveDataSO graphData)
        {
            List<ChoiceSaveData> choices = CloneNodeChoices(node.Choices);

            NodeSaveData nodeData = new NodeSaveData()
            {
                ID = node.ID,
                Name = node.DialogueName,
                Choices = choices,
                Text = node.Text,
                GroupID = node.Group?.ID,
                DialogueType = node.DialogueType,
                Position = node.GetPosition().position
            };

            graphData.Nodes.Add(nodeData);
        }
        #endregion

        #endregion

        #region Load Methods
        public static void Load()
        {
            GraphSaveDataSO graphData = LoadAsset<GraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", graphFileName);

            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Graph Not Found",
                    "The graph you are trying to load does not exist.",
                    "OK"
                );
                return;
            }

            DialogueSystemEditorWindow.UpdateFileName(graphData.FileName);
            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodesConnections();
        }

        private static void LoadNodesConnections()
        {
            foreach (KeyValuePair<string, DialogueSystemNode> loadedNode in loadedNodes)
            {
                foreach (Port choicePort in loadedNode.Value.outputContainer.Children())
                {
                    ChoiceSaveData choiceData = choicePort.userData as ChoiceSaveData;

                    if (string.IsNullOrEmpty(choiceData.NodeID))
                    {
                        continue;
                    }

                    DialogueSystemNode nextNode = loadedNodes[choiceData.NodeID];

                    Port nextNodeInputPort = nextNode.inputContainer.Children().First() as Port;

                    Edge edge = choicePort.ConnectTo(nextNodeInputPort);

                    graphView.AddElement(edge);

                    loadedNode.Value.RefreshPorts();
                }
            }
        }

        private static void LoadNodes(List<NodeSaveData> nodes)
        {
            foreach (NodeSaveData nodeData in nodes)
            {
                List<ChoiceSaveData> choices = CloneNodeChoices(nodeData.Choices);
                DialogueSystemNode node = graphView.CreateNode("DialogueName", nodeData.DialogueType, nodeData.Position, false);

                node.ID = nodeData.ID;
                node.Choices = choices;
                node.Text = nodeData.Text;

                node.Draw();

                graphView.AddElement(node);

                loadedNodes.Add(node.ID, node);

                if (string.IsNullOrEmpty(nodeData.GroupID))
                {
                    continue;
                }

                DialogueSystemGroup group = loadedGroups[nodeData.GroupID];
                node.Group = group;
                group.AddElement(node);
            }
        }

        private static void LoadGroups(List<GroupSaveData> groups)
        {
            foreach (GroupSaveData groupData in groups)
            {
                DialogueSystemGroup group = graphView.CreateGroup(groupData.Name, groupData.Position);

                group.ID = groupData.ID;

                loadedGroups.Add(group.ID, group);
            }
        }
        #endregion

        #region Creation Methods
        private static void CreateStaticFolders()
        {
            CreateFolder("Assets/Editor/DialogueSystem", "Graphs");
            CreateFolder("Assets", "DialogueSystem");
            CreateFolder("Assets/DialogueSystem", "Dialogues");
            CreateFolder("Assets/DialogueSystem/Dialogues", graphFileName);
            CreateFolder(containerFolderPath, "Global");
            CreateFolder(containerFolderPath, "Groups");
            CreateFolder($"{containerFolderPath}/Global", "Dialogues");
        }
        #endregion

        #region Fetch Methods
        private static void GetElementsFromGraphView()
        {
            Type groupType = typeof(DialogueSystemGroup);
            graphView.graphElements.ForEach(graphElement =>
            {
                if (graphElement is DialogueSystemNode node)
                {
                    nodes.Add(node);

                    return;
                }

                if (graphElement.GetType() == groupType)
                {
                    DialogueSystemGroup group = graphElement as DialogueSystemGroup;

                    groups.Add(group);

                    return;
                }
            });
        }
        #endregion

        #region Utility Methods
        public static List<ChoiceSaveData> CloneNodeChoices(List<ChoiceSaveData> nodeChoices)
        {
            List<ChoiceSaveData> choices = new List<ChoiceSaveData>();

            foreach (ChoiceSaveData choice in nodeChoices)
            {
                ChoiceSaveData choiceData = new ChoiceSaveData()
                {
                    Text = choice.Text,
                    NodeID = choice.NodeID
                };

                choices.Add(choiceData);
            }

            return choices;
        }

        public static void CreateFolder(string path, string folderName)
        {
            if (AssetDatabase.IsValidFolder($"{path}/{folderName}"))
            {
                return;
            }

            AssetDatabase.CreateFolder(path, folderName);
        }

        public static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";

            T asset = LoadAsset<T>(path, assetName);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();

                AssetDatabase.CreateAsset(asset, fullPath);
            }

            return asset;
        }

        public static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";
            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

        public static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void RemoveFolder(string fullPath)
        {
            FileUtil.DeleteFileOrDirectory($"{fullPath}.meta");
            FileUtil.DeleteFileOrDirectory($"{fullPath}/");
        }

        public static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
        }
        #endregion
    }
}

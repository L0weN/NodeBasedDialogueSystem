using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Mert.DialogueSystem.Windows
{
    using Elements;
    using Enumerations;
    public class DialogueSystemSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueSystemGraphView graphView;
        private Texture2D indentationIcon;
        public void Initialize(DialogueSystemGraphView dialogueSystemGraphView)
        {
            graphView = dialogueSystemGraphView;

            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, Color.clear);
            indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Element")),
                new SearchTreeGroupEntry(new GUIContent("Dialogue Node"), 1),
                new SearchTreeEntry(new GUIContent("Single Choice", indentationIcon))
                {
                    level = 2,
                    userData = DialogueType.SingleChoice,
                },
                new SearchTreeEntry(new GUIContent("Multiple Choice", indentationIcon))
                {
                    level = 2,
                    userData = DialogueType.MultipleChoice,
                },
                new SearchTreeGroupEntry(new GUIContent("Dialogue Group"), 1),
                new SearchTreeEntry(new GUIContent("Single Group", indentationIcon))
                {
                    level = 2,
                    userData = new Group()
                }
            };

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);

            switch (SearchTreeEntry.userData)
            {
                case DialogueType.SingleChoice:
                    {
                        SingleChoiceNode singleChoiceNode = graphView.CreateNode(DialogueType.SingleChoice, localMousePosition) as SingleChoiceNode;
                        graphView.AddElement(singleChoiceNode);
                        return true;
                    }
                case DialogueType.MultipleChoice:
                    {
                        MultipleChoiceNode multipleChoiceNode = graphView.CreateNode(DialogueType.MultipleChoice, localMousePosition) as MultipleChoiceNode;
                        graphView.AddElement(multipleChoiceNode);
                        return true;
                    }
                case Group _:
                    {
                        Group group = graphView.CreateGroup("DialogueGroup", localMousePosition);
                        graphView.AddElement(group);
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
    }
}

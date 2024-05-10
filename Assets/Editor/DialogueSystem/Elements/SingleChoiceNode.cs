using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Mert.DialogueSystem.Elements
{
    using Enumerations;
    using Utilities;
    using Windows;
    using Data.Save;

    public class SingleChoiceNode : DialogueSystemNode
    {
        public override void Initialize(string nodeName, DialogueSystemGraphView dialogueSystemGraphView, Vector2 position)
        {
            base.Initialize(nodeName, dialogueSystemGraphView, position);

            DialogueType = DialogueType.SingleChoice;

            ChoiceSaveData choiceData = new ChoiceSaveData()
            {
                Text = "Next Dialogue"
            };

            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            foreach (ChoiceSaveData choice in Choices)
            {
                Port choicePort = this.CreatePort(choice.Text);

                choicePort.userData = choice;

                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }
    }
}

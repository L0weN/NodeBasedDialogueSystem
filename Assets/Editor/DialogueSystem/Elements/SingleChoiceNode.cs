using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Mert.DialogueSystem.Elements
{
    using Enumerations;

    public class SingleChoiceNode : DialogueSystemNode
    {
        public override void Initialize(Vector2 position)
        {
            base.Initialize(position);

            DialogueType = DialogueType.SingleChoice;

            Choices.Add("Next Dialogue");
        }

        public override void Draw()
        {
            base.Draw();

            foreach (string choice in Choices)
            {
                Port choicePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));

                choicePort.portName = choice;

                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }
    }
}

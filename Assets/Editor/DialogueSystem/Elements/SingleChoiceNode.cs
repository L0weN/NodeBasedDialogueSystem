using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Mert.DialogueSystem.Elements
{
    using Enumerations;
    using Utilities;

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
                Port choicePort = this.CreatePort(choice);

                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }
    }
}

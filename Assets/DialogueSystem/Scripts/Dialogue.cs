using UnityEngine;

namespace Mert.DialogueSystem
{
    using ScriptableObjects;
    public class Dialogue : MonoBehaviour
    {
        [SerializeField] private DialogueContainerSO dialogueContainer;
        [SerializeField] private DialogueGroupSO dialogueGroup;
        [SerializeField] private DialogueSO dialogue;

        [SerializeField] private bool groupedDialogues;
        [SerializeField] private bool startingDialoguesOnly;

        [SerializeField] private int selectedDialogueGroupIndex;
        [SerializeField] private int selectedDialogueIndex;
    }
}

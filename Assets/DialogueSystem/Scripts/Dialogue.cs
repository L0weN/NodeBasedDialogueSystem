using UnityEngine;

namespace Mert.DialogueSystem
{
    using ScriptableObjects;
    using TMPro;

    public class Dialogue : MonoBehaviour
    {
        [SerializeField] private DialogueContainerSO dialogueContainer;
        [SerializeField] private DialogueGroupSO dialogueGroup;
        [SerializeField] private DialogueSO dialogue;

        [SerializeField] private bool groupedDialogues;
        [SerializeField] private bool startingDialoguesOnly;

        [SerializeField] private int selectedDialogueGroupIndex;
        [SerializeField] private int selectedDialogueIndex;

        [SerializeField] private TextMeshProUGUI dialogueText;

        private void Start()
        {
            Debug.Log(dialogue.Text);
            dialogueText.text = dialogue.Text;
        }
    }
}

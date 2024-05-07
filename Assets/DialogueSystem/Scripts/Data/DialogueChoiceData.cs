using System;
using UnityEngine;

namespace Mert.DialogueSystem.Data
{
    using ScriptableObjects;
    [Serializable]
    public class DialogueChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public DialogueSO NextDialogue { get; set; }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Mert.DialogueSystem.ScriptableObjects
{
    public class DialogueContainerSO : ScriptableObject
    {
        public string FileName { get; set; }
        public SerializableDictionary<DialogueGroupSO, List<DialogueSO>> DialogueGroups { get; set; }
        public List<DialogueSO> UngroupedDialogues { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;
            DialogueGroups = new SerializableDictionary<DialogueGroupSO, List<DialogueSO>>();
            UngroupedDialogues = new List<DialogueSO>();
        }
    }
}

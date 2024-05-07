using System;
using UnityEngine;

namespace Mert.DialogueSystem.Data.Save
{
    [Serializable]
    public class ChoiceSaveData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public string NodeID { get; set; }
    }
}

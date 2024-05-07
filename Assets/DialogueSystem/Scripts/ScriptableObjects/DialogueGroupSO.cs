using UnityEngine;

namespace Mert.DialogueSystem.ScriptableObjects
{
    public class DialogueGroupSO : ScriptableObject
    {
        [field: SerializeField] public string GroupName { get; set; }

        public void Initialize(string groupName)
        {
            GroupName = groupName;
        }
    }
}

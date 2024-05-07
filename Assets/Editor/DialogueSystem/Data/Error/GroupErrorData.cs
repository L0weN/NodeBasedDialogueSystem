using System.Collections.Generic;

namespace Mert.DialogueSystem.Data.Error
{
    using Elements;
    public class GroupErrorData
    {
        public ErrorData ErrorData { get; set; }
        public List<DialogueSystemGroup> Groups { get; set; }

        public GroupErrorData()
        {
            ErrorData = new ErrorData();
            Groups = new List<DialogueSystemGroup>();
        }
    }
}

using System.Collections.Generic;

namespace Mert.DialogueSystem.Data.Error
{
    using Elements;
    public class NodeErrorData
    {
        public ErrorData ErrorData { get; set; }
        public List<DialogueSystemNode> Nodes { get; set; }

        public NodeErrorData()
        {
            ErrorData = new ErrorData();
            Nodes = new List<DialogueSystemNode>();
        }
    }
}

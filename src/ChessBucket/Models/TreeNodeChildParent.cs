namespace ChessBucket.Models
{
    public class TreeNodeTransition
    {
        public int Id { get; set; }
        public string San { get; set; }
        public TreeNode Parent { get; set; }
        public TreeNode Child { get; set; }
        public string Player { get; set; }
    }
}
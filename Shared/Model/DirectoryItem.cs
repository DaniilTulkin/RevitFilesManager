namespace RevitFilesManager
{
    internal class DirectoryItem
    {
        public RootType RootType { get; set; }
        public DirectoryItemType Type { get; set; }
        public string FullPath { get; set; }
        public string Name { get
            {
                return Type == DirectoryItemType.RootType ? RootType == RootType.Windows ? FullPath : "Server" :
                    DirectoryService.GetDirecroryItemName(RootType, FullPath);
            } 
        }
    }
}

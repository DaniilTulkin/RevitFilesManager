namespace RevitFilesManager
{
    internal class ServerProperties
    {
        public int MaximumFolderPathLength { get; set; }
        public int MaximumModelNameLength { get; set; }
        public int[] ServerRoles { get; set; }
        public string[] Servers { get; set; }
    }
}

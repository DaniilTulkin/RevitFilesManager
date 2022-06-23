using System;

namespace RevitFilesManager
{
    internal class Contents
    {
        public string Path { get; set; }
        public string LockContext { get; set; }
        public int LockState { get; set; }
        public ModelLockInProgress[] ModelLocksInProgress { get; set; }
        public Folder[] Folders { get; set; }
        public RevitModel[] Models { get; set; }
    }

    public class ModelLockInProgress
    {
        public TimeSpan MyProperty { get; set; }
        public int ModelLockOption { get; set; }
        public int ModelLockType { get; set; }
        public string ModelPath { get; set; }
        public DateTime TimeStamp { get; set; }
        public string UserName { get; set; }
    }

    public class Folder
    {
        public bool HasContents { get; set; }
        public string LockContext { get; set; }
        public int LockState { get; set; }
        public ModelLockInProgress[] ModelLocksInProgress { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
    }

    public class RevitModel
    {
        public bool HasContents { get; set; }
        public string LockContext { get; set; }
        public int LockState { get; set; }
        public ModelLockInProgress[] ModelLocksInProgress { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
    }
}

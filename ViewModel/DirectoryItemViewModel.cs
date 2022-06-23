using System.Collections.ObjectModel;
using System.Linq;

namespace RevitFilesManager
{
    internal class DirectoryItemViewModel : BaseViewModel
    {
        public RootType RootType { get; set; }
        public DirectoryItemType Type { get; set; }
        public string FullPath { get; set; }
        public string Name
        {
            get
            {
                return Type == DirectoryItemType.RootType ? RootType == RootType.Windows ? FullPath : "Revit server" :
                    DirectoryService.GetDirecroryItemName(RootType, FullPath);
            }
        }
        public DirectoryService DirectoryService { get; set; }
        public ObservableCollection<DirectoryItemViewModel> Children { get; set; }
        public bool CanExpand { get { return Type != DirectoryItemType.FileType; } }
        public bool IsExpanded
        {
            get
            {
                return Children?.Count(x => x != null) > 0;
            }
            set
            {
                if (value == true) Expand();
                else ClearChildren();
            }
        }

        public DirectoryItemViewModel(RootType rootType, 
                                      string fullPath, 
                                      DirectoryItemType type,
                                      DirectoryService directoryService)
        {
            RootType = rootType;
            FullPath = fullPath;
            Type = type;
            DirectoryService = directoryService;
            ClearChildren();
        }

        private void Expand()
        {
            if (Type == DirectoryItemType.FileType) return;

            Children = new ObservableCollection<DirectoryItemViewModel>(
                DirectoryService.GetDirectoryItemContent(RootType, FullPath)
                .Select(x => new DirectoryItemViewModel(x.RootType, x.FullPath, x.Type, DirectoryService)));
        }

        private void ClearChildren()
        {
            Children = new ObservableCollection<DirectoryItemViewModel>();

            if (Type != DirectoryItemType.FileType) Children.Add(null);
        }
    }
}

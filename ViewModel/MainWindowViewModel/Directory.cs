using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace RevitFilesManager
{
    internal partial class MainWindowViewModel
    {
        public bool CurrenDirectoryIsWindows { get; set; } = true;
        public bool CurrenDirectoryIsRS { get; set; }
        public ObservableCollection<string> RSVersions { get; set; }
        public string SelectedRSVersion { get; set; }
        public ObservableCollection<string> RSAdresses { get; set; }
        public string SelectedRSAdress { get; set; }
        public ObservableCollection<DirectoryItemViewModel> Items { get; set; }
        public string CurrentPath { get; set; }

        private DirectoryService currentDirectoryService;

        public ICommand CurrentDirectoryChecked => new RelayCommandWithoutParameter(() =>
        {
            CurrentPath = null;
            CopyButton = false;

            if (CurrenDirectoryIsWindows)
            {
                SelectedRSVersion = null;
                SelectedRSAdress = null;
                if (DestinationDirectoryIsRS) DestinationRSVersionCombobox = true;
                Items = GetRootItems();
            }
            else if (CurrenDirectoryIsRS)
            {
                Items.Clear();
                RSVersions = new ObservableCollection<string>(currentDirectoryService.GetRSVersions());
            }
        });
        public ICommand SelectedRSVersionChanged => new RelayCommandWithoutParameter(() =>
        {
            CurrentPath = null;
            CopyButton = false;

            SelectedRSAdress = null;
            Items.Clear();
            if (SelectedRSVersion != null)
            {
                SelectedDestinationRSVersion = SelectedRSVersion;
                DestinationRSVersionCombobox = false;
                RSAdresses = new ObservableCollection<string>(currentDirectoryService.GetRSAdresses(SelectedRSVersion));
            }
        });
        public ICommand SelectedRSAdressChanged => new RelayCommandWithoutParameter(() =>
        {
            CurrentPath = null;
            CopyButton = false;

            if (SelectedRSAdress != null) Items = GetRootItems();
        });
        public ICommand SelectedCurrentPath => new RelayCommand<string>(x =>
        {
            CurrentPath = x;

            if (DestinationPath != null) CopyButton = true;
        });

        private ObservableCollection<DirectoryItemViewModel> GetRootItems()
        {
            currentDirectoryService = new DirectoryService();
            return new ObservableCollection<DirectoryItemViewModel>(
                currentDirectoryService.GetRootItems(CurrenDirectoryIsWindows, SelectedRSVersion, SelectedRSAdress)
                .Select(x => new DirectoryItemViewModel(x.RootType, x.FullPath, DirectoryItemType.RootType, currentDirectoryService)));
        }
    }
}

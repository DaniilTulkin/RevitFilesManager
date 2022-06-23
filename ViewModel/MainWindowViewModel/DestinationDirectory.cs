using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace RevitFilesManager
{
    internal partial class MainWindowViewModel
    {
        public bool DestinationDirectoryIsWindows { get; set; } = true;
        public bool DestinationDirectoryIsRS { get; set; }
        public bool DestinationRSVersionCombobox { get; set; }
        public string SelectedDestinationRSVersion { get; set; }
        public ObservableCollection<string> DestinationRSAdresses { get; set; }
        public string SelectedDestinationRSAdress { get; set; }
        public ObservableCollection<DirectoryItemViewModel> DestinationItems { get; set; }
        public string DestinationPath { get; set; }

        private DirectoryService destinationDirectoryService;

        public ICommand DestinationDirectoryChecked => new RelayCommandWithoutParameter(() =>
        {
            DestinationPath = null;
            CopyButton = false;

            if (DestinationDirectoryIsWindows)
            {
                SelectedDestinationRSVersion = null;
                SelectedDestinationRSAdress = null;
                DestinationRSVersionCombobox = false;
                DestinationItems = GetDestinationRootItems();
            }
            else if (DestinationDirectoryIsRS)
            {
                DestinationItems.Clear();
                if (SelectedRSVersion == null)
                {
                    DestinationRSVersionCombobox = true;
                    RSVersions = new ObservableCollection<string>(destinationDirectoryService.GetRSVersions());
                }
                else
                {
                    SelectedDestinationRSVersion = SelectedRSVersion;
                    DestinationRSAdresses = new ObservableCollection<string>(destinationDirectoryService.GetRSAdresses(SelectedDestinationRSVersion));
                }
            }
        });
        public ICommand SelectedDestinationRSVersionChanged => new RelayCommandWithoutParameter(() =>
        {
            DestinationPath = null;
            CopyButton = false;

            SelectedDestinationRSAdress = null;
            if (DestinationDirectoryIsRS) DestinationItems.Clear();
            if (SelectedDestinationRSVersion != null)
                DestinationRSAdresses = new ObservableCollection<string>(destinationDirectoryService.GetRSAdresses(SelectedDestinationRSVersion));
        });
        public ICommand SelectedDestinationRSAdressChanged => new RelayCommandWithoutParameter(() =>
        {
            DestinationPath = null;
            CopyButton = false;

            if (SelectedDestinationRSAdress != null) DestinationItems = GetDestinationRootItems();
        });
        public ICommand SelectedDestinationPath => new RelayCommand<string>(x =>
        {
            DestinationPath = x;
            if (CurrentPath != null) CopyButton = true;
        });

        private ObservableCollection<DirectoryItemViewModel> GetDestinationRootItems()
        {
            destinationDirectoryService = new DirectoryService();
            return new ObservableCollection<DirectoryItemViewModel>(
                destinationDirectoryService.GetRootItems(DestinationDirectoryIsWindows, SelectedDestinationRSVersion, SelectedDestinationRSAdress)
                .Select(x => new DirectoryItemViewModel(x.RootType, x.FullPath, DirectoryItemType.RootType, destinationDirectoryService)));
        }
    }
}

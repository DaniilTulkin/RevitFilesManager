using Autodesk.Revit.UI;
using System;
using System.Windows.Input;

namespace RevitFilesManager
{
    internal partial class MainWindowViewModel : BaseViewModel
    {
        public Action CloseAction { get; set; }
        public bool CopyButton { get; set; }

        FileOperationService operationService;

        public MainWindowViewModel(UIApplication app)
        {
            operationService = new FileOperationService(app, this);
            Items = GetRootItems();
            DestinationItems = GetDestinationRootItems();
        }

        public ICommand Cancel => new RelayCommandWithoutParameter(() => CloseAction());
        public ICommand Copy => new RelayCommandWithoutParameter(() => operationService.Copy());
    }
}

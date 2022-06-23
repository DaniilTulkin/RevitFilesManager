using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RevitFilesManager
{
    internal class FileOperationService
    {
        private UIApplication app;
        private UIDocument uidoc;
        private Document doc;

        MainWindowViewModel vm;
        OpenOptions openOptions;
        SaveAsOptions saveAsOptions;
        TransactWithCentralOptions transactWithCentralOptions;
        SynchronizeWithCentralOptions synchronizeWithCentralOptions;

        public FileOperationService(UIApplication app, MainWindowViewModel vm)
        {
            this.app = app;
            uidoc = app.ActiveUIDocument;
            doc = uidoc.Document;

            this.vm = vm;

            openOptions = new OpenOptions();
            openOptions.Audit = true;
            openOptions.DetachFromCentralOption = DetachFromCentralOption.ClearTransmittedSaveAsNewCentral;
            openOptions.SetOpenWorksetsConfiguration(new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets));

            saveAsOptions = new SaveAsOptions();
            saveAsOptions.Compact = true;
            saveAsOptions.MaximumBackups = 3;
            saveAsOptions.OverwriteExistingFile = true;
            saveAsOptions.SetWorksharingOptions(new WorksharingSaveAsOptions { SaveAsCentral = true });

            RelinquishOptions relinquishOptions = new RelinquishOptions(true);
            relinquishOptions.CheckedOutElements = true;
            relinquishOptions.FamilyWorksets = true;
            relinquishOptions.StandardWorksets = true;
            relinquishOptions.UserWorksets = true;
            relinquishOptions.ViewWorksets = true;

            transactWithCentralOptions = new TransactWithCentralOptions();
            synchronizeWithCentralOptions = new SynchronizeWithCentralOptions();
            synchronizeWithCentralOptions.SetRelinquishOptions(relinquishOptions);
            synchronizeWithCentralOptions.SaveLocalAfter = true;
        }

        internal void Copy()
        {
            if (vm.CurrentPath != null && vm.DestinationPath != null)
            {
                DialogResult dialogResult = MessageBox.Show("Вы уверены, что хотите копировать выбранную директорию?",
                                                            "Предупреждение",
                                                            MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    if (vm.CurrenDirectoryIsWindows && vm.DestinationDirectoryIsWindows)
                        CopyWinToWin(vm.CurrentPath, vm.DestinationPath);
                    else if (vm.CurrenDirectoryIsWindows && vm.DestinationDirectoryIsRS)
                        CopyWinToRS(vm.CurrentPath, vm.DestinationPath, vm.SelectedDestinationRSAdress);
                    else if (vm.CurrenDirectoryIsRS && vm.DestinationDirectoryIsWindows)
                        CopyRSToWin(vm.CurrentPath, vm.DestinationPath, vm.SelectedRSAdress);
                    else if (vm.CurrenDirectoryIsRS && vm.DestinationDirectoryIsRS)
                        CopyRSToRS(vm.CurrentPath, vm.DestinationPath, vm.SelectedRSAdress, vm.SelectedDestinationRSAdress);
                }
            }
        }

        private void CopyWinToWin(string currentPath,
                                  string destinationPath)
        {
            if (new FileInfo(destinationPath).Name.Contains("."))
            {
                destinationPath = destinationPath.Replace(new FileInfo(destinationPath).Name, "");
                destinationPath = destinationPath.Remove(destinationPath.Length - 1);
            }            

            FileAttributes fileAttributes = File.GetAttributes(currentPath);
            if (fileAttributes.HasFlag(FileAttributes.Directory))
            {
                foreach (string filePath in Directory.GetFiles(currentPath))
                {
                    if (IsRvt(filePath))
                    {
                        ModelPath modelCurrentPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);
                        ModelPath modelDestinationtPath = ModelPathUtils
                        .ConvertUserVisiblePathToModelPath(destinationPath + $"\\{new FileInfo(filePath).Name}");

                        CopyFile(modelCurrentPath, modelDestinationtPath);
                    }
                }
            }
            else
            {
                if (IsRvt(currentPath))
                {
                    ModelPath modelCurrentPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(currentPath);
                    ModelPath modelDestinationtPath = ModelPathUtils
                    .ConvertUserVisiblePathToModelPath(destinationPath + $"\\{new FileInfo(currentPath).Name}");

                    CopyFile(modelCurrentPath, modelDestinationtPath);
                }
            }
        }

        private void CopyWinToRS(string currentPath, 
                                 string destinationPath, 
                                 string selectedDestinationRSAdress)
        {
            destinationPath = $"RSN://{selectedDestinationRSAdress}{destinationPath.Replace("|", "/")}";

            if (destinationPath.Split('/').Last().Contains("."))
                destinationPath = destinationPath.Replace(destinationPath.Split('/').Last(), "");

            FileAttributes fileAttributes = File.GetAttributes(currentPath);
            if (fileAttributes.HasFlag(FileAttributes.Directory))
            {
                foreach (string filePath in Directory.GetFiles(currentPath))
                {
                    if (IsRvt(filePath))
                    {
                        ModelPath modelCurrentPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);
                        ModelPath modelDestinationtPath = ModelPathUtils
                        .ConvertUserVisiblePathToModelPath(destinationPath + $"{new FileInfo(filePath).Name}");

                        CopyFile(modelCurrentPath, modelDestinationtPath);
                    }
                }
            }
            else
            {
                if (IsRvt(currentPath))
                {
                    ModelPath modelCurrentPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(currentPath);
                    ModelPath modelDestinationtPath = ModelPathUtils
                    .ConvertUserVisiblePathToModelPath(destinationPath + $"{new FileInfo(currentPath).Name}");

                    CopyFile(modelCurrentPath, modelDestinationtPath);
                }
            }
        }

        private void CopyRSToWin(string currentPath, 
                                 string destinationPath,   
                                 string selectedRSAdress)
        {
            if (new FileInfo(destinationPath).Name.Contains("."))
            {
                destinationPath = destinationPath.Replace(new FileInfo(destinationPath).Name, "");
                destinationPath = destinationPath.Remove(destinationPath.Length - 1);
            }
                        
            if (!IsRvt(currentPath))
            {
                RevitServerService revitServerService = new RevitServerService(vm.SelectedRSVersion, vm.SelectedRSAdress);
                Contents contents = revitServerService.GetContents(currentPath);

                foreach (RevitModel revitModel in contents.Models)
                {
                    string filePath = $"RSN://{selectedRSAdress}{(currentPath + $"{revitModel.Name}").Replace("|", "/")}";

                    ModelPath modelCurrentPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);
                    ModelPath modelDestinationtPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(
                                                      destinationPath + $"\\{filePath.Split('/').Last()}");

                    CopyFile(modelCurrentPath, modelDestinationtPath);
                }
            }
            else
            {
                currentPath = $"RSN://{selectedRSAdress}{currentPath.Replace("|", "/")}";

                ModelPath modelCurrentPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(currentPath);
                ModelPath modelDestinationtPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(
                                                  destinationPath + $"\\{currentPath.Split('/').Last()}");

                CopyFile(modelCurrentPath, modelDestinationtPath);
            }
        }

        private void CopyRSToRS(string currentPath, 
                                string destinationPath, 
                                string selectedRSAdress, 
                                string selectedDestinationRSAdress)
        {
            destinationPath = $"RSN://{selectedDestinationRSAdress}{destinationPath.Replace("|", "/")}";

            if (destinationPath.Split('/').Last().Contains("."))
                destinationPath = destinationPath.Replace(destinationPath.Split('/').Last(), "");

            if (!IsRvt(currentPath))
            {
                RevitServerService revitServerService = new RevitServerService(vm.SelectedRSVersion, vm.SelectedRSAdress);
                Contents contents = revitServerService.GetContents(currentPath);

                foreach (RevitModel revitModel in contents.Models)
                {
                    string filePath = $"RSN://{selectedRSAdress}{(currentPath + $"{revitModel.Name}").Replace("|", "/")}";

                    ModelPath modelCurrentPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);
                    ModelPath modelDestinationtPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(
                        destinationPath + $"{filePath.Split('/').Last()}");

                    CopyFile(modelCurrentPath, modelDestinationtPath);
                }
            }
            else
            {
                currentPath = $"RSN://{selectedRSAdress}{currentPath.Replace("|", "/")}";

                ModelPath modelCurrentPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(currentPath);
                ModelPath modelDestinationtPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(
                    destinationPath + $"{currentPath.Split('/').Last()}");

                CopyFile(modelCurrentPath, modelDestinationtPath);
            }
        }

        private bool IsRvt(string filePath) => filePath.Split('.').Last() == "rvt";

        private void CopyFile(ModelPath modelCurrentPath, ModelPath modelDestinationtPath)
        {
            app.DialogBoxShowing += UiAppOnDialogBoxShowing;
            string tempPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TempModel.rvt";
            ModelPath tempModelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(tempPath);

            app.Application.CopyModel(modelCurrentPath, tempPath, true);
            SetTransmissionDada(tempModelPath, modelCurrentPath, modelDestinationtPath);

            Document tempDoc = null;
            try { tempDoc = app.Application.OpenDocumentFile(tempModelPath, openOptions); }
            catch { return; }

            if (tempDoc != null)
            {
                if (!tempDoc.IsWorkshared)
                    tempDoc.EnableWorksharing("!Общие уровни и оси", "!Элементы модели");

                tempDoc.SaveAs(modelDestinationtPath, saveAsOptions);
                tempDoc.SynchronizeWithCentral(transactWithCentralOptions, synchronizeWithCentralOptions);
                tempDoc.Close(false);
            }

            File.Delete(tempPath);
            app.DialogBoxShowing -= UiAppOnDialogBoxShowing;
        }

        private void SetTransmissionDada(ModelPath openModelPath, ModelPath modelCurrentPath, ModelPath modelDestinationtPath)
        {
            string currentPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelCurrentPath);
            string destinationPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelDestinationtPath);

            TransmissionData trData = TransmissionData.ReadTransmissionData(openModelPath);

            var referenceIds = trData.GetAllExternalFileReferenceIds();
            foreach (ElementId referenceId in referenceIds)
            {
                ExternalFileReference reference = trData.GetLastSavedReferenceData(referenceId);
                if(reference.ExternalFileReferenceType == ExternalFileReferenceType.RevitLink)
                {
                    string newLinkPath = GetNewLinkPath(reference, currentPath, destinationPath);
                    ModelPath linkModelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(newLinkPath);

                    PathType pathType = PathType.Absolute;
                    if (destinationPath.Substring(0, 3).ToUpper() == "RSN") pathType = PathType.Server;

                    trData.SetDesiredReferenceData(referenceId, linkModelPath, pathType, true);
                    trData.IsTransmitted = true;
                    TransmissionData.WriteTransmissionData(openModelPath, trData);
                }
            }
        }

        private string GetNewLinkPath(ExternalFileReference reference, 
                                      string currentPath, 
                                      string destinationPath)
        {
            string relativePath = GetRelativePath(currentPath, ConvertPath(currentPath, destinationPath));
            string rootCurrentPath = currentPath.Replace(relativePath, "");
            string rootDestinationPath = destinationPath.Replace(ConvertPath(destinationPath, relativePath), "");

            string linkFullName = ModelPathUtils.ConvertModelPathToUserVisiblePath(reference.GetAbsolutePath());
            if (reference.PathType == PathType.Server) CorrectServerPath(ref linkFullName, rootCurrentPath);

            return rootDestinationPath + ConvertPath(destinationPath, linkFullName.Replace(rootCurrentPath, ""));
        }

        private void CorrectServerPath(ref string linkFullName, string rootCurrentPath)
        {
            foreach (string subLinkPath in linkFullName.Split('/').ToList().Take(3))
            {
                if (string.IsNullOrEmpty(subLinkPath)) continue;
                foreach (string subRootPath in rootCurrentPath.Split('/').ToList().Take(3))
                {                    
                    if (subLinkPath.ToLower().Equals(subRootPath.ToLower()))
                    {
                        linkFullName = linkFullName.Replace(subLinkPath, subRootPath);
                        break;
                    }                        
                }
            }
        }

        private string GetRelativePath(string currentPath, string destinationPath)
        {
            int[,] lengths = new int[currentPath.Length, destinationPath.Length];
            int greatestLength = 0;
            string relativePath = "";
            for (int i = 0; i < currentPath.Length; i++)
            {
                for (int j = 0; j < destinationPath.Length; j++)
                {
                    if (currentPath[i] == destinationPath[j])
                    {
                        lengths[i, j] = i == 0 || j == 0 ? 1 : lengths[i - 1, j - 1] + 1;
                        greatestLength = lengths[i, j];
                        relativePath = currentPath.Substring(i - greatestLength + 1, greatestLength);
                    }
                    else
                    {
                        lengths[i, j] = 0;
                    }
                }
            }
            return relativePath;
        }

        private string ConvertPath(string path, string pathToConvert)
        {
            if (path.Substring(0, 3).ToUpper() == "RSN")
                return pathToConvert.Replace("\\", "/");
            else return pathToConvert.Replace("/", "\\");
        }

        private static void UiAppOnDialogBoxShowing(object sender, DialogBoxShowingEventArgs args)
        {
            switch (args)
            {
                case TaskDialogShowingEventArgs args2:
                    if (args2.DialogId == "TaskDialog_Save_As_Central_Model")
                        args2.OverrideResult(1002);
                    break;
                default:
                    return;
            }
        }
    }
}

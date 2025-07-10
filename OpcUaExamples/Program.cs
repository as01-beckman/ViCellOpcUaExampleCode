using Opc.Ua;
using System;
using System.Linq;
using OPCUAExamples;


namespace ViCellOpcUaClient
{
    class ViCellOpcUaClient_SimpleLock
    {
        // Stores the collection of method nodes and play control node found during browsing.
        private static ReferenceDescriptionCollection _allCollection;
        // Application configuration for OPC UA client.
        static void Main(string[] args)
        {

            try
            {
                ConnectionManager.InitializeConnection();

                _allCollection = new ReferenceDescriptionCollection();
                if (ConnectionManager._methodCollection != null)
                { _allCollection.AddRange(ConnectionManager._methodCollection); }
                if (ConnectionManager._methodCollectionPlayControl != null)
                { _allCollection.AddRange(ConnectionManager._methodCollectionPlayControl); }

                // Get and display available commands
                var commands = GetAvailableCommands();
                var menuCommands = commands.Concat(new[] { "Exit" }).ToArray();

                while (true)
                {
                    Console.WriteLine("Available commands:");
                    for (int i = 0; i < menuCommands.Length; i++)
                    {
                        Console.WriteLine($"{i + 1}. {menuCommands[i]}");
                    }
                    Console.WriteLine("Select a command by entering its number:");
                    if (int.TryParse(Console.ReadLine(), out int selection) && selection > 0 && selection <= menuCommands.Length)
                    {
                        var selectedCommand = menuCommands[selection - 1];
                        if (selectedCommand == "Exit")
                        {
                            Console.WriteLine("Exiting application.");
                            break;
                        }
                        switch (selectedCommand)
                        {
                            case "RequestLock":
                                RequestLockOperation requestLockOperation = new RequestLockOperation();
                                requestLockOperation.CallRequestLock();
                                Console.WriteLine("RequestLock call end.");
                                break;
                            case "ReleaseLock":
                                ReleaseLockOperation releaseLockOperation = new ReleaseLockOperation();
                                releaseLockOperation.CallReleaseLock();
                                Console.WriteLine("ReleaseLock call end.");
                                break;
                            case "StartSample":
                                StartSampleOperation startSample = new StartSampleOperation();
                                startSample.CallStartSample();
                                Console.WriteLine("StartSample call end.");
                                break;
                            case "StartSampleSet":
                                StartSampleSetOperation startSampleSet = new StartSampleSetOperation();
                                startSampleSet.CallStartSampleSet();
                                Console.WriteLine("StartSampleSet call end.");
                                break;
                            case "GetSampleResults":
                                GetSampleResultsOperation getSampleResultsOperation = new GetSampleResultsOperation();
                                getSampleResultsOperation.CallGetSampleResults();
                                Console.WriteLine("GetSampleResults call end.");
                                break;
                            case "DeleteSampleResults":
                                DeleteSampleResultsOperation deleteSampleResultsOperation = new DeleteSampleResultsOperation();
                                // With the following code to prompt the user for the UUID:
                                Console.Write("Get the UUID by running GetSampleResults command");
                                Console.Write("\nEnter the UUID or multiple UUIDs (separated by commas) of the sample results you wish to delete: ");
                                {
                                    string getUUID = Console.ReadLine();

                                    // Prompt for retainResults (yes/no)
                                    Console.Write("Do you want to retain the results? (yes/no): ");
                                    string retainResultsInput = Console.ReadLine();
                                    bool getRetainResults = retainResultsInput?.Trim().ToLower() == "yes";

                                    deleteSampleResultsOperation.CallDeleteSampleResults(getUUID, getRetainResults);
                                    Console.WriteLine("DeleteSampleResults call end.");
                                }
                                break;
                            case "StartExport":
                                StartSampleExportOperation startExport = new StartSampleExportOperation();
                                // With the following code to prompt the user for the UUID:
                                Console.Write("Get the UUID by running GetSampleResults command");
                                Console.Write("\nEnter the UUID : ");
                                {
                                    string getUUID = Console.ReadLine();
                                    startExport.CallRetrieveSampleExport(getUUID);
                                    Console.WriteLine("StartExport call end.");
                                }
                                 break;
                            case "CreateCellType":
                                CreateCellTypeOperation createCellTypeOperation = new CreateCellTypeOperation();
                                createCellTypeOperation.CallCreateCellType();
                                Console.WriteLine("CreateCellType call end.");
                                break;
                            case "DeleteCellType":
                                DeleteCellTypeOperation deleteCellTypeOperation = new DeleteCellTypeOperation();
                                Console.Write("\nEnter the cell type name that you wish to delete: ");
                                string getCellTypeName = Console.ReadLine();
                                deleteCellTypeOperation.CallDeleteCellType(getCellTypeName);
                                Console.WriteLine("DeleteCellType call end.");
                                break;
                            case "CreateQualityControl":
                                CreateQualityControlOperation createQualityControlOperation = new CreateQualityControlOperation();
                                createQualityControlOperation.CallQualityControl() ;
                                Console.WriteLine("CreateQualityControl call end.");
                                break;
                            case "GetQualityControls":
                                GetQualityControl getQualityControl = new GetQualityControl();
                                getQualityControl.CallGetQualityControl();
                                Console.WriteLine("CreateQualityControl call end.");
                                break;
                            case "ExportConfig":
                                ExportConfigOperation exportConfigOperation = new ExportConfigOperation();
                                Console.Write("\nEnter the path where you want to save the file : ");
                                string fullFilePathToSaveConfigIncludingExtension = Console.ReadLine();
                                exportConfigOperation.CallExportConfig(fullFilePathToSaveConfigIncludingExtension);
                                Console.WriteLine("ExportConfig call end.");
                                break;
                            case "ImportConfig":
                                ImportConfigOperation importConfigOperation = new ImportConfigOperation();
                                Console.Write("\nEnter the path where you want to save the file : ");
                                string importFilePath = Console.ReadLine();
                                importConfigOperation.CallImportConfig(importFilePath);
                                Console.WriteLine("ImportConfig call end.");
                                break;
                            case "GetAvailableDiskSpace":
                                GetAvailableDiskSpace getAvailableDiskSpace = new GetAvailableDiskSpace();
                                getAvailableDiskSpace.CallGetAvailableDiskSpace(); 
                                Console.WriteLine("GetAvailableDiskSpace call end.");
                                break;
                            case "Pause":
                                PauseOperation pauseOperation = new PauseOperation();
                                pauseOperation.CallPauseOperation();
                                Console.WriteLine("Pause call end.");
                                break;
                            case "EjectStage":
                                EjectStageOperation ejectStageOperation = new EjectStageOperation();
                                ejectStageOperation.CallEjectStageOperation();
                                Console.WriteLine("EjectStage call end.");
                                break;
                            case "Resume":
                                ResumeOperation resumeOperation = new ResumeOperation();
                                resumeOperation.CallResumeOperation();
                                Console.WriteLine("Resume call end.");
                                break;
                            case "Stop":
                                StopOperation stopOperation = new StopOperation();
                                stopOperation.CallStopOperation();
                                Console.WriteLine("Stop call end.");
                                break;
                            default:
                                Console.WriteLine($"Command '{selectedCommand}' is not implemented yet.");
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid selection.");
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Gets all available method names (commands) from the Methods node.
        /// </summary>
        /// <returns>Array of method names as strings.</returns>
        public static string[] GetAvailableCommands()
        {
            if (_allCollection == null)
                throw new InvalidOperationException("Method collection is not initialized. Call SetupMethodCollection() first.");

            return _allCollection
                .Where(n => n.NodeClass == NodeClass.Method)
                .Select(n => n.DisplayName.Text)
                .ToArray();
        }
    }

}




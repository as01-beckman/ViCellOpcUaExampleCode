using Opc.Ua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViCellBlu;

namespace OPCUAExamples
{
    public class ExportConfigOperation
    {
        /// <summary>
        /// Calls the "ExportConfig" method on the Vi-Cell BLU OPC UA server.
        /// Decodes and prints the specific result structure for ExportConfig.
        /// </summary>
        public void CallExportConfig(String fullFilePathToSaveConfigIncludingExtension)
        {
            // Find the specific OPC UA method ("ExportConfig") from a collection of methods.
            var methodExportConfig = ConnectionManager._methodCollection.First(n => n.DisplayName.ToString().Equals(ViCellBlu.BrowseNames.ExportConfig)); //"ExportConfig"
            var methodNodeExportConfig = ExpandedNodeId.ToNodeId(methodExportConfig.NodeId, ConnectionManager._opcSession.NamespaceUris);

            Console.WriteLine($"Calling method: {methodExportConfig.DisplayName}");

            // Prepare method call request (no input arguments for ExportConfig)
            var reqHeaderExportConfig = new RequestHeader();
            var cmRequestExportConfig = new CallMethodRequest
            {
                ObjectId = ConnectionManager._parentMethodNode,
                MethodId = methodNodeExportConfig,
                InputArguments = new VariantCollection() // ExportConfig has no input arguments
            };
            var cmReqCollectionExportConfig = new CallMethodRequestCollection { cmRequestExportConfig };

            // Show what is being sent
            Console.WriteLine("Sending:");
            Console.WriteLine($"  Command: {methodExportConfig.DisplayName}");
            //Console.WriteLine($"  ObjectId: {_parentMethodNode}"); // Optional: show Object and Method IDs
            //Console.WriteLine($"  MethodId: {methodNodeExportConfig}");

            // Call the method
            ConnectionManager._opcSession.Call(reqHeaderExportConfig, cmReqCollectionExportConfig, out var resultCollectionExportConfig, out var diagResultsExportConfig);

            // Show output
            var result = resultCollectionExportConfig[0];
            Console.WriteLine("Method call result status: " + result.StatusCode);

            // decoding the output if available
            if ((resultCollectionExportConfig.Count > 0) && (resultCollectionExportConfig[0].OutputArguments.Count > 0))
            {
                Console.WriteLine("Decoding method output...");
                // Use the specific decode method for ExportConfig results
                DecodeRawExportConfig(resultCollectionExportConfig[0].OutputArguments[0].Value,
                    (ServiceMessageContext)ConnectionManager._opcSession.MessageContext, diagResultsExportConfig, fullFilePathToSaveConfigIncludingExtension);
            }
            else
            {
                Console.WriteLine("No output values returned by the method.");
                // Still check diagnostic results if the call itself had issues
                if (diagResultsExportConfig != null && diagResultsExportConfig.Count > 0)
                {
                    Console.WriteLine($"Diagnostic result: {diagResultsExportConfig.ToString()}");
                }
            }
        }

        /// <summary>
        /// Decodes the raw byte[] output from the ExportConfig method into a specific ViCellBlu result structure.
        /// Assumes the output is an ExtensionObject containing binary data decodeable as a ViCellBlu.VcbResult
        /// followed by the specific LockStateEnum.
        /// </summary>
        public void DecodeRawExportConfig(Object rawResult, ServiceMessageContext messageContext, DiagnosticInfoCollection diagResultsExportConfig, string fullFilePathToSaveConfigIncludingExtension)
        {
            ViCellBlu.VcbResultExportConfig callResult = new ViCellBlu.VcbResultExportConfig { ErrorLevel = ErrorLevelEnum.Warning, MethodResult = MethodResultEnum.Failure };
            callResult.ErrorLevel = ErrorLevelEnum.Warning;
            callResult.MethodResult = MethodResultEnum.Failure;
            callResult.ResponseDescription = "Decoding raw result ...";
            try
            {
                byte[] myData;
                var val = (Opc.Ua.ExtensionObject)rawResult;
                myData = (byte[])val.Body;
                callResult.Decode(new Opc.Ua.BinaryDecoder(myData, 0, myData.Count(), messageContext));

                if (callResult.ErrorLevel == ErrorLevelEnum.NoError)
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    // Combine the base name, timestamp, and extension
                    string baseFileName = "ExportConfigOPCUAExample";
                    string fileExtension = ".cfg";
                    string uniqueFileNameToAppend = $"{baseFileName}_{timestamp}{fileExtension}";
                    File.WriteAllBytes(Path.Combine(fullFilePathToSaveConfigIncludingExtension, uniqueFileNameToAppend), callResult.FileData); //writing the result to the file
                }

                Console.WriteLine("Decoded VcbResult:");
                Console.WriteLine("  ErrorLevel: " + callResult.ErrorLevel);
                Console.WriteLine("  MethodResult: " + callResult.MethodResult);
                Console.WriteLine("  ResponseDescription: " + callResult.ResponseDescription);


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excption: {ex.ToString()}");
            }
        }
    }
}

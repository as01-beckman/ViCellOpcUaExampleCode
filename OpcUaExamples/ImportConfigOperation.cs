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
    public class ImportConfigOperation
    {
        /// <summary>
        /// Calls the "ImportConfig" method on the Vi-Cell BLU OPC UA server.
        /// Decodes and prints the specific result structure for ImportConfig.
        /// </summary>
        public void CallImportConfig(String fileName)
        {

            // Find the specific OPC UA method ("ImportConfig") from a collection of methods.
            var methodImportConfig = ConnectionManager._methodCollection.First(n => n.DisplayName.ToString().Equals(ViCellBlu.BrowseNames.ImportConfig)); //"ImportConfig"
            var methodNodeImportConfig = ExpandedNodeId.ToNodeId(methodImportConfig.NodeId, ConnectionManager._opcSession.NamespaceUris);

            Console.WriteLine($"Calling method: {methodImportConfig.DisplayName}");

            var callResult = new ViCellBlu.VcbResult { ErrorLevel = ErrorLevelEnum.Warning, MethodResult = MethodResultEnum.Failure };

            if (!File.Exists(fileName))
            {
                callResult.ErrorLevel = ErrorLevelEnum.Error;
                callResult.MethodResult = MethodResultEnum.Failure;
                callResult.ResponseDescription = "Import configuration file does not exist.";
                return;
            }

            var fileContents = File.ReadAllBytes(fileName);

            // Prepare method call request (no input arguments for ImportConfig)
            var reqHeaderImportConfig = new RequestHeader();
            var cmRequestImportConfig = new CallMethodRequest
            {
                ObjectId = ConnectionManager._parentMethodNode,
                MethodId = methodNodeImportConfig,
                InputArguments = new VariantCollection { new Variant(fileContents) } // ImportConfig has no input arguments
            };
            var cmReqCollectionImportConfig = new CallMethodRequestCollection { cmRequestImportConfig };

            // Show what is being sent
            Console.WriteLine("Sending:");
            Console.WriteLine($"  Command: {methodImportConfig.DisplayName}");
            //Console.WriteLine($"  ObjectId: {_parentMethodNode}"); // Optional: show Object and Method IDs
            //Console.WriteLine($"  MethodId: {methodNodeImportConfig}");

            // Call the method
            ConnectionManager._opcSession.Call(reqHeaderImportConfig, cmReqCollectionImportConfig, out var resultCollectionImportConfig, out var diagResultsImportConfig);

            // Show output
            var result = resultCollectionImportConfig[0];
            Console.WriteLine("Method call result status: " + result.StatusCode);

            // decoding the output if available
            if ((resultCollectionImportConfig.Count > 0) && (resultCollectionImportConfig[0].OutputArguments.Count > 0))
            {
                Console.WriteLine("Decoding method output...");
                // Use the specific decode method for ImportConfig results
                DecodeRaw(resultCollectionImportConfig[0].OutputArguments[0].Value,ConnectionManager._opcSession.MessageContext);
            }
            else
            {
                Console.WriteLine("No output values returned by the method.");
                // Still check diagnostic results if the call itself had issues
                if (diagResultsImportConfig != null && diagResultsImportConfig.Count > 0)
                {
                    Console.WriteLine($"Diagnostic result: {diagResultsImportConfig.ToString()}");
                }
            }
        }

        /// <summary>
        /// Decodes the raw byte[] output from the ImportConfig method into a specific ViCellBlu result structure.
        /// Assumes the output is an ExtensionObject containing binary data decodeable as a ViCellBlu.VcbResult
        /// followed by the specific LockStateEnum.
        /// </summary>
        public static ViCellBlu.VcbResult DecodeRaw(Object rawResult, ServiceMessageContext messageContext)
        {
            ViCellBlu.VcbResult callResult = new ViCellBlu.VcbResult() { ErrorLevel = ErrorLevelEnum.Warning, MethodResult = MethodResultEnum.Failure };
            callResult.ErrorLevel = ErrorLevelEnum.Warning;
            callResult.MethodResult = MethodResultEnum.Failure;
            callResult.ResponseDescription = "Decoding raw result ...";
            try
            {
                byte[] myData;
                var val = (Opc.Ua.ExtensionObject)rawResult;
                myData = (byte[])val.Body;
                callResult.Decode(new Opc.Ua.BinaryDecoder(myData, 0, myData.Count(), messageContext));

                Console.WriteLine("Decoded VcbResult:");
                Console.WriteLine("  ErrorLevel: " + callResult.ErrorLevel);
                Console.WriteLine("  MethodResult: " + callResult.MethodResult);
                Console.WriteLine("  ResponseDescription: " + callResult.ResponseDescription);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excption: {ex.ToString()}");
            }
            return callResult;
        }
    }
}

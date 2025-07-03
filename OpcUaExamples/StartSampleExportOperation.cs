using Opc.Ua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OPCUAExamples
{
    public class StartSampleExportOperation
    {
        /// <summary>
        /// Calls the OPC UA StartExport method to initiate the export of sample results for specified UUIDs.
        /// The exported files are saved to a predefined directory.
        /// </summary>
        /// <param name="getUUID">A comma-separated string of UUIDs for the samples to export.</param>
        public void CallRetrieveSampleExport(string getUUID)
        {
            // Initialize a result object specific to the StartExport operation with default failure status.
            var callResult = new ViCellBlu.VcbResultStartExport { ErrorLevel = ViCellBlu.ErrorLevelEnum.Warning, MethodResult = ViCellBlu.MethodResultEnum.Failure, ExportDataId = string.Empty };

            // Find the specific OPC UA method ("StartExport") from a collection of methods.
            var method = ConnectionManager._methodCollection.First(n => n.DisplayName.ToString().Equals(ViCellBlu.BrowseNames.StartExport));      // "StartExport"
            // Convert the ExpandedNodeId of the method to a NodeId usable by the session.
            var methodNodeRetrieveSampleExport = ExpandedNodeId.ToNodeId(method.NodeId, ConnectionManager._opcSession.NamespaceUris);

            // Split the input UUID string by comma into an array.
            var paramArray = getUUID.Split(',');
            // Define the local folder path where exported files are expected to be saved by the instrument.
            var folderPath = $"C:\\Instrument\\Export";
            // Convert the UUID strings into a list of Uuid objects.
            var uuids = paramArray.Select(t => new Uuid(t)).ToList();

            // Print the input UUIDs to the console for verification.
            Console.WriteLine("Input Arguments");
            Console.WriteLine("uuids: " + string.Join(", ", uuids));

            // Check if the target export directory exists and create it if it doesn't.
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Create a new RequestHeader for the OPC UA call.
            var reqHeader = new RequestHeader();
            // Create the CallMethodRequest object.
            var cmRequest = new CallMethodRequest
            {
                ObjectId = ConnectionManager._parentMethodNode, // Node ID of the object that contains the method.
                MethodId = methodNodeRetrieveSampleExport,      // Node ID of the method itself.
                // Add the list of UUIDs as the input argument for the StartExport method.
                InputArguments = new VariantCollection { new Variant(uuids) }
            };
            // Create a collection containing the single call request.
            var cmReqCollection = new CallMethodRequestCollection { cmRequest };           

            // Execute the OPC UA Call method on the session.
            var respHdr = ConnectionManager._opcSession.Call(reqHeader, cmReqCollection, out var resultCollection, out var diagResults);
            // Check if the call was successful and returned output arguments.
            if ((resultCollection.Count > 0) && (resultCollection[0].OutputArguments.Count > 0))
            {
                // Decode the raw result returned by the OPC UA method using a dedicated decoding method.
                callResult = DecodeRawStartExport(resultCollection[0].OutputArguments[0].Value, ConnectionManager._opcSession.MessageContext);
            }

            // Print a success or failure message based on the method result.
            Console.WriteLine(callResult.MethodResult == ViCellBlu.MethodResultEnum.Success
                ? "StartExport Success.\n"
                : "StartExport Failure.\n");

            // Inform the user that the code is attempting to open the export folder.
            Console.WriteLine("\nNavigating to folder path." + folderPath);
            // Open the export directory in the file explorer. This might require user interaction depending on OS settings.
            System.Diagnostics.Process.Start("explorer.exe", folderPath);
            // Pause execution briefly.
            Thread.Sleep(5000); 
            // Indicate that the StartExport call sequence has completed.
            Console.WriteLine("\nStartExport call completed.");
        }


        /// <summary>
        /// Decodes the raw result received from the OPC UA StartExport method call into a specific result object.
        /// </summary>
        /// <param name="rawResult">The raw result object returned by the OPC UA method, expected to be an ExtensionObject.</param>
        /// <param name="messageContext">The service message context required for decoding.</param>
        /// <returns>A ViCellBlu.VcbResultStartExport object containing the decoded result information.</returns>
        public static ViCellBlu.VcbResultStartExport DecodeRawStartExport(object rawResult, ServiceMessageContext messageContext)
        {
            // Initialize the result object with default failure status.
            var callResult = new ViCellBlu.VcbResultStartExport
            {
                ErrorLevel = ViCellBlu.ErrorLevelEnum.Warning,
                MethodResult = ViCellBlu.MethodResultEnum.Failure
            };

            // Set an initial response description indicating the decoding process is starting.
            callResult.ResponseDescription = "Decoding raw result ...";
            try
            {
                // Cast the raw result to an ExtensionObject, which is expected for complex types in OPC UA.
                var val = (Opc.Ua.ExtensionObject)rawResult;
                // Get the binary data from the ExtensionObject body. This binary data contains the encoded result.
                var myData = (byte[])val.Body;
                // Create a BinaryDecoder using the retrieved binary data and the message context.
                // Then, call the Decode method on the result object to populate it from the binary data.
                callResult.Decode(new Opc.Ua.BinaryDecoder(myData, 0, myData.Count(), messageContext));

                Console.WriteLine("Decoded VcbResult:");
                Console.WriteLine("  ErrorLevel: " + callResult.ErrorLevel);
                Console.WriteLine("  MethodResult: " + callResult.MethodResult);
                Console.WriteLine("  ResponseDescription: " + callResult.ResponseDescription);
            }
            catch (Exception ex)
            {
                // Catch any exceptions that occur during the decoding process.
                // Update the result object to reflect the error level and method failure.
                callResult.ErrorLevel = ViCellBlu.ErrorLevelEnum.RequiresUserInteraction;
                callResult.MethodResult = ViCellBlu.MethodResultEnum.Failure;
                // Set the response description to include the exception details.
                callResult.ResponseDescription = "DecodeRaw-Exception: " + ex.ToString();
            }
            // Return the decoded or error-populated result object.
            return callResult;
        }

    }
}

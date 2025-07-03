using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViCellBlu;

namespace OPCUAExamples
{
    public class GetAvailableDiskSpace
    {
        /// <summary>
        /// Configures and calls the "GetAvailableDiskSpace" OPC UA method.
        /// This method sets up the sample configuration, finds the method node,
        /// prepares the OPC UA CallMethodRequest, sends it to the server,
        /// and decodes the response.
        /// </summary>
        public void CallGetAvailableDiskSpace()
        {
            // Initialize a default VcbResult object to hold the decoded method output.
            // Defaulting to Warning/Failure before the actual call result is decoded.
            VcbResultGetDiskSpace callResult = new VcbResultGetDiskSpace() { ErrorLevel = ErrorLevelEnum.Warning, MethodResult = MethodResultEnum.Failure };

            // --- OPC UA Method Call Preparation ---
            // Find the reference description for the "GetAvailableDiskSpace" method from the pre-browsed collection.
            // Assumes ConnectionManager._methodCollectionPlayControl holds method nodes for "PlayControl".
            // ViCellBlu.BrowseNames.GetAvailableDiskSpace provides the string literal "GetAvailableDiskSpace".
            var methodGetAvailableDiskSpace = ConnectionManager._methodCollection.First(n => n.DisplayName.ToString().Equals(ViCellBlu.BrowseNames.GetAvailableDiskSpace)); //"GetAvailableDiskSpace"
                                                                                                                                                                        // Convert the method's ExpandedNodeId (from browsing) to a Session-specific NodeId.
            var methodNodeGetAvailableDiskSpace = ExpandedNodeId.ToNodeId(methodGetAvailableDiskSpace.NodeId, ConnectionManager._opcSession.NamespaceUris);

            Console.WriteLine($"Preparing to call method: {methodGetAvailableDiskSpace.DisplayName.Text}");

            // Create a RequestHeader (often default for simple calls).
            var reqHeaderGetAvailableDiskSpace = new RequestHeader();
            // Create the CallMethodRequest object.
            var cmRequestGetAvailableDiskSpace = new CallMethodRequest
            {
                // ObjectId: The NodeId of the object that the method belongs to (the parent node).
                // Assumes ConnectionManager._parentPlayNode holds the NodeId for the "PlayControl" object.
                ObjectId = ConnectionManager._parentMethodNode,
                // MethodId: The NodeId of the method itself.
                MethodId = methodNodeGetAvailableDiskSpace,
                // InputArguments: A collection of input arguments for the method.
                // The GetAvailableDiskSpace method expects one input argument: the SampleConfig object.
                // The SampleConfig object needs to be wrapped in a Variant.
                InputArguments = new VariantCollection()
            };
            // Create a collection of CallMethodRequests (usually just one for a single method call).
            var cmReqCollectioncmRequestGetAvailableDiskSpace = new CallMethodRequestCollection { cmRequestGetAvailableDiskSpace };

            // Show what is being sent to the server.
            Console.WriteLine("Sending:");
            Console.WriteLine($"  Command: {methodGetAvailableDiskSpace.DisplayName.Text}");

            // --- Execute the OPC UA Call ---
            // Call the method on the server using the active OPC UA session.
            // This sends the CallMethodRequest(s) and receives the CallMethodResult(s) and any DiagnosticInfo.
            ConnectionManager._opcSession.Call(
                reqHeaderGetAvailableDiskSpace,                  // Request header
                cmReqCollectioncmRequestGetAvailableDiskSpace,   // Collection of methods to call
                out var resultCollectionGetAvailableDiskSpace, // Output: Collection of results from the calls
                out var diagResultsGetAvailableDiskSpace         // Output: Diagnostic information
                );

            // --- Process Results ---
            // Get the result for the first (and only) method call in the collection.
            var result = resultCollectionGetAvailableDiskSpace[0];
            Console.WriteLine("Method call result status: " + result.StatusCode);

            // Check if the method call returned output arguments and attempt to decode the first one.
            if ((resultCollectionGetAvailableDiskSpace.Count > 0) && (resultCollectionGetAvailableDiskSpace[0].OutputArguments.Count > 0))
            {
                Console.WriteLine("Decoding method output...");
                // Call the helper method to decode the raw output value.
                // The output is expected to be a custom ViCellBlu.VcbResult type,
                // which needs special decoding logic (provided in DecodeRaw).
                callResult = DecodeRawDiskSpaceData(resultCollectionGetAvailableDiskSpace[0].OutputArguments[0].Value, ConnectionManager._opcSession.MessageContext);

                // Check if the method result indicates success.
                if (callResult.MethodResult == ViCellBlu.MethodResultEnum.Success)
                {
                    int totalLineWidth = 80;
                    Console.WriteLine(new string('-', totalLineWidth));
                    Console.WriteLine("Output arguments GetAvailableDiskSpaces call"); // Changed title slightly
                    Console.WriteLine(new string('-', totalLineWidth));
                    Console.WriteLine(string.Format("{0,-30} {1,-5} {2,-40}", "Name", "Type", "Value"));
                    Console.WriteLine(new string('-', totalLineWidth));

                    Console.WriteLine(string.Format("{0,-30} [{1,-5}]: {2,-40}", "DiskSpaceDataBytes", callResult.DiskSpaceDataBytes.GetType().Name, callResult.DiskSpaceDataBytes));
                    Console.WriteLine(string.Format("{0,-30} [{1,-5}]: {2,-40}", "DiskSpaceExportBytes", callResult.DiskSpaceExportBytes.GetType().Name, callResult.DiskSpaceExportBytes));
                    Console.WriteLine(string.Format("{0,-30} [{1,-5}]: {2,-40}", "DiskSpaceOtherBytes", callResult.DiskSpaceOtherBytes.GetType().Name, callResult.DiskSpaceOtherBytes));
                    Console.WriteLine(string.Format("{0,-30} [{1,-5}]: {2,-40}", "TotalFreeBytes", callResult.TotalFreeBytes.GetType().Name, callResult.TotalFreeBytes));
                    Console.WriteLine(string.Format("{0,-30} [{1,-5}]: {2,-40}", "TotalSizeBytes", callResult.TotalSizeBytes.GetType().Name, callResult.TotalSizeBytes));
                }
                else
                {
                    // Handle cases where the method call succeeded at the OPC UA level (StatusCode is Good)
                    // but returned no output arguments (which shouldn't happen for GetAvailableDiskSpace if successful).
                    Console.WriteLine("No output values returned by the method.");
                    // Still check and display diagnostic results if the OPC UA call had issues.
                    if (diagResultsGetAvailableDiskSpace != null && diagResultsGetAvailableDiskSpace.Count > 0)
                    {
                        Console.WriteLine($"Diagnostic result: {diagResultsGetAvailableDiskSpace.ToString()}");
                    }
                }

                // Print the final result status based on the decoded VcbResult.
                Console.WriteLine(callResult.MethodResult == ViCellBlu.MethodResultEnum.Success
                    ? "GetAvailableDiskSpace command Success.\n" // If decoded result indicates success
                    : "GetAvailableDiskSpace command Failure.\n"); // If decoded result indicates failure or decoding failed
            }
        }

        /// <summary>
        /// Decodes the raw output value returned by a Vi-Cell BLU OPC UA method call.
        /// Vi-Cell BLU methods often return custom data types (like VcbResult)
        /// encoded as ExtensionObjects containing binary data. This method decodes that data.
        /// </summary>
        /// <param name="rawResult">The raw output value (expected to be an ExtensionObject).</param>
        /// <param name="messageContext">The service message context from the session, required for decoding custom types.</param>
        /// <returns>A decoded ViCellBlu.VcbResult object.</returns>
        public static VcbResultGetDiskSpace DecodeRawDiskSpaceData(Object rawResult, ServiceMessageContext messageContext)
        {
            VcbResultGetDiskSpace callResult = new VcbResultGetDiskSpace();
            try
            {
                byte[] myData;
                var val = (Opc.Ua.ExtensionObject)rawResult;
                myData = (byte[])val.Body;
                callResult.Decode(new Opc.Ua.BinaryDecoder(myData, 0, myData.Count(), messageContext));

                // Print decoded VcbResult properties to the console.
                Console.WriteLine("Decoded VcbResult:");
                Console.WriteLine("  ErrorLevel: " + callResult.ErrorLevel);
                Console.WriteLine("  MethodResult: " + callResult.MethodResult);
                Console.WriteLine("  ResponseDescription: " + callResult.ResponseDescription);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.ToString()}");
            }
            return callResult;
        }
    }
}

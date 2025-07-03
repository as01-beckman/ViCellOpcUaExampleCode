using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViCellBlu;

namespace OPCUAExamples
{
    public class GetQualityControl
    {
        /// <summary>
        /// Configures and calls the "GetQualityControl" OPC UA method.
        /// This method sets up the sample configuration, finds the method node,
        /// prepares the OPC UA CallMethodRequest, sends it to the server,
        /// and decodes the response.
        /// </summary>
        public void CallGetQualityControl()
        {
            // Initialize a default VcbResult object to hold the decoded method output.
            // Defaulting to Warning/Failure before the actual call result is decoded.
            VcbResultGetQualityControls callResult = new VcbResultGetQualityControls() { ErrorLevel = ErrorLevelEnum.Warning, MethodResult = MethodResultEnum.Failure };

            // --- OPC UA Method Call Preparation ---
            // Find the reference description for the "GetQualityControl" method from the pre-browsed collection.
            // Assumes ConnectionManager._methodCollectionPlayControl holds method nodes for "PlayControl".
            // ViCellBlu.BrowseNames.GetQualityControl provides the string literal "GetQualityControl".
            var methodGetQualityControl = ConnectionManager._methodCollection.First(n => n.DisplayName.ToString().Equals(ViCellBlu.BrowseNames.GetQualityControls)); //"GetQualityControls"
                                                                                                                                                                        // Convert the method's ExpandedNodeId (from browsing) to a Session-specific NodeId.
            var methodNodeGetQualityControl = ExpandedNodeId.ToNodeId(methodGetQualityControl.NodeId, ConnectionManager._opcSession.NamespaceUris);

            Console.WriteLine($"Preparing to call method: {methodGetQualityControl.DisplayName.Text}");

            // Create a RequestHeader (often default for simple calls).
            var reqHeaderGetQualityControl = new RequestHeader();
            // Create the CallMethodRequest object.
            var cmRequestGetQualityControl = new CallMethodRequest
            {
                // ObjectId: The NodeId of the object that the method belongs to (the parent node).
                // Assumes ConnectionManager._parentPlayNode holds the NodeId for the "PlayControl" object.
                ObjectId = ConnectionManager._parentMethodNode,
                // MethodId: The NodeId of the method itself.
                MethodId = methodNodeGetQualityControl,
                // InputArguments: A collection of input arguments for the method.
                // The GetQualityControl method expects one input argument: the SampleConfig object.
                // The SampleConfig object needs to be wrapped in a Variant.
                InputArguments = new VariantCollection()
            };
            // Create a collection of CallMethodRequests (usually just one for a single method call).
            var cmReqCollectioncmRequestGetQualityControl = new CallMethodRequestCollection { cmRequestGetQualityControl };

            // Show what is being sent to the server.
            Console.WriteLine("Sending:");
            Console.WriteLine($"  Command: {methodGetQualityControl.DisplayName.Text}");

            // --- Execute the OPC UA Call ---
            // Call the method on the server using the active OPC UA session.
            // This sends the CallMethodRequest(s) and receives the CallMethodResult(s) and any DiagnosticInfo.
            ConnectionManager._opcSession.Call(
                reqHeaderGetQualityControl,                  // Request header
                cmReqCollectioncmRequestGetQualityControl,   // Collection of methods to call
                out var resultCollectionGetQualityControl, // Output: Collection of results from the calls
                out var diagResultsGetQualityControl         // Output: Diagnostic information
                );

            // --- Process Results ---
            // Get the result for the first (and only) method call in the collection.
            var result = resultCollectionGetQualityControl[0];
            Console.WriteLine("Method call result status: " + result.StatusCode);

            // Check if the method call returned output arguments and attempt to decode the first one.
            if ((resultCollectionGetQualityControl.Count > 0) && (resultCollectionGetQualityControl[0].OutputArguments.Count > 0))
            {
                Console.WriteLine("Decoding method output...");
                // Call the helper method to decode the raw output value.
                // The output is expected to be a custom ViCellBlu.VcbResult type,
                // which needs special decoding logic (provided in DecodeRaw).
                callResult = DecodeRawQcData(resultCollectionGetQualityControl[0].OutputArguments[0].Value, ConnectionManager._opcSession.MessageContext);


                // Check if the method result indicates success.
                if (callResult.MethodResult == ViCellBlu.MethodResultEnum.Success)
                {
                    int totalLineWidth = 80;
                    Console.WriteLine(new string('-', totalLineWidth));
                    Console.WriteLine("Output arguments GetQualityControls call"); // Changed title slightly
                    Console.WriteLine(new string('-', totalLineWidth));
                    // Format: Property Name      : Value
                    Console.WriteLine(string.Format("{0,-25} : {1,-30}", "Property Name", "Value"));
                    Console.WriteLine(new string('-', totalLineWidth));
                    // Retrieve the decoded sample results.3
                    var results = callResult.QualityControls;
                    
                    foreach (var res in results)
                    {   
                        // Print each row using 'res'
                        Console.WriteLine(string.Format("{0,-25} : {1,-30}", "Cell Type Name", res.CellTypeName));
                        Console.WriteLine(string.Format("{0,-25} : {1,-30}", "Acceptance Limits", res.AcceptanceLimits));
                        Console.WriteLine(string.Format("{0,-25} : {1,-30}", "Lot Number", res.LotNumber));
                        Console.WriteLine(string.Format("{0,-25} : {1,-30}", "Assay Parameter", res.AssayParameter)); // Uses the calculated enum range
                        Console.WriteLine(string.Format("{0,-25} : {1,-30}", "Quality Control Name", res.QualityControlName)); // Using "" based on your previous example                        
                        Console.WriteLine(string.Format("{0,-25} : {1,-30}", "Expiration Date", res.ExpirationDate));
                        Console.WriteLine(string.Format("{0,-25} : {1,-30}", "Comments", res.Comments));
                        Console.WriteLine(new string('-', totalLineWidth));
                    }
                }
                else
                {
                    // Handle cases where the method call succeeded at the OPC UA level (StatusCode is Good)
                    // but returned no output arguments (which shouldn't happen for GetQualityControl if successful).
                    Console.WriteLine("No output values returned by the method.");
                    // Still check and display diagnostic results if the OPC UA call had issues.
                    if (diagResultsGetQualityControl != null && diagResultsGetQualityControl.Count > 0)
                    {
                        Console.WriteLine($"Diagnostic result: {diagResultsGetQualityControl.ToString()}");
                    }
                }

                // Print the final result status based on the decoded VcbResult.
                Console.WriteLine(callResult.MethodResult == ViCellBlu.MethodResultEnum.Success
                    ? "GetQualityControl command Success.\n" // If decoded result indicates success
                    : "GetQualityControl command Failure.\n"); // If decoded result indicates failure or decoding failed
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
        public static VcbResultGetQualityControls DecodeRawQcData(Object rawResult, ServiceMessageContext messageContext)
        {
            VcbResultGetQualityControls callResult = new VcbResultGetQualityControls() { ErrorLevel = ErrorLevelEnum.Warning, MethodResult = MethodResultEnum.Failure };
            callResult.ErrorLevel = ErrorLevelEnum.Warning;
            callResult.MethodResult = MethodResultEnum.Failure;
            callResult.ResponseDescription = "Decoding raw result ...";
            try
            {
                // Cast the raw result to an ExtensionObject.
                var val = (Opc.Ua.ExtensionObject)rawResult;
                // Get the binary data from the ExtensionObject body.
                var myData = (byte[])val.Body;
                // Create a BinaryDecoder and decode the binary data into the callResult object.
                callResult.Decode(new Opc.Ua.BinaryDecoder(myData, 0, myData.Count(), messageContext));
                // Print decoded VcbResult properties to the console.
                Console.WriteLine("Decoded VcbResult:");
                Console.WriteLine("  ErrorLevel: " + callResult.ErrorLevel);
                Console.WriteLine("  MethodResult: " + callResult.MethodResult);
                Console.WriteLine("  ResponseDescription: " + callResult.ResponseDescription);
            }
            catch (Exception ex)
            {
                // Catch any exceptions during decoding and update the result object with error information.
                callResult.ErrorLevel = ViCellBlu.ErrorLevelEnum.RequiresUserInteraction;
                callResult.MethodResult = ViCellBlu.MethodResultEnum.Failure;
                callResult.ResponseDescription = "DecodeRaw-Exception: " + ex.ToString();
            }
            // Return the decoded or error-populated result object.
            return callResult;
        }
    }
}

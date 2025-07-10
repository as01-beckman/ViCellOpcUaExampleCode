using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViCellBlu;

namespace OPCUAExamples
{
    public class CreateCellTypeOperation
    {
            /// <summary>
            /// Configures and calls the "CreateCellType" OPC UA method.
            /// This method sets up the sample configuration, finds the method node,
            /// prepares the OPC UA CallMethodRequest, sends it to the server,
            /// and decodes the response.
            /// </summary>
            public void CallCreateCellType()
            {

            //create cell type
            CellType cell = new CellType();
            cell.CellTypeName = $"CT_{DateTime.Now:MMddHHmmssfff}"; 
            cell.ConcentrationAdjustmentFactor = (float)-10.00;
            cell.NumAspirationCycles = (int)5;
            cell.DeclusterDegree = DeclusterDegreeEnum.Medium;
            cell.MaxDiameter = (double)20.00;
            cell.MinDiameter = (double)10.00;
            cell.NumMixingCycles = (int)5;
            cell.NumImages = (int)50;
            cell.CellSharpness = (float)10.00;
            cell.MinCircularity = (double)0.50;
            cell.ViableSpotArea = (float)10.00;
            cell.ViableSpotBrightness = (float)10.00;

            Console.WriteLine("---New Cell Type Details ---");
            Console.WriteLine($"Cell Type Name (string): {cell.CellTypeName} (Less than 50 characters)"); 
            Console.WriteLine($"Concentration Adjustment Factor (float): {cell.ConcentrationAdjustmentFactor} (Range: -20.0 to 20.0)");
            Console.WriteLine($"Number of Aspiration Cycles (int): {cell.NumAspirationCycles} (Range: 1 to 10)");
            Console.WriteLine($"Decluster Degree (enum): {cell.DeclusterDegree} (Options: None, Low, Medium, High)");
            Console.WriteLine($"Maximum Diameter (double): {cell.MaxDiameter} (Range: 1.00 to 60.00)");
            Console.WriteLine($"Minimum Diameter (double): {cell.MinDiameter} (Range: 1.00 to 60.00)");
            Console.WriteLine($"Number of Mixing Cycles (int): {cell.NumMixingCycles} (Range: 1 to 10)");
            Console.WriteLine($"Number of Images (int): {cell.NumImages} (Range: 10 to 100)");
            Console.WriteLine($"Cell Sharpness (float): {cell.CellSharpness} (Range: 0.0 to 100.00)");
            Console.WriteLine($"Minimum Circularity (double): {cell.MinCircularity} (Range: 0.00 to 1.00)");
            Console.WriteLine($"Viable Spot Area (float): {cell.ViableSpotArea} (Range: 0.00 to 95.00)");
            Console.WriteLine($"Viable Spot Brightness (float): {cell.ViableSpotBrightness} (Range: 0.00 to 95.00)");

            // Initialize a default VcbResult object to hold the decoded method output.
            // Defaulting to Warning/Failure before the actual call result is decoded.
            var callResult = new ViCellBlu.VcbResult
                {
                    ErrorLevel = ViCellBlu.ErrorLevelEnum.Warning,
                    MethodResult = ViCellBlu.MethodResultEnum.Failure
                };

                // --- OPC UA Method Call Preparation ---
                // Find the reference description for the "CreateCellType" method from the pre-browsed collection.
                // Assumes ConnectionManager._methodCollectionPlayControl holds method nodes for "PlayControl".
                // ViCellBlu.BrowseNames.CreateCellType provides the string literal "CreateCellType".
                var methodCreateCellType = ConnectionManager._methodCollection.First(n => n.DisplayName.ToString().Equals(ViCellBlu.BrowseNames.CreateCellType)); //"CreateCellType"
                // Convert the method's ExpandedNodeId (from browsing) to a Session-specific NodeId.
                var methodNodeCreateCellType = ExpandedNodeId.ToNodeId(methodCreateCellType.NodeId, ConnectionManager._opcSession.NamespaceUris);

                Console.WriteLine($"Preparing to call method: {methodCreateCellType.DisplayName.Text}");

                // Create a RequestHeader (often default for simple calls).
                var reqHeaderCreateCellType = new RequestHeader();
                // Create the CallMethodRequest object.
                var cmRequestCreateCellType = new CallMethodRequest
                {
                    // ObjectId: The NodeId of the object that the method belongs to (the parent node).
                    // Assumes ConnectionManager._parentPlayNode holds the NodeId for the "PlayControl" object.
                    ObjectId = ConnectionManager._parentMethodNode,
                    // MethodId: The NodeId of the method itself.
                    MethodId = methodNodeCreateCellType,
                    // InputArguments: A collection of input arguments for the method.
                    // The CreateCellType method expects one input argument: the SampleConfig object.
                    // The SampleConfig object needs to be wrapped in a Variant.
                    InputArguments = new VariantCollection { new Variant(cell) }
                };
                // Create a collection of CallMethodRequests (usually just one for a single method call).
                var cmReqCollectioncmRequestCreateCellType = new CallMethodRequestCollection { cmRequestCreateCellType };

                // Show what is being sent to the server.
                Console.WriteLine("Sending:");
                Console.WriteLine($"  Command: {methodCreateCellType.DisplayName.Text}");

                // --- Execute the OPC UA Call ---
                // Call the method on the server using the active OPC UA session.
                // This sends the CallMethodRequest(s) and receives the CallMethodResult(s) and any DiagnosticInfo.
                ConnectionManager._opcSession.Call(
                    reqHeaderCreateCellType,                  // Request header
                    cmReqCollectioncmRequestCreateCellType,   // Collection of methods to call
                    out var resultCollectionCreateCellType, // Output: Collection of results from the calls
                    out var diagResultsCreateCellType         // Output: Diagnostic information
                    );

                // --- Process Results ---
                // Get the result for the first (and only) method call in the collection.
                var result = resultCollectionCreateCellType[0];
                Console.WriteLine("Method call result status: " + result.StatusCode);

                // Check if the method call returned output arguments and attempt to decode the first one.
                if ((resultCollectionCreateCellType.Count > 0) && (resultCollectionCreateCellType[0].OutputArguments.Count > 0))
                {
                    Console.WriteLine("Decoding method output...");
                    // Call the helper method to decode the raw output value.
                    // The output is expected to be a custom ViCellBlu.VcbResult type,
                    // which needs special decoding logic (provided in DecodeRaw).
                    callResult = DecodeRaw(
                        resultCollectionCreateCellType[0].OutputArguments[0].Value, // The raw output value
                        (ServiceMessageContext)ConnectionManager._opcSession.MessageContext // Needed for decoding custom types
                        );
                }
                else
                {
                    // Handle cases where the method call succeeded at the OPC UA level (StatusCode is Good)
                    // but returned no output arguments (which shouldn't happen for CreateCellType if successful).
                    Console.WriteLine("No output values returned by the method.");
                    // Still check and display diagnostic results if the OPC UA call had issues.
                    if (diagResultsCreateCellType != null && diagResultsCreateCellType.Count > 0)
                    {
                        Console.WriteLine($"Diagnostic result: {diagResultsCreateCellType.ToString()}");
                    }
                }

                // Print the final result status based on the decoded VcbResult.
                Console.WriteLine(callResult.MethodResult == ViCellBlu.MethodResultEnum.Success
                    ? "CreateCellType command Success.\n" // If decoded result indicates success
                    : "CreateCellType command Failure.\n"); // If decoded result indicates failure or decoding failed
            }

            /// <summary>
            /// Decodes the raw output value returned by a Vi-Cell BLU OPC UA method call.
            /// Vi-Cell BLU methods often return custom data types (like VcbResult)
            /// encoded as ExtensionObjects containing binary data. This method decodes that data.
            /// </summary>
            /// <param name="rawResult">The raw output value (expected to be an ExtensionObject).</param>
            /// <param name="messageContext">The service message context from the session, required for decoding custom types.</param>
            /// <returns>A decoded ViCellBlu.VcbResult object.</returns>
            public ViCellBlu.VcbResult DecodeRaw(object rawResult, ServiceMessageContext messageContext)
            {
                // Initialize a default VcbResult in case decoding fails.
                var callResult = new ViCellBlu.VcbResult
                {
                    ErrorLevel = ViCellBlu.ErrorLevelEnum.Warning,
                    MethodResult = ViCellBlu.MethodResultEnum.Failure
                };

                callResult.ResponseDescription = "Decoding raw result ..."; // Initial status message

                try
                {
                    // The raw result is expected to be an OPC UA ExtensionObject.
                    // The actual custom data is stored in the 'Body' of the ExtensionObject,
                    // typically as a byte array for binary encoding.
                    if (!(rawResult is Opc.Ua.ExtensionObject val) || !(val.Body is byte[] myData))
                    {
                        callResult.ErrorLevel = ViCellBlu.ErrorLevelEnum.RequiresUserInteraction;
                        callResult.MethodResult = ViCellBlu.MethodResultEnum.Failure;
                        callResult.ResponseDescription = "DecodeRaw Error: Raw result is not an ExtensionObject with a byte array body.";
                        Console.WriteLine(callResult.ResponseDescription); // Log error
                        return callResult; // Return initialized failure result
                    }

                    // Create a BinaryDecoder to read the custom data from the byte array.
                    // The messageContext is crucial as it contains the encoding details and namespace table
                    // needed to understand the structure and types within the binary data.
                    var decoder = new Opc.Ua.BinaryDecoder(myData, 0, myData.Count(), messageContext);

                    // Decode the binary data according to the structure of the ViCellBlu.VcbResult class.
                    // Assumes VcbResult has a public Decode method that takes an IDecoder.
                    callResult.Decode(decoder);

                    // Print the decoded output values for verification.
                    Console.WriteLine("Decoded VcbResult:");
                    Console.WriteLine("  ErrorLevel: " + callResult.ErrorLevel);
                    Console.WriteLine("  MethodResult: " + callResult.MethodResult);
                    Console.WriteLine("  ResponseDescription: " + callResult.ResponseDescription);

                }
                catch (Exception ex)
                {
                    // Catch any exceptions during the decoding process.
                    callResult.ErrorLevel = ViCellBlu.ErrorLevelEnum.RequiresUserInteraction;
                    callResult.MethodResult = ViCellBlu.MethodResultEnum.Failure;
                    callResult.ResponseDescription = "DecodeRaw-Exception: " + ex.ToString(); // Include exception details
                    Console.WriteLine($"DecodeRaw Exception: {ex.Message}"); // Log the exception
                }
                return callResult; // Return the decoded or failure result
            }       
    }
}

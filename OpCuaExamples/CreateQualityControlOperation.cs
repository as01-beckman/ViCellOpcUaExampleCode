using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViCellBlu;

namespace OPCUAExamples
{
    public class CreateQualityControlOperation
    {
        /// <summary>
        /// Configures and calls the "CreateQualityControl" OPC UA method.
        /// This method sets up the sample configuration, finds the method node,
        /// prepares the OPC UA CallMethodRequest, sends it to the server,
        /// and decodes the response.
        /// </summary>
        public void CallQualityControl()
        {

            //create quality control
            QualityControl qc = new QualityControl();
            qc.QualityControlName = "QCFromOpcuaExample10";
            qc.CellTypeName = "Yeast";
            qc.AcceptanceLimits = 100;
            qc.AssayParameter = AssayParameterEnum.Concentration;
            qc.AssayValue = (double)99999.00;
            qc.Comments = "QC from opcua example";
            qc.ExpirationDate = DateTime.Parse("2025-07-07T00:00:00.000Z");
            qc.LotNumber = "Lot-111";


            // Get the names of the enum members for AssayParameter
            string[] assayParameterNames = Enum.GetNames(typeof(AssayParameterEnum));
            string assayParameterRange = string.Join(", ", assayParameterNames); // Join them with comma and space


            // Write QC information to console in a tabular format
            // Write QC information to console in a tabular format including types and hardcoded dummy ranges
            Console.WriteLine("------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine(" New Quality Control");
            Console.WriteLine("------------------------------------------------------------------------------------------------------------------");
            // Format: Property Name      : Value                         (Type)        Range/Constraints
            Console.WriteLine(string.Format("{0,-25} : {1,-30} {2,-20} {3}", "Property Name", "Value", "Type", "Range/Constraints"));
            Console.WriteLine("------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine(string.Format("{0,-25} : {1,-30} {2,-20} {3}", "Quality Control Name", qc.QualityControlName, qc.QualityControlName.GetType().Name, ""));
            Console.WriteLine(string.Format("{0,-25} : {1,-30} {2,-20} {3}", "Cell Type Name", qc.CellTypeName, qc.CellTypeName.GetType().Name, "From predefined list"));
            Console.WriteLine(string.Format("{0,-25} : {1,-30} {2,-20} {3}", "Acceptance Limits", qc.AcceptanceLimits, qc.AcceptanceLimits.GetType().Name, "1 - 100"));
            Console.WriteLine(string.Format("{0,-25} : {1,-30} {2,-20} {3}", "Assay Parameter", qc.AssayParameter, qc.AssayParameter.GetType().Name, assayParameterRange));
            Console.WriteLine(string.Format("{0,-25} : {1,-30} {2,-20} {3}", "Assay Value", qc.AssayValue, qc.AssayValue.GetType().Name, "concentration -> 0.00- 0.99999,Viability -> 1.00 - 100.00,Avg diameter  1.00 - 22.00 "));
            Console.WriteLine(string.Format("{0,-25} : {1,-30} {2,-20} {3}", "Comments", qc.Comments, qc.Comments.GetType().Name, "Optional, max 200 characters"));
            Console.WriteLine(string.Format("{0,-25} : {1,-30} {2,-20} {3}", "Expiration Date", qc.ExpirationDate, qc.ExpirationDate.GetType().Name, "Format used : 2025-07-07T00:00:00.000Z"));
            Console.WriteLine(string.Format("{0,-25} : {1,-30} {2,-20} {3}", "Lot Number", qc.LotNumber, qc.LotNumber.GetType().Name, "Alphanumeric, max 20 characters"));
            Console.WriteLine("------------------------------------------------------------------------------------------------------------------");

            // Initialize a default VcbResult object to hold the decoded method output.
            // Defaulting to Warning/Failure before the actual call result is decoded.
            var callResult = new ViCellBlu.VcbResult
            {
                ErrorLevel = ViCellBlu.ErrorLevelEnum.Warning,
                MethodResult = ViCellBlu.MethodResultEnum.Failure
            };

            // --- OPC UA Method Call Preparation ---
            // Find the reference description for the "CreateQualityControl" method from the pre-browsed collection.
            // Assumes ConnectionManager._methodCollectionPlayControl holds method nodes for "PlayControl".
            // ViCellBlu.BrowseNames.CreateQualityControl provides the string literal "CreateQualityControl".
            var methodCreateQualityControl = ConnectionManager._methodCollection.First(n => n.DisplayName.ToString().Equals(ViCellBlu.BrowseNames.CreateQualityControl)); //"CreateQualityControl"
                                                                                                                                                                    // Convert the method's ExpandedNodeId (from browsing) to a Session-specific NodeId.
            var methodNodeCreateQualityControl = ExpandedNodeId.ToNodeId(methodCreateQualityControl.NodeId, ConnectionManager._opcSession.NamespaceUris);

            Console.WriteLine($"Preparing to call method: {methodCreateQualityControl.DisplayName.Text}");

            // Create a RequestHeader (often default for simple calls).
            var reqHeaderCreateQualityControl = new RequestHeader();
            // Create the CallMethodRequest object.
            var cmRequestCreateQualityControl = new CallMethodRequest
            {
                // ObjectId: The NodeId of the object that the method belongs to (the parent node).
                // Assumes ConnectionManager._parentPlayNode holds the NodeId for the "PlayControl" object.
                ObjectId = ConnectionManager._parentMethodNode,
                // MethodId: The NodeId of the method itself.
                MethodId = methodNodeCreateQualityControl,
                // InputArguments: A collection of input arguments for the method.
                // The CreateQualityControl method expects one input argument: the SampleConfig object.
                // The SampleConfig object needs to be wrapped in a Variant.
                InputArguments = new VariantCollection { new Variant(qc) }
            };
            // Create a collection of CallMethodRequests (usually just one for a single method call).
            var cmReqCollectioncmRequestCreateQualityControl = new CallMethodRequestCollection { cmRequestCreateQualityControl };

            // Show what is being sent to the server.
            Console.WriteLine("Sending:");
            Console.WriteLine($"  Command: {methodCreateQualityControl.DisplayName.Text}");

            // --- Execute the OPC UA Call ---
            // Call the method on the server using the active OPC UA session.
            // This sends the CallMethodRequest(s) and receives the CallMethodResult(s) and any DiagnosticInfo.
            ConnectionManager._opcSession.Call(
                reqHeaderCreateQualityControl,                  // Request header
                cmReqCollectioncmRequestCreateQualityControl,   // Collection of methods to call
                out var resultCollectionCreateQualityControl, // Output: Collection of results from the calls
                out var diagResultsCreateQualityControl         // Output: Diagnostic information
                );

            // --- Process Results ---
            // Get the result for the first (and only) method call in the collection.
            var result = resultCollectionCreateQualityControl[0];
            Console.WriteLine("Method call result status: " + result.StatusCode);

            // Check if the method call returned output arguments and attempt to decode the first one.
            if ((resultCollectionCreateQualityControl.Count > 0) && (resultCollectionCreateQualityControl[0].OutputArguments.Count > 0))
            {
                Console.WriteLine("Decoding method output...");
                // Call the helper method to decode the raw output value.
                // The output is expected to be a custom ViCellBlu.VcbResult type,
                // which needs special decoding logic (provided in DecodeRaw).
                callResult = DecodeRaw(
                    resultCollectionCreateQualityControl[0].OutputArguments[0].Value, // The raw output value
                    (ServiceMessageContext)ConnectionManager._opcSession.MessageContext // Needed for decoding custom types
                    );
            }
            else
            {
                // Handle cases where the method call succeeded at the OPC UA level (StatusCode is Good)
                // but returned no output arguments (which shouldn't happen for CreateQualityControl if successful).
                Console.WriteLine("No output values returned by the method.");
                // Still check and display diagnostic results if the OPC UA call had issues.
                if (diagResultsCreateQualityControl != null && diagResultsCreateQualityControl.Count > 0)
                {
                    Console.WriteLine($"Diagnostic result: {diagResultsCreateQualityControl.ToString()}");
                }
            }

            // Print the final result status based on the decoded VcbResult.
            Console.WriteLine(callResult.MethodResult == ViCellBlu.MethodResultEnum.Success
                ? "CreateQualityControl command Success.\n" // If decoded result indicates success
                : "CreateQualityControl command Failure.\n"); // If decoded result indicates failure or decoding failed
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

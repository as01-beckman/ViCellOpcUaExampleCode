using Opc.Ua;
using System;
using System.Linq;

namespace OPCUAExamples
{
    public class DeleteSampleResultsOperation
    {
        // Method to call the DeleteSampleResults OPC UA method.
        // It takes the UUID of the sample(s) to delete and a boolean indicating whether to retain results.
        public void CallDeleteSampleResults(string getUUID, bool getRetainResults)
        {
            // Initialize a result object with default failure status.
            var callResult = new ViCellBlu.VcbResult()
            {
                ErrorLevel = ViCellBlu.ErrorLevelEnum.Warning,
                MethodResult = ViCellBlu.MethodResultEnum.Failure
            };

            // Find the specific OPC UA method ("DeleteSampleResults") from a collection of methods.
            var method = ConnectionManager._methodCollection.First(n => n.DisplayName.ToString().Equals(ViCellBlu.BrowseNames.DeleteSampleResults)); //"DeleteSampleResults"
            // Convert the ExpandedNodeId of the method to a NodeId usable by the session.
            var methodNodeDeleteSampleResults = ExpandedNodeId.ToNodeId(method.NodeId, ConnectionManager._opcSession.NamespaceUris);

            // Split the input UUID string by comma into an array.
            var paramArray = getUUID.Split(',');
            // Convert the UUID strings into a list of Uuid objects.
            var uuids = paramArray.Select(t => new Uuid(t)).ToList();
            // Store the boolean value for retaining results.
            bool retainResults = getRetainResults;

            // Display the input UUIDs in the console for verification.
            Console.WriteLine("Input Arguments");
            Console.WriteLine("\nInput UUIDs:");
            // Iterate through the list of UUIDs and print each one.
            foreach (var uuid in uuids)
            {
                Console.WriteLine("  " + uuid.ToString());
            }

            // Display the input retainResults flag in the console.
            Console.WriteLine("\nInput retainResults:" + retainResults.ToString());

            // Create a new RequestHeader for the OPC UA call.
            var reqHeader = new RequestHeader();
            // Create the CallMethodRequest object.
            var cmRequest = new CallMethodRequest
            {
                ObjectId = ConnectionManager._parentMethodNode, // Node ID of the object that contains the method.
                MethodId = methodNodeDeleteSampleResults,       // Node ID of the method itself.
                // Add the list of UUIDs as the first input argument.
                InputArguments = new VariantCollection { new Variant(uuids) }
            };
            // Add the boolean 'retainResults' as the second input argument.
            cmRequest.InputArguments.Add(new Variant(retainResults));           // the DeleteSampleResults command requires an additional boolean parameter to specify if the results and a single image should be retained

            // Create a collection containing the single call request.
            var cmReqCollection = new CallMethodRequestCollection { cmRequest };

            // Execute the OPC UA Call method on the session.
            var respHdr = ConnectionManager._opcSession.Call(reqHeader, cmReqCollection, out var resultCollection, out var diagResults);
            // Check if the call was successful and returned output arguments.
            if ((resultCollection.Count > 0) && (resultCollection[0].OutputArguments.Count > 0))
            {
                // Decode the raw result returned by the OPC UA method.
                callResult = DecodeRaw(resultCollection[0].OutputArguments[0].Value, ConnectionManager._opcSession.MessageContext);
            }

            // Print a success or failure message based on the method result.
            Console.WriteLine(callResult.MethodResult == ViCellBlu.MethodResultEnum.Success
                ? "DeleteSampleResults Success.\n"
                : "DeleteSampleResults Failure.\n");

            // Indicate that the call has completed.
            Console.WriteLine("\nDeleteSampleResults call completed.\n");
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
                // The actual custom data is stored in the 'Body' as a byte array.
                if (!(rawResult is Opc.Ua.ExtensionObject val) || !(val.Body is byte[] myData))
                {
                    callResult.ErrorLevel = ViCellBlu.ErrorLevelEnum.RequiresUserInteraction;
                    callResult.MethodResult = ViCellBlu.MethodResultEnum.Failure;
                    callResult.ResponseDescription = "DecodeRaw Error: Raw result is not an ExtensionObject with a byte array body.";
                    Console.WriteLine(callResult.ResponseDescription); // Log error
                    return callResult; // Return initialized failure result
                }

                // Create a BinaryDecoder to read the custom data from the byte array.
                // messageContext is crucial for decoding custom types.
                var decoder = new Opc.Ua.BinaryDecoder(myData, 0, myData.Count(), messageContext);

                // Decode the binary data according to the structure of ViCellBlu.VcbResult.
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

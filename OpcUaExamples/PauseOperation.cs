﻿using Opc.Ua;
using System;
using System.Linq;

namespace OPCUAExamples
{
    internal class PauseOperation
    {
        /// <summary>
        /// Configures and calls the "StartSample" OPC UA method.
        /// This method sets up the sample configuration, finds the method node,
        /// prepares the OPC UA CallMethodRequest, sends it to the server,
        /// and decodes the response.
        /// </summary>
        public void CallPauseOperation()
        {
            // Initialize a default VcbResult object to hold the decoded method output.
            // Defaulting to Warning/Failure before the actual call result is decoded.
            var callResult = new ViCellBlu.VcbResult
            {
                ErrorLevel = ViCellBlu.ErrorLevelEnum.Warning,
                MethodResult = ViCellBlu.MethodResultEnum.Failure
            };

            // --- OPC UA Method Call Preparation ---
            // Find the reference description for the "StartSample" method from the pre-browsed collection.
            // Assumes ConnectionManager._methodCollectionPlayControl holds method nodes for "PlayControl".
            // ViCellBlu.BrowseNames.StartSample provides the string literal "StartSample".
            var methodStartSample = ConnectionManager._methodCollectionPlayControl.First(n => n.DisplayName.ToString().Equals(ViCellBlu.BrowseNames.Pause)); //Pause
            // Convert the method's ExpandedNodeId (from browsing) to a Session-specific NodeId.
            var methodNodeStartSample = ExpandedNodeId.ToNodeId(methodStartSample.NodeId, ConnectionManager._opcSession.NamespaceUris);

            Console.WriteLine($"Preparing to call method: {methodStartSample.DisplayName.Text}");

            // Create a RequestHeader (often default for simple calls).
            var reqHeaderStartSample = new RequestHeader();
            // Create the CallMethodRequest object.
            var cmRequestStartSample = new CallMethodRequest
            {
                // ObjectId: The NodeId of the object that the method belongs to (the parent node).
                // Assumes ConnectionManager._parentPlayNode holds the NodeId for the "PlayControl" object.
                ObjectId = ConnectionManager._parentPlayNode,
                // MethodId: The NodeId of the method itself.
                MethodId = methodNodeStartSample,
                // InputArguments: A collection of input arguments for the method.
                // The StartSample method expects one input argument: the SampleConfig object.
                // The SampleConfig object needs to be wrapped in a Variant.
                InputArguments = new VariantCollection()
            };
            // Create a collection of CallMethodRequests (usually just one for a single method call).
            var cmReqCollectioncmRequestStartSample = new CallMethodRequestCollection { cmRequestStartSample };

            // Show what is being sent to the server.
            Console.WriteLine("Sending:");
            Console.WriteLine($"  Command: {methodStartSample.DisplayName.Text}");

            // --- Execute the OPC UA Call ---
            // Call the method on the server using the active OPC UA session.
            // This sends the CallMethodRequest(s) and receives the CallMethodResult(s) and any DiagnosticInfo.
            ConnectionManager._opcSession.Call(
                reqHeaderStartSample,                  // Request header
                cmReqCollectioncmRequestStartSample,   // Collection of methods to call
                out var resultCollectionStartSample, // Output: Collection of results from the calls
                out var diagResultsStartSample         // Output: Diagnostic information
                );

            // --- Process Results ---
            // Get the result for the first (and only) method call in the collection.
            var result = resultCollectionStartSample[0];
            Console.WriteLine("Method call result status: " + result.StatusCode);

            // Check if the method call returned output arguments and attempt to decode the first one.
            if ((resultCollectionStartSample.Count > 0) && (resultCollectionStartSample[0].OutputArguments.Count > 0))
            {
                Console.WriteLine("Decoding method output...");
                // Call the helper method to decode the raw output value.
                // The output is expected to be a custom ViCellBlu.VcbResult type,
                // which needs special decoding logic (provided in DecodeRaw).
                callResult = DecodeRaw(
                    resultCollectionStartSample[0].OutputArguments[0].Value, // The raw output value
                    (ServiceMessageContext)ConnectionManager._opcSession.MessageContext // Needed for decoding custom types
                    );
            }
            else
            {
                // Handle cases where the method call succeeded at the OPC UA level (StatusCode is Good)
                // but returned no output arguments (which shouldn't happen for StartSample if successful).
                Console.WriteLine("No output values returned by the method.");
                // Still check and display diagnostic results if the OPC UA call had issues.
                if (diagResultsStartSample != null && diagResultsStartSample.Count > 0)
                {
                    Console.WriteLine($"Diagnostic result: {diagResultsStartSample.ToString()}");
                }
            }

            // Print the final result status based on the decoded VcbResult.
            Console.WriteLine(callResult.MethodResult == ViCellBlu.MethodResultEnum.Success
                ? "StartSample command Success.\n" // If decoded result indicates success
                : "StartSample command Failure.\n"); // If decoded result indicates failure or decoding failed
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

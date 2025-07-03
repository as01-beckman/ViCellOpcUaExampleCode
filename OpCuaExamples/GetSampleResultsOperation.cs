using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
// using statements are intentionally not commented as per instruction.

namespace OPCUAExamples
{
    // Class definition for the GetSampleResults operation.
    public class GetSampleResultsOperation
    {
        // Method to initiate the call to the GetSampleResults OPC UA method.
        public void CallGetSampleResults()
        {
            // Initialize a result object with default failure status.
            var callResult = new ViCellBlu.VcbResultGetSampleResults { ErrorLevel = ViCellBlu.ErrorLevelEnum.Warning, MethodResult = ViCellBlu.MethodResultEnum.Failure };

            // Find the specific OPC UA method ("GetSampleResults") from a collection of methods.
            var method = ConnectionManager._methodCollection.First(n => n.DisplayName.ToString().Equals(ViCellBlu.BrowseNames.GetSampleResults));     // "GetSampleResults"
            // Convert the ExpandedNodeId of the method to a NodeId usable by the session.
            var methodNodeGetSampleResults = ExpandedNodeId.ToNodeId(method.NodeId, ConnectionManager._opcSession.NamespaceUris);

            // Call a private helper method to filter results by sample set.
            FilterSampleSet(methodNodeGetSampleResults, callResult);
            // Call a private helper method to filter results by individual sample.
            FilterSample(methodNodeGetSampleResults, callResult);
        }

        // Private helper method to call GetSampleResults filtering by Sample Set.
        private void FilterSampleSet(NodeId methodNodeGetSampleResults, ViCellBlu.VcbResultGetSampleResults callResult)
        {
            // Initialize a list to hold the results.
            var results = new List<ViCellBlu.SampleResult>();

            // Create a new RequestHeader for the OPC UA call.
            var reqHeader = new RequestHeader();
            // Define the input arguments for the GetSampleResults method call.
            var inputArguments = new VariantCollection
                                 {
                                     new Variant(string.Empty),                                  // User name string
                                     new Variant(DateTime.Now.Subtract(TimeSpan.FromDays(30))),  // start date
                                     new Variant(DateTime.Now),                                  // end date
                                     new Variant(0),                                             // filter ON: 0 = sample set, 1 = sample
                                     new Variant("Insect"),                                      // Cell type or QC name
                                     new Variant(string.Empty),                                  // Search string (sample or sample set name)
                                     new Variant(string.Empty)                                   // Search tag string
                                 };

            // Argument names extracted from the comments above
            string[] argumentNames = new[]
            {
                "User name",
                "start date",
                "end date",
                "filter ON [0 = sample set, 1 = sample]",
                "Cell type or QC name",
                "Search string (sample or sample set name)",
                "Search tag string"
            };

            // Print the input arguments to the console for verification.
            Console.WriteLine("Input Arguments For Sample:");
            for (int i = 0; i < inputArguments.Count; i++)
            {
                string argName = (i < argumentNames.Length) ? argumentNames[i] : $"Argument {i}";
                Console.WriteLine($"\t{argName}: {inputArguments[i].Value}");
            }

            // Create the CallMethodRequest object.
            var cmRequest = new CallMethodRequest
            {
                ObjectId = ConnectionManager._parentMethodNode, // Node ID of the object that contains the method.
                MethodId = methodNodeGetSampleResults,          // Node ID of the method itself.
                InputArguments = inputArguments                 // The arguments defined above.
            };

            // Create a collection containing the single call request.
            var cmReqCollection = new CallMethodRequestCollection { cmRequest };
            // Execute the OPC UA Call method on the session.
            var respHdr = ConnectionManager._opcSession.Call(reqHeader, cmReqCollection, out var resultCollection, out var diagResults);

            // Check if the call was successful and returned output arguments.
            if ((resultCollection.Count > 0) && (resultCollection[0].OutputArguments.Count > 0))
            {
                // Decode the raw result returned by the OPC UA method.
                callResult = DecodeRawSampleResults(resultCollection[0].OutputArguments[0].Value, ConnectionManager._opcSession.MessageContext);
            }

            // Check if the method result indicates success.
            if (callResult.MethodResult == ViCellBlu.MethodResultEnum.Success)
            {
                // Retrieve the decoded sample results.
                results = callResult.SampleResults;

                Console.WriteLine("\n======================= sample set result start =======================");
                // Iterate through the results and print their properties.
                int srNo = 1;
                foreach (var result in results)
                {
                    Console.WriteLine("\tSampleSetResult " +srNo);
                    // Use reflection to get all properties of the result object.
                    var properties = result.GetType().GetProperties();
                    // Print each property name and its value.
                    foreach (var prop in properties)
                    {
                        var value = prop.GetValue(result, null);
                        Console.WriteLine($"\t  {prop.Name}: {value}");
                    }
                    srNo++;
                }
                Console.WriteLine("\n======================= sample set result end =======================");
                // Print a newline for separation.
                Console.WriteLine('\n');
            }
            else
            {
                // Print a failure message if the method result was not success.
                Console.WriteLine("GetSampleResults Failure.\n");
            }
        }

        // Private helper method to call GetSampleResults filtering by individual Sample.
        private void FilterSample(NodeId methodNodeGetSampleResults, ViCellBlu.VcbResultGetSampleResults callResult)
        {
            // Initialize a list to hold the results.
            var results = new List<ViCellBlu.SampleResult>();

            // Create a new RequestHeader for the OPC UA call.
            var reqHeader = new RequestHeader();
            // Define the input arguments for the GetSampleResults method call.
            var inputArguments = new VariantCollection
                                 {
                                     new Variant(string.Empty),                                  // User name string
                                     new Variant(DateTime.Now.Subtract(TimeSpan.FromDays(30))),  // start date
                                     new Variant(DateTime.Now),                                  // end date
                                     new Variant(1),                                             // filter ON: 0 = sample set, 1 = sample
                                     new Variant("Insect"),                                      // Cell type or QC name
                                     new Variant(string.Empty),                                  // Search string (sample or sample set name)
                                     new Variant(string.Empty)                                   // Search tag string
                                 };

            // Argument names extracted from the comments above
            string[] argumentNames = new[]
            {
                "User name",
                "start date",
                "end date",
                "filter ON [0 = sample set, 1 = sample]",
                "Cell type or QC name",
                "Search string (sample or sample set name)",
                "Search tag string"
            };

            // Print the input arguments to the console for verification.
            Console.WriteLine("Input Arguments For Sample set:");
            for (int i = 0; i < inputArguments.Count; i++)
            {
                string argName = (i < argumentNames.Length) ? argumentNames[i] : $"Argument {i}";
                Console.WriteLine($"\t{argName}: {inputArguments[i].Value}");
            }

            // Create the CallMethodRequest object.
            var cmRequest = new CallMethodRequest
            {
                ObjectId = ConnectionManager._parentMethodNode, // Node ID of the object that contains the method.
                MethodId = methodNodeGetSampleResults,          // Node ID of the method itself.
                InputArguments = inputArguments                 // The arguments defined above.
            };

            // Create a collection containing the single call request.
            var cmReqCollection = new CallMethodRequestCollection { cmRequest };
            // Execute the OPC UA Call method on the session.
            var respHdr = ConnectionManager._opcSession.Call(reqHeader, cmReqCollection, out var resultCollection, out var diagResults);

            // Check if the call was successful and returned output arguments.
            if ((resultCollection.Count > 0) && (resultCollection[0].OutputArguments.Count > 0))
            {
                // Decode the raw result returned by the OPC UA method.
                callResult = DecodeRawSampleResults(resultCollection[0].OutputArguments[0].Value, ConnectionManager._opcSession.MessageContext);
            }

            // Check if the method result indicates success.
            if (callResult.MethodResult == ViCellBlu.MethodResultEnum.Success)
            {
                // Retrieve the decoded sample results.
                results = callResult.SampleResults;

                Console.WriteLine("\n======================= sample result start =======================");
                // Iterate through the results and print their properties.
                int srNo = 1;
                foreach (var result in results)
                {
                    Console.WriteLine("\tSample Result " + srNo);
                    // Use reflection to get all properties of the result object.
                    var properties = result.GetType().GetProperties();
                    // Print each property name and its value.
                    foreach (var prop in properties)
                    {
                        var value = prop.GetValue(result, null);
                        Console.WriteLine($"\t  {prop.Name}: {value}");                        
                    }
                    srNo++;
                }
                Console.WriteLine("\n======================= sample result end =======================");
                // Print a newline for separation.
                Console.WriteLine('\n');

            }
            else
            {
                // Print a failure message if the method result was not success.
                Console.WriteLine("GetSampleResults Failure.\n");
            }
        }

        // Static method to decode the raw result received from the OPC UA method call.
        public static ViCellBlu.VcbResultGetSampleResults DecodeRawSampleResults(object rawResult, ServiceMessageContext messageContext)
        {
            // Initialize the result object with default failure status.
            var callResult = new ViCellBlu.VcbResultGetSampleResults
            {
                ErrorLevel = ViCellBlu.ErrorLevelEnum.Warning,
                MethodResult = ViCellBlu.MethodResultEnum.Failure
            };

            // Set an initial response description.
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

using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OPCUAExamples
{
    /// <summary>
    /// Class to demonstrate calling the "StartSampleSet" OPC UA method on a Vi-Cell BLU server.
    /// This example is specifically configured for starting an plate samples.
    /// Assumes a ConnectionManager class exists with an active OPC UA session (_opcSession),
    /// method collections (_methodCollectionPlayControl), and parent node information (_parentPlayNode).
    /// </summary>
    public class StartSampleSetOperation
    {
        // Method to configure and call the StartSampleSet OPC UA method.
        public void CallStartSampleSet()
        {
            // Available Default Cell Types:
            // - BCI Default
            // - Mammalian
            // - Insect
            // - Yeast
            // - BCI Viab Beads
            // - BCI L10 Beads

            // Ensure the worklist sample configuration is a new object without a pre-assigned UUID

            // Create a list to hold multiple SampleConfig objects for the worklist.
            List<ViCellBlu.SampleConfig> sampleList = new List<ViCellBlu.SampleConfig>();
            // Add the first sample configuration to the list.
            sampleList.Add(new ViCellBlu.SampleConfig
            {
                SampleName = "PlateSample1FromOpcuaExample",
                SamplePosition = { Column = 1, Row = "A" },                         // 96-Well-Plate (Columns = 1-12, Rows = A-H); cannot use the A-Cup position designator in a worklist started with the StartSampleSet method
                Tag = "PlateSampleTag",
                Dilution = 1,
                CellType = new ViCellBlu.CellType { CellTypeName = "Insect" },      // You must choose between CellType OR QualityControl... if one is valid, the other must be string.Empty.
                QualityControl = { QualityControlName = string.Empty },             // You must choose between CellType OR QualityControl... if one is valid, the other must be string.Empty.
                SaveEveryNthImage = 1,
                WashType = ViCellBlu.WashTypeEnum.NormalWash // Normal or Fast
            });

            // Add the second sample configuration to the list.
            sampleList.Add(new ViCellBlu.SampleConfig
            {
                SampleName = "PlateSample2FromOpcuaExample",
                SamplePosition = { Column = 2, Row = "A" },                         // 96-Well-Plate (Columns = 1-12, Rows = A-H); cannot use the A-Cup position designator in a worklist started with the StartSampleSet method
                Tag = "PlateSampleTag",
                Dilution = 1,
                CellType = new ViCellBlu.CellType { CellTypeName = "Yeast" },       // You must choose between CellType OR QualityControl... if one is valid, the other must be string.Empty.
                QualityControl = { QualityControlName = string.Empty },             // You must choose between CellType OR QualityControl... if one is valid, the other must be string.Empty.
                SaveEveryNthImage = 1,
                WashType = ViCellBlu.WashTypeEnum.NormalWash // Normal or Fast
            });

            // Create a SampleSet object to group the sample configurations into a worklist.
            var sampleSet = new ViCellBlu.SampleSet()
            {
                SampleSetName = "OpcTestSampleSet", // Assign a name for the worklist.
                PlatePrecession = ViCellBlu.PlatePrecessionEnum.RowMajor, // Define the plate processing order.
                Samples = new ViCellBlu.SampleConfigCollection(sampleList.ToArray()) // Add the list of sample configs.
            };

            // Print the sampleList contents in a readable format (similar to GetSampleResultsOperation)
            string[] sampleArgumentNames = new[]
            {
                "SampleName",
                "SamplePosition (Column, Row)",
                "Tag",
                "Dilution",
                "CellType",
                "QualityControl",
                "SaveEveryNthImage",
                "WashType"
            };

            Console.WriteLine("Input Arguments For Sample Set:");
            int sampleIndex = 1;
            foreach (var sample in sampleList)
            {
                Console.WriteLine($"\tSample {sampleIndex}:");
                Console.WriteLine($"\t  {sampleArgumentNames[0]}: {sample.SampleName}");
                Console.WriteLine($"\t  {sampleArgumentNames[1]}: Column {sample.SamplePosition.Column}, Row {sample.SamplePosition.Row}");
                Console.WriteLine($"\t  {sampleArgumentNames[2]}: {sample.Tag}");
                Console.WriteLine($"\t  {sampleArgumentNames[3]}: {sample.Dilution}");
                Console.WriteLine($"\t  {sampleArgumentNames[4]}: {(sample.CellType?.CellTypeName ?? string.Empty)}");
                Console.WriteLine($"\t  {sampleArgumentNames[5]}: {(sample.QualityControl?.QualityControlName ?? string.Empty)}");
                Console.WriteLine($"\t  {sampleArgumentNames[6]}: {sample.SaveEveryNthImage}");
                Console.WriteLine($"\t  {sampleArgumentNames[7]}: {sample.WashType}");
                Console.WriteLine();
                sampleIndex++;
            }

            // Initialize a default VcbResult object for the output.
            var callResult = new ViCellBlu.VcbResult
            {
                ErrorLevel = ViCellBlu.ErrorLevelEnum.Warning,
                MethodResult = ViCellBlu.MethodResultEnum.Failure
            };

            // --- OPC UA Method Call Preparation ---
            // Find the reference description for the "StartSampleSet" method from the pre-browsed collection.
            // Assumes ConnectionManager._methodCollectionPlayControl holds method nodes for "PlayControl".
            // ViCellBlu.BrowseNames.StartSampleSet provides the string literal "StartSampleSet".
            var methodStartSampleSet = ConnectionManager._methodCollectionPlayControl.First(n => n.DisplayName.ToString().Equals(ViCellBlu.BrowseNames.StartSampleSet)); //"StartSampleSet"
                                                                                                                                                                         // Convert the method's ExpandedNodeId (from browsing) to a Session-specific NodeId.
            var methodNodeStartSampleSet = ExpandedNodeId.ToNodeId(methodStartSampleSet.NodeId, ConnectionManager._opcSession.NamespaceUris);

            // Create a RequestHeader (often default).
            var reqHeader = new RequestHeader();
            // Create the CallMethodRequest object for the StartSampleSet method.
            var cmRequest = new CallMethodRequest
            {
                // ObjectId: The NodeId of the parent object ("PlayControl").
                ObjectId = ConnectionManager._parentPlayNode,
                // MethodId: The NodeId of the method itself.
                MethodId = methodNodeStartSampleSet,
                // InputArguments: The SampleSet object wrapped in a Variant.
                InputArguments = new VariantCollection { new Variant(sampleSet) }
            };

            // Create a collection containing the single CallMethodRequest.
            var cmReqCollection = new CallMethodRequestCollection { cmRequest };
            // Call the method on the server using the active OPC UA session.
            var respHdr = ConnectionManager._opcSession.Call(reqHeader, cmReqCollection, out var resultCollection, out var diagResults);

            // Check if output arguments were returned and decode the first one.
            if ((resultCollection.Count > 0) && (resultCollection[0].OutputArguments.Count > 0))
            {
                callResult = DecodeRaw(resultCollection[0].OutputArguments[0].Value, ConnectionManager._opcSession.MessageContext);
            }

            // Print the final result status based on the decoded VcbResult.
            Console.WriteLine(callResult.MethodResult == ViCellBlu.MethodResultEnum.Success
                ? "StartSampleSet Success.\n"
                : "StartSampleSet Failure.\n");
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

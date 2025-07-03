using Opc.Ua;
using System;
using System.Linq;
using ViCellBlu;

namespace OPCUAExamples
{
    public class RequestLockOperation
    {
        /// <summary>
        /// Calls the "RequestLock" method on the Vi-Cell BLU OPC UA server.
        /// Decodes and prints the specific result structure for RequestLock.
        /// </summary>
        public void CallRequestLock()
        {
            // Find the RequestLock method node
            var methodRequestLock = ConnectionManager._methodCollection.First(n => n.DisplayName.ToString().Equals("RequestLock"));
            var methodNodeRequestLock = ExpandedNodeId.ToNodeId(methodRequestLock.NodeId, ConnectionManager._opcSession.NamespaceUris);

            Console.WriteLine($"Calling method: {methodRequestLock.DisplayName}");

            // Prepare method call request (no input arguments for RequestLock)
            var reqHeaderRequestLock = new RequestHeader();
            var cmRequestRequestLock = new CallMethodRequest
            {
                ObjectId = ConnectionManager._parentMethodNode,
                MethodId = methodNodeRequestLock,
                InputArguments = new VariantCollection() // RequestLock has no input arguments
            };
            var cmReqCollectionRequestLock = new CallMethodRequestCollection { cmRequestRequestLock };

            // Show what is being sent
            Console.WriteLine("Sending:");
            Console.WriteLine($"  Command: {methodRequestLock.DisplayName}");
            //Console.WriteLine($"  ObjectId: {_parentMethodNode}"); // Optional: show Object and Method IDs
            //Console.WriteLine($"  MethodId: {methodNodeRequestLock}");

            // Call the method
            ConnectionManager._opcSession.Call(reqHeaderRequestLock, cmReqCollectionRequestLock, out var resultCollectionRequestLock, out var diagResultsRequestLock);

            // Show output
            var result = resultCollectionRequestLock[0];
            Console.WriteLine("Method call result status: " + result.StatusCode);

            // decoding the output if available
            if ((resultCollectionRequestLock.Count > 0) && (resultCollectionRequestLock[0].OutputArguments.Count > 0))
            {
                Console.WriteLine("Decoding method output...");
                // Use the specific decode method for RequestLock results
                DecodeRawLockResult(resultCollectionRequestLock[0].OutputArguments[0].Value,
                    (ServiceMessageContext)ConnectionManager._opcSession.MessageContext, diagResultsRequestLock);
            }
            else
            {
                Console.WriteLine("No output values returned by the method.");
                // Still check diagnostic results if the call itself had issues
                if (diagResultsRequestLock != null && diagResultsRequestLock.Count > 0)
                {
                    Console.WriteLine($"Diagnostic result: {diagResultsRequestLock.ToString()}");
                }
            }
        }

        /// <summary>
        /// Decodes the raw byte[] output from the RequestLock method into a specific ViCellBlu result structure.
        /// Assumes the output is an ExtensionObject containing binary data decodeable as a ViCellBlu.VcbResult
        /// followed by the specific LockStateEnum.
        /// </summary>
        public void DecodeRawLockResult(Object rawResult, ServiceMessageContext messageContext, DiagnosticInfoCollection diagResultsRequestLock)
        {
            Console.WriteLine("Attempting to decode raw RequestLock result...");
            
            // The raw result is expected to be an ExtensionObject containing a byte array
            var val = (Opc.Ua.ExtensionObject)rawResult;
            byte[] myData = (byte[])val.Body;
            
            IDecoder decoder = new Opc.Ua.BinaryDecoder(myData, 0, myData.Count(), messageContext);
            try
            {
                // Push the ViCellBlu namespace URI to correctly resolve NodeIds within the encoded data
                decoder.PushNamespace(ViCellBlu.Namespaces.ViCellBlu);

                // Read the common VcbResult parts first based on the ViCellBlu SDK structure
                var MethodResult = (MethodResultEnum)decoder.ReadEnumerated("MethodResult", typeof(MethodResultEnum));
                var ResponseDescription = decoder.ReadString("ResponseDescription");
                var ErrorLevel = (ErrorLevelEnum)decoder.ReadEnumerated("ErrorLevel", typeof(ErrorLevelEnum));
                
                // Then read the specific output argument for RequestLock: LockState               
                var Lockstate = (LockStateEnum)decoder.ReadEnumerated("LockState", typeof(LockStateEnum));

                Console.WriteLine($"  MethodResult: {MethodResult}");
                Console.WriteLine($"  ResponseDescription: {ResponseDescription}");
                Console.WriteLine($"  ErrorLevel: {ErrorLevel}");
                Console.WriteLine($"  LockState: {Lockstate}");

                // Display diagnostic info if any
                if (diagResultsRequestLock != null && diagResultsRequestLock.Count > 0)
                {
                    Console.WriteLine($"  Diagnostic result: {diagResultsRequestLock.ToString()}");
                }
                else
                {
                    Console.WriteLine("  No diagnostic information available.");
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error decoding raw result: {ex.Message}");
                // If decoding fails, you might want to show the raw bytes
                Console.WriteLine($"  Raw Bytes (if available): {(rawResult is Opc.Ua.ExtensionObject ext && ext.Body is byte[] bytes ? BitConverter.ToString(bytes) : "N/A")}");

                // Still display diagnostic info even if decoding failed
                if (diagResultsRequestLock != null && diagResultsRequestLock.Count > 0)
                {
                    Console.WriteLine($"  Diagnostic result: {diagResultsRequestLock.ToString()}");
                }
            }
            finally
            {
                // Important: Pop the namespace after decoding
                if (decoder != null)
                {
                    decoder.PopNamespace();
                }
            }
        }

    }
}

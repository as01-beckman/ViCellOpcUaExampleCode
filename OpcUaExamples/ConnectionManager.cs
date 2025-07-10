using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.Linq;
using System.Net;


namespace OPCUAExamples
{
    public class ConnectionManager
    {
        
        // Holds the active OPC UA session.
        public static Session _opcSession;
        // Stores the collection of method nodes found during browsing.
        public static ReferenceDescriptionCollection _methodCollection;
        // Stores the collection of playcontrol nodes found during browsing.
        public static ReferenceDescriptionCollection _methodCollectionPlayControl;
        // The parent node of the methods (used as ObjectId in method calls).
        public static NodeId _parentMethodNode;
        // The parent node of the playcontrol (used as ObjectId in method calls).
        public static NodeId _parentPlayNode;
        // Namespace table for resolving ExpandedNodeIds.
        public static NamespaceTable _namespaceUris;
        // Application configuration for OPC UA client.
        public static ApplicationConfiguration OpcAppConfig { get; private set; }

        private static string _applicationName = "ViCellOpcUaClient_ImplementationExample";
        public static void InitializeConnection()
        {
            Console.WriteLine("Enter OPC UA Server IP Address (e.g., 127.0.0.1):");
            var ipAddress = Console.ReadLine();

            Console.WriteLine("Enter OPC UA Server Port (e.g., 62641):");
            var port = Console.ReadLine();

            Console.WriteLine("Enter Username:");
            var username = Console.ReadLine();

            Console.WriteLine("Enter Password:");
            var password = Console.ReadLine();

            //var ipAddress = "127.0.0.1"; // Replace with your Vi-Cell BLU IP
            //var port = "62641";         // Replace with your Vi-Cell BLU Port
            //var username = "factory_admin"; // Replace with your username
            //var password = "Pass#12345";     // Replace with your password

            try
            {
                var endpointUrl = $"opc.tcp://{ipAddress}:{port}/ViCellBlu/Server";
                Console.WriteLine("Endpoint Url : " + endpointUrl);

                // 1. Initialize and configure application
                InitializeAppConfig();
                ConfigureSecurity();
                OpcAppConfig.Validate(ApplicationType.Client).Wait();
                Console.WriteLine("Initialize and configure application created successfully.");

                // 2. Create session
                _opcSession = CreateSession(endpointUrl, username, password);
                _namespaceUris = _opcSession.NamespaceUris;
                Console.WriteLine("OPC UA Session connection status : " + _opcSession.Connected.ToString());

                // 3. Browse and setup method collection
                SetupMethodCollection();
                Console.WriteLine("Method collection setup complete.");

                Console.WriteLine("Connected");

            }
            catch (Exception ex)
            {
                Console.WriteLine("InitializeConnection Exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Initializes the OPC UA application configuration for demo.
        /// Sets up application name, URI, security, transport, and client settings.
        /// </summary>
        private static void InitializeAppConfig()
        {
            OpcAppConfig = new ApplicationConfiguration
            {
                ApplicationName = _applicationName,
                ApplicationUri = Utils.Format(@"urn:{0}:ViCellBLU:Server", Dns.GetHostName()),
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration(),
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas(),
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                DisableHiResClock = true,
                TraceConfiguration = new TraceConfiguration(),
                CertificateValidator = new CertificateValidator()
            };
            OpcAppConfig.TransportQuotas = new TransportQuotas
            {
                OperationTimeout = 600000,
                MaxStringLength = 1048576,
                MaxByteStringLength = 1048576,
                MaxArrayLength = 65535,
                MaxMessageSize = 4194304,
                MaxBufferSize = 65535,
                ChannelLifetime = 300000,
                SecurityTokenLifetime = 3600000
            };
            OpcAppConfig.ClientConfiguration.MinSubscriptionLifetime = 10000;
            Console.WriteLine("Application configuration initialized.");
        }

        /// <summary>
        /// Configures OPC UA application security settings for demo purposes.
        /// Sets up certificate stores, trust lists, and validation behavior.
        /// </summary>
        private static void ConfigureSecurity()
        {
            OpcAppConfig.SecurityConfiguration.ApplicationCertificate = new CertificateIdentifier
            {
                StoreType = @"Directory",
                StorePath = @"%CommonApplicationData%\ViCellBlu_dotNET\pki\own",
                SubjectName = "CN=Vi-Cell BLU Client, C=US, S=Colorado, O=Beckman Coulter, DC=" + Dns.GetHostName()
            };
            OpcAppConfig.SecurityConfiguration.TrustedIssuerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\ViCellBlu_dotNET\pki\issuers" };
            OpcAppConfig.SecurityConfiguration.TrustedPeerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\ViCellBlu_dotNET\pki\trusted" };
            OpcAppConfig.SecurityConfiguration.RejectedCertificateStore = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\ViCellBlu_dotNET\pki\rejected" };
            OpcAppConfig.SecurityConfiguration.UserIssuerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\ViCellBlu_dotNET\pki\issuerUser" };
            OpcAppConfig.SecurityConfiguration.TrustedUserCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\ViCellBlu_dotNET\pki\trustedUser" };
            OpcAppConfig.SecurityConfiguration.AddAppCertToTrustedStore = true;
            OpcAppConfig.SecurityConfiguration.RejectSHA1SignedCertificates = false;
            OpcAppConfig.SecurityConfiguration.RejectUnknownRevocationStatus = true;
            OpcAppConfig.SecurityConfiguration.MinimumCertificateKeySize = 2048;
            OpcAppConfig.SecurityConfiguration.SendCertificateChain = true;
            OpcAppConfig.SecurityConfiguration.AutoAcceptUntrustedCertificates = true; // ONLY FOR DEMO
            OpcAppConfig.CertificateValidator.CertificateValidation += (validator, e) =>
            {
                if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
                    e.Accept = true; // ONLY FOR DEMO
            };
            Console.WriteLine("Security configuration applied.");
        }

        /// <summary>
        /// Establishes a session with the OPC UA server.
        /// Handles endpoint selection, user authentication, and session creation.
        /// </summary>
        /// <param name="endpointUrl">OPC UA server endpoint URL.</param>
        /// <param name="username">Username for authentication.</param>
        /// <param name="password">Password for authentication.</param>
        /// <returns>Established Session object.</returns>
        private static Session CreateSession(string endpointUrl, string username, string password)
        {
            var application = new ApplicationInstance
            {
                ApplicationType = ApplicationType.Client,
                ApplicationName = _applicationName,
                ApplicationConfiguration = OpcAppConfig
            };

            // Ensure application certificate exists
            var haveAppCertificateTask = application.CheckApplicationInstanceCertificate(false, 0);
            var haveAppCertificate = haveAppCertificateTask.Result;
            if (haveAppCertificate)
                OpcAppConfig.ApplicationUri = X509Utils.GetApplicationUriFromCertificate(OpcAppConfig.SecurityConfiguration.ApplicationCertificate.Certificate);

            // Select the best endpoint (with security if possible)
            // Using default timeout 15s
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(OpcAppConfig, endpointUrl, haveAppCertificate, 15000);
            var endpointConfiguration = EndpointConfiguration.Create(OpcAppConfig);
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

            // Set up user identity (username/password)
            var user = new UserIdentity(username, password);

            // Create the session
            // Using default timeout 240s
            var sessionTask = Session.Create(OpcAppConfig, endpoint, true, false, _applicationName, 240000, user, null);
            return sessionTask.Result;
        }

        /// <summary>
        /// Browses to the Methods node and sets up method collection and parent node.
        /// This is required to locate methods like "RequestLock".
        /// </summary>
        private static void SetupMethodCollection()
        {
            // Browse root objects
            var scoutXrefs = BrowseNode(out _);
            // Find the ViCellBluStateObject node
            var viCellBlu = scoutXrefs.First(n => n.BrowseName.Name.Equals("ViCellBluStateObject"));
            // Browse its children
            var viCellBluRefs = BrowseNode(out _, viCellBlu.NodeId);
            // Find the Methods node
            var browsedMethods = viCellBluRefs.First(n => n.BrowseName.Name.Equals("Methods"));
            // Browse PlayControl
            var browsedPlayControl = viCellBluRefs.First(n => n.BrowseName.Name.Equals("PlayControl"));
            // Browse the Methods node to get available methods
            _methodCollection = BrowseNode(out _, browsedMethods.NodeId);
            _methodCollectionPlayControl = BrowseNode(out _, browsedPlayControl.NodeId);
            // Store the parent node for method calls
            _parentMethodNode = ExpandedNodeId.ToNodeId(browsedMethods.NodeId, _opcSession.NamespaceUris);
             // Store the parent node for play control calls  
            _parentPlayNode = ExpandedNodeId.ToNodeId(browsedPlayControl.NodeId, _opcSession.NamespaceUris);
        }

        /// <summary>
        /// Browses the OPC UA server for references from a given node.
        /// If no node is specified, starts from the ObjectsFolder.
        /// </summary>
        /// <param name="continuationPoint">Continuation point for browsing large address spaces.</param>
        /// <param name="nodeId">NodeId to browse from (optional).</param>
        /// <returns>Collection of references found.</returns>
        private static ReferenceDescriptionCollection BrowseNode(out byte[] continuationPoint, NodeId nodeId = null)
        {
            if (nodeId == null)
                nodeId = Opc.Ua.ObjectIds.ObjectsFolder;
            _opcSession.Browse(null, null, nodeId, 0u, BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences,
                true,
                (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method, out continuationPoint,
                out var references);
            return references;
        }

        /// <summary>
        /// Overload to browse using ExpandedNodeId.
        /// Converts ExpandedNodeId to NodeId using the namespace table.
        /// </summary>
        /// <param name="continuationPoint">Continuation point for browsing.</param>
        /// <param name="expandedNodeId">ExpandedNodeId to browse from.</param>
        /// <returns>Collection of references found.</returns>
        private static ReferenceDescriptionCollection BrowseNode(out byte[] continuationPoint, ExpandedNodeId expandedNodeId)
        {
            var nodeId = ExpandedNodeId.ToNodeId(expandedNodeId, _namespaceUris);
            return BrowseNode(out continuationPoint, nodeId);
        }

    }
}

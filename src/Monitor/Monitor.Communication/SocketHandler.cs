using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Common.Logging;
using Monitor.Common;

namespace Monitor.Communication
{
    public class SocketHandler
    {
        private static ILog logger = LogManager.GetCurrentClassLogger();

        private string ip;
        private int port;

        private bool isStarted = false;

        public bool IsStarted
        {
            get { return isStarted; }
            set { isStarted = value; }
        }

        public SocketHandler(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        private Socket socket = null;

        public void Start()
        {
            if (!isStarted)
            {
                try
                {
                    // Create one SocketPermission for socket access restrictions
                    SocketPermission permission = new SocketPermission(
                        NetworkAccess.Connect,    // Connection permission
                        TransportType.Tcp,        // Defines transport types
                        "",                       // Gets the IP addresses
                        SocketPermission.AllPorts // All ports
                        );

                    // Ensures the code to have permission to access a Socket
                    permission.Demand();

                    IPAddress ipAddr = IPAddress.Parse(ip);

                    // Creates a network endpoint
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

                    // Create one Socket object to setup Tcp connection
                    socket = new Socket(
                        ipAddr.AddressFamily,// Specifies the addressing scheme
                        SocketType.Stream,   // The type of socket 
                        ProtocolType.Tcp     // Specifies the protocols 
                        );

                    Configure();

                    // Establishes a connection to a remote host
                    socket.Connect(ipEndPoint);

                    logger.Debug(m => m("Socket connected to {0}", socket.RemoteEndPoint.ToString()));
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }

            isStarted = true;
        }

        public void Stop()
        {
            if (isStarted)
            {
                try {
                    // Disables sends and receives on a Socket.
                    socket.Shutdown(SocketShutdown.Both);

                    //Closes the Socket connection and releases all resources
                    socket.Close();

                    logger.Debug("Socket disconnected");
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }

            isStarted = false;
        }

        private void Configure()
        {
            // Don't allow another socket to bind to this port.
            socket.ExclusiveAddressUse = true;

            // The socket will linger for 10 seconds after 
            // Socket.Close is called.
            socket.LingerState = new LingerOption(true, 10);

            // Disable the Nagle Algorithm for this tcp socket.
            socket.NoDelay = true;

            // Set the receive buffer size to 8k
            socket.ReceiveBufferSize = 8192;

            // Set the timeout for synchronous receive methods to 
            // 1 second (1000 milliseconds.)
            socket.ReceiveTimeout = 1000;

            // Set the send buffer size to 8k.
            socket.SendBufferSize = 8192;

            // Set the timeout for synchronous send methods
            // to 1 second (1000 milliseconds.)			
            socket.SendTimeout = 1000;

            // Set the Time To Live (TTL) to 42 router hops.
            socket.Ttl = 42;
        }

        public bool Handshake(string projectId, string gatewayId)
        {
            try
            {
                SendRequest(projectId, gatewayId);
                Message response = ReceiveResponse();
                logger.Debug(m => m("Response sequence: {0}", response.id_validate.sequence));
                SendAuthentication(response.id_validate.sequence, projectId, gatewayId);
                Message result = ReceiveAuthenticationResult();
                logger.Debug(m => m("result: {0}", result.id_validate.result));

                if (result.id_validate.result.Equals("pass"))
                {
                    logger.Debug("Handshake succeed.");
                    return true;
                }
                else
                {
                    logger.Debug("Handshake succeed.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return false;
        }

        private void SendRequest(string projectId, string gatewayId)
        {
            byte[] msg = MessageFactory.CreateRequest(projectId, gatewayId);

            int bytesSend = socket.Send(msg);

            logger.Debug(m => m("Send request {0} bytes", bytesSend));
        }

        private Message ReceiveResponse()
        {
            byte[] bytes = new byte[1024];

            int bytesRec = Receive(bytes);

            logger.Debug(m =>m("Receive response {0} bytes", bytesRec));

            return MessageFactory.GetResponse(bytes, bytesRec);
        }

        private void SendAuthentication(string sequence, string projectId, string gatewayId)
        {
            byte[] msg = MessageFactory.CreateAuthentication(projectId, gatewayId, sequence);

            int bytesSend = socket.Send(msg);

            logger.Debug(m => m("Send authentication {0} bytes", bytesSend));
        }

        private Message ReceiveAuthenticationResult()
        {
            byte[] bytes = new byte[1024];

            int bytesRec = Receive(bytes);

            logger.Debug(m => m("Receive authentication result {0} bytes", bytesRec));

            return MessageFactory.GetAuthenticationResult(bytes, bytesRec);
        }

        //public bool HeartBeat(string projectId, string gatewayId)
        //{
        //    try
        //    {
        //        SendHeartBeat(projectId, gatewayId);
        //        Message result = ReceiveHeartBeatResult();

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);
        //    }

        //    return false;
        //}

        //private void SendHeartBeat(string projectId, string gatewayId)
        //{
        //    byte[] msg = MessageFactory.CreateHeartBeat(projectId, gatewayId);

        //    int bytesSend = socket.Send(msg);

        //    logger.Debug(m => m("Send heart beat {0} bytes", bytesSend));
        //}

        //private Message ReceiveHeartBeatResult()
        //{
        //    byte[] bytes = new byte[1024];

        //    int bytesRec = Receive(bytes);

        //    logger.Debug(m => m("Receive heart beat result {0} bytes", bytesRec));

        //    return MessageFactory.GetHeartBeatResult(bytes, bytesRec);
        //}

        //public bool Report(string projectId, string gatewayId)
        //{
        //    try
        //    {
        //        SendReport(projectId, gatewayId);
        //        ReceiveReportResult();

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);
        //    }

        //    return false;
        //}

        //private void SendReport(string projectId, string gatewayId)
        //{
        //    byte[] msg = MessageFactory.CreateReport(projectId, gatewayId);

        //    int bytesSend = socket.Send(msg);

        //    logger.Debug(m => m("Send report {0} bytes", bytesSend));
        //}

        //public Message ReceiveReportResult()
        //{
        //    byte[] bytes = new byte[1024];

        //    int bytesRec = Receive(bytes);

        //    logger.Debug(m => m("Receive report result {0} bytes", bytesRec));

        //    return MessageFactory.GetMessage(bytes, bytesRec);
        //}

        public int Send(byte[] bytes)
        {
            try
            {
                return socket.Send(bytes);
            }
            catch (SocketException se)
            {
                logger.Error(se);
                logger.Info("Shutdown monitor");
                System.Environment.Exit(0);
            }

            return 0;
        }

        public int Receive(byte[] buffer)
        {
            int bytesRec = socket.Receive(buffer);

            /*
            // Converts byte array to string
            theMessage = Encoding.Unicode.GetString(bytes, 0, bytesRec);

            // Continues to read the data till data isn't available
            while (socket.Available > 0)
            {
                bytesRec = socket.Receive(bytes);
                theMessage += Encoding.Unicode.GetString(bytes, 0, bytesRec);
            }
            */

            return bytesRec;
        }

        public static void Send(Socket socket, byte[] buffer, int offset, int size, int timeout)
        {
            int startTickCount = Environment.TickCount;
            int sent = 0;  // how many bytes is already sent
            do
            {
                if (Environment.TickCount > startTickCount + timeout)
                    throw new Exception("Timeout.");
                try
                {
                    sent += socket.Send(buffer, offset + sent, size - sent, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably full, wait and try again
                        //Thread.Sleep(30);
                    }
                    else
                        throw ex;  // any serious error occurr
                }
            } while (sent < size);
        }

        public static void Receive(Socket socket, byte[] buffer, int offset, int size, int timeout)
        {
            int startTickCount = Environment.TickCount;
            int received = 0;  // how many bytes is already received
            do
            {
                if (Environment.TickCount > startTickCount + timeout)
                    throw new Exception("Timeout.");
                try
                {
                    received += socket.Receive(buffer, offset + received, size - received, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably empty, wait and try again
                        //Thread.Sleep(30);
                    }
                    else
                        throw ex;  // any serious error occurr
                }
            } while (received < size);
        }

    }
}

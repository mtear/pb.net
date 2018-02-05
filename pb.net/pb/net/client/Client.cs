using pb.encryption;
using pb.json;
using pb.net.server;
using System;
using System.Net.Sockets;
using System.Threading;

namespace pb.net.client
{
    public class Client : WebIOSocket
    {
        private static ManualResetEvent connectDone = new ManualResetEvent(false);

        protected SocketIOHandler sdw;

        protected Action connectedCallback;
        Action<JSONObject> messageReceivedCallback;

        private string tdesKey;
        public string TDESKey
        {
            get
            {
                return tdesKey;
            }
            set
            {
                if(sdw != null)
                    sdw.Symkey = value;
                tdesKey = value;
            }
        }

        private string rsaKey;
        public string RSAKey
        {
            get
            {
                return rsaKey;
            }
            set
            {
                rsaKey = value;
            }
        }

        public Client() { }
        public Client(Action connectedCallback, Action<JSONObject> messageReceivedCallback)
        {
            this.connectedCallback = connectedCallback;
            this.messageReceivedCallback = messageReceivedCallback;
        }

        public Client Connect(String address, int port)
        {
            Connection connection = CreateConnectionSocket(address, port);

            // Connect to the remote endpoint
            connection.Socket.BeginConnect(connection.Target, new AsyncCallback(ConnectionCompletedCallback), connection.Socket);
            connectDone.WaitOne();

            sdw = new SocketIOHandler(connection.Socket);
            sdw.Symkey = tdesKey;
            sdw.SetCallback(new ServerMessageCallback(_MessageReceivedCallback));

            return this;
        }

        private void ConnectionCompletedCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket server = (Socket)ar.AsyncState;

                // Complete the connection.  
                server.EndConnect(ar);

                Console.WriteLine("Connected to {0}",
                    server.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                  NetUtil.Log(e.ToString());
                  Console.ReadLine();
            }

            //Call the user defined on connect method
            connectedCallback?.Invoke();

        }

        private void _MessageReceivedCallback(SocketIOHandler user, JSONObject message)
        {
            messageReceivedCallback?.Invoke(message);
        }

        public void SendRSA(string json)
        {
            Send(RSA.Encrypt(rsaKey, json));
        }

        public void SendTDES(string json)
        {
            Send(Convert.ToBase64String(TDES.Encrypt(tdesKey, json)));
        }

        public void Send(string json)
        {
            sdw.Send(json);
        }

    }
}

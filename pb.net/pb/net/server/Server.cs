using pb.json;
using pb.net.user;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace pb.net.server
{
    public delegate void ServerMessageCallback(SocketIOHandler user, JSONObject message);
    public delegate User GetUserForServer(SocketIOHandler sdw);

    public class Server : WebIOSocket
    {

        protected int port;
        protected Action<SocketIOHandler, JSONObject> messageReceivedCallback;
        protected Func<SocketIOHandler, User> userForServerCallback;
        static ManualResetEvent mutex = new ManualResetEvent(false);
        Dictionary<SocketIOHandler, User> clients = new Dictionary<SocketIOHandler, User>();

        public Server(int port) { this.port = port; }
        public Server(int port, Func<SocketIOHandler, User> userForServerCallback,
            Action<SocketIOHandler, JSONObject> messageReceivedCallback)
        {
            this.port = port;
            this.userForServerCallback = userForServerCallback;
            this.messageReceivedCallback = messageReceivedCallback;
        }

        public int Start()
        {
            Connection connection = CreateConnectionSocket(Dns.GetHostName(), port);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                connection.Socket.Bind(connection.Target);
                connection.Socket.Listen(100);
                NetUtil.Log("Server started");
        
                while (true)
                {
                    NetUtil.Log("Waiting for client connection");

                    // Set the event to nonsignaled state.  
                    mutex.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    connection.Socket.BeginAccept(new AsyncCallback(ConnectionCompletedCallback), connection.Socket);

                    // Wait until a connection is made before continuing.  
                    mutex.WaitOne();
                }
            }
            catch (Exception e)
            {
                NetUtil.Log("EXCEPTION <stack trace>:");
                NetUtil.Log(e.ToString());
                return -1;
            }
        }

        public User GetUser(SocketIOHandler sdw)
        {
            if (clients.ContainsKey(sdw))
            {
                return clients[sdw];
            }
            else return null;
        }

        void ConnectionCompletedCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            mutex.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            NetUtil.Log("New client connected from " + (handler.RemoteEndPoint as IPEndPoint).Address);

            SocketIOHandler sdw = new SocketIOHandler(handler);
            clients[sdw] = userForServerCallback?.Invoke(sdw);
            sdw.SetCallback(new ServerMessageCallback(_MessageReceivedCallback));
        }

        private void _MessageReceivedCallback(SocketIOHandler user, JSONObject message)
        {
            messageReceivedCallback?.Invoke(user, message);
        }

        public void Send(SocketIOHandler user, string json)
        {
            user.Send(json);
        }

        public void Broadcast(string json)
        {
            foreach(User u in clients.Values)
            {
                u.socketIOHandler.Send(json);
            }
        }

    }
}

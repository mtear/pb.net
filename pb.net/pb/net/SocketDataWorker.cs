using pb.json;
using pb.net.server;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace pb.net
{
    public class SocketIOHandler
    {

        SocketInputHandler inputWorker;
        Socket socket;
        public Socket ConnectionInfo
        {
            get
            {
                return socket;
            }
        }

        private string symkey = null;
        public String Symkey
        {
            get { return symkey; }
            set { symkey = value; }
        }

        private string asymkey = null;
        public String Asymkey
        {
            get { return asymkey; }
            set { asymkey = value; }
        }

        private ServerMessageCallback callback = null;

        public String IpAddress
        {
            get
            {
                return (socket.RemoteEndPoint as System.Net.IPEndPoint).Address.ToString();
            }
        }

        public SocketIOHandler(Socket socket)
        {
            this.socket = socket;
            inputWorker = new SocketInputHandler(socket, new SocketInputHandler.SocketMessageReceivedCallback(MessageReceived));
        }

        public void Send(string data)
        {
            SocketSend(socket, data, symkey);
        }

        private static void SocketSend(Socket handler, String data, string symkey)
        {
            //Encrypt
            if(symkey != null)
            {
               // data = Convert.ToBase64String(TDESHandler.Encrypt(symkey, data));
            }

            NetUtil.Log("About to send: " + data);

            //Add message ending
            data += SocketDataBuffer.EOF;

            // Convert the string data to byte data using UTF8 encoding.  
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                NetUtil.Log("Sent " + bytesSent + " bytes to client.");
            }
            catch (Exception e)
            {
                NetUtil.Log(e.ToString());
            }
        }

        public void SetCallback(ServerMessageCallback callback)
        {
            this.callback = callback;
        }

        public void MessageReceived(string json)
        {
            if(asymkey != null)
            {
                json = RSAHandler.Decrypt(asymkey, json);
            }else 
            if(symkey != null)
            {
                json = TDESHandler.Decrypt(symkey, json);
            }
            JSONObject jo = new JSONObject(json);
            callback?.Invoke(this, jo);
        }

    }
}

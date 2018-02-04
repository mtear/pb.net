using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace pb.net
{
    public class WebIOSocket
    {

        protected bool useTDES;
        protected string rsaKey;
        protected string tdesKey;

        protected Connection CreateConnectionSocket(string address, int port)
        {
            //Create local server ip endpoint
            IPHostEntry ipHostInfo = Dns.Resolve(address);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
            // Create a tcp socket on the endpoint 
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Connection connection = new Connection();
            connection.Target = endPoint;
            connection.Socket = socket;
            return connection;
        }

        public struct Connection{
            public IPEndPoint Target;
            public Socket Socket;
        }

    }
}

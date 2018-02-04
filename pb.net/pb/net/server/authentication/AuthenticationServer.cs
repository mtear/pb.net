using pb.json;
using pb.net.user;
using System;

namespace pb.net.server.authentication
{
    public class AuthenticationServer : Server
    {

        private Func<string, string, JSONObject> authenticateUserCallback;

        public AuthenticationServer(int port, string rsaKey, bool useTDES, Func<string, string, JSONObject> authenticateUser)
        : base(port)
        {
            this.rsaKey = rsaKey;
            this.useTDES = useTDES;
            this.authenticateUserCallback = authenticateUser;

            this.userForServerCallback = GetUserForServer;
            this.messageReceivedCallback = MessageReceivedCallback;
        }

        User GetUserForServer(SocketIOHandler sdw)
        {
            sdw.Asymkey = rsaKey;
            return new User(sdw);
        }

        void MessageReceivedCallback(SocketIOHandler socket, JSONObject message)
        {
            NetUtil.Log(socket.IpAddress + ": " + message.ToString());

            if (authenticateUserCallback != null)
            {
                JSONObject response = authenticateUserCallback.Invoke(message.GetString("username"), message.GetString("password"));
                if (useTDES)
                {
                    Send(socket, Convert.ToBase64String(TDESHandler.Encrypt(message.GetString("key"), response.ToString())));
                }
                else
                {
                    Send(socket, response.ToString());
                }
            }
        }

    }

}

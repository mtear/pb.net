using System;
using System.Net.Sockets;
using System.Text;

namespace pb.net
{
    public class SocketInputHandler
    {

        private SocketMessageReceivedCallback callback;

        private void ReadCallback(IAsyncResult ar)
        {
            NetUtil.Log("Reading data");

            // Get the socket input buffer from the callback result
            SocketDataBuffer state = (SocketDataBuffer)ar.AsyncState;
            Socket socket = state.TargetSocket;

            // Read data from the client   
            int bytesRead = socket.EndReceive(ar);
            if (bytesRead > 0)
            { 
                state.AppendData(Encoding.UTF8.GetString(state.Buffer, 0, bytesRead));

                // Pop a message if one is available 
                if (state.HasMessage)
                {
                    string message = state.PopMessage();
                    NetUtil.Log("Message: " + message);
                    callback(message);
                }

                //Keep reading
                socket.BeginReceive(state.Buffer, 0, SocketDataBuffer.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
        }

        public delegate void SocketMessageReceivedCallback(string message);

        public SocketInputHandler(Socket socket, SocketMessageReceivedCallback callback)
        {
            this.callback = callback;

            //Start the reader listening on the socket
            SocketDataBuffer state = new SocketDataBuffer(socket);
            socket.BeginReceive(state.Buffer, 0, SocketDataBuffer.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

    }
}

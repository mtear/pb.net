
namespace pb.net.user
{
    public class User
    {
        public SocketIOHandler socketIOHandler;

        public string IpAddress
        {
            get
            {
                return socketIOHandler.IpAddress;
            }
        }

        public User(SocketIOHandler socketIOHandler)
        {
            this.socketIOHandler = socketIOHandler;
        }
    }
}

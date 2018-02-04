using pb.json;
using System;

namespace pb.net.client.authentication
{
    public class AuthenticationClient : Client
    {

        private string username;
        private string password;
        private Action<JSONObject> messageReceivedCallback;

        public AuthenticationClient(string username, string password, string rsaKey, bool useTDES, Action<JSONObject> messageReceivedCallback)
        : base(null, messageReceivedCallback)
        {
            this.username = username;
            this.password = password;
            this.rsaKey = rsaKey;
            this.useTDES = useTDES;
            this.messageReceivedCallback = messageReceivedCallback;

            this.connectedCallback = _ConnectedCallback;
        }

        protected void _ConnectedCallback()
        {
            JSONObject jsonObject = new JSONObject().Put("username", username).Put("password", password);
            if (useTDES)
            {
                tdesKey = TDESHandler.GenerateKey();
                jsonObject.Put("key", tdesKey);
            }

            if(rsaKey != null && rsaKey != ""){
                Send(RSAHandler.Encrypt(rsaKey, jsonObject.ToString()));
            }else{
                Send(jsonObject.ToString());
            }

            this.sdw.Symkey = tdesKey;
        }

    }
}

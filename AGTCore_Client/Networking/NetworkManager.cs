using Riptide;
using Riptide.Utils;
using System;

namespace AGTCore
{
    public enum ServerMessageID : ushort
    {
        spawnPlayer = 1,
        position,
        block,
        chunkSend
    }

    public enum ClientMessageID : ushort
    {
        username = 1,
        position,
        block,
        chunkLoad,
        chunkUnload,
    }

    class NetworkManager : Updatable
    {
        public static Client client { get; set; }
        public string ip = "127.0.0.1";
        public ushort port = 8001;
        Random random = new Random();
        string username;


        public NetworkManager()
        {
            // tells riptide what method to use for sending console messages and errors
            RiptideLogger.Initialize(System.Console.WriteLine, true);
            client = new Client();
            // += suscribes a method to an event
            client.Connected += Connected;
            client.ClientDisconnected += NetworkPlayerDisconnected;
            client.Disconnected += Disconnected;
            base.beginUpdating();
        }


        public bool Connect(string _ip, string _username)
        {
            username = _username;
            if (_ip == "") // adds default ip 
                _ip = ip;
            if (!_ip.Contains(":")) // adds default port
                _ip = $"{ip}:{port}";

            return client.Connect(_ip);
        }

        public void SendUsername()
        {
            // creates a new message, adds a string containt the client's username, and sends it
            // MessageSendMode.Reliable ensures the message is recieved
            Message message = Message.Create(MessageSendMode.Reliable, ClientMessageID.username);
            message.AddString(username);
            client.Send(message);
        }

        // updates the riptide client
        public override void Update()
        {
            client.Update();
        }
        // disconnects client if this object is disposed
        public override void OnDispose()
        {
            client.Disconnect();

        }

        // invoked when riptide sends the connected event
        private void Connected(object sender, EventArgs e)
        {
            // registers username with server
            SendUsername();
        }

        private void Disconnected(object sender, EventArgs e)
        {

        }

        private void NetworkPlayerDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            Player.players[e.Id].Dispose(); // todo: remove player entity as well
        }
    }
}
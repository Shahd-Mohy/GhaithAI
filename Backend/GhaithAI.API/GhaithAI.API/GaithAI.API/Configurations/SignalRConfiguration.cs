namespace GhaithAI.API.Configurations
{
    public class SignalRConfiguration
    {
        public int ClientTimeoutInterval { get; set; }

        public int HandshakeTimeout { get; set; }

        public int KeepAliveInterval { get; set; }
    }
}
namespace ElectrumX
{
    public class ClientSettings
    {
        public string Host { get; set; } 
        public int Port { get; set; }
        public Coin Coin { get; set; }
        public Network Network { get; set; }
        public bool UseSsl { get; set; }
    }
}

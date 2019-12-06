using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ElectrumX.Request;
using ElectrumX.Response;
using ElectrumX.ResultsModel;
using NBitcoin;
using NBitcoin.Altcoins;
using Newtonsoft.Json;

namespace ElectrumX
{
    public class Subscriber
    {
        
        public event Action<Subscriber, string> OnReceiveMessageOnAddress;
        public event Action<Subscriber, BlockchainHeadersSubscribeResult> OnReceiveMessageOnBlockHeader;
        private string Host { get; }
        private int Port { get; }
        private TcpClient TcpClient { get; set; }
        private NetworkStream TcpStream { get; set; }
        private int Buffersize { get; } = ushort.MaxValue;
        private bool IsConnected { get; set; }
        private Timer _timer;

        protected Coin Coin;
        protected readonly NBitcoin.Network Network;

        public Subscriber(string host, int port, Coin coin, Network network)
        {
            Host = host;
            Port = port;
            Coin = coin; 
            if (Coin == Coin.Bitcoin && network == ElectrumX.Network.Mainnet)
                Network = NBitcoin.Network.Main;
            else if (Coin == Coin.Bitcoin && network == ElectrumX.Network.Testnet)
                Network = NBitcoin.Network.TestNet;
            else if (Coin == Coin.BitcoinCash && network == ElectrumX.Network.Mainnet)
                Network = BCash.Instance.Mainnet;
            else if (Coin == Coin.BitcoinCash && network == ElectrumX.Network.Testnet)
                Network = BCash.Instance.Testnet;
        }

        public Subscriber(ClientSettings settings) : this(settings.Host, settings.Port, settings.Coin, settings.Network) { }

        public async Task SubscribeOnAddress(IDestination address)
        {
            var scriptHash = Client.GetScriptHash(address);
            var requestData = new BlockchainScripthashSubscribeRequest(scriptHash).GetRequestData();
            try
            {
                await Connect();
                await SendMessageToSubcribe(requestData);
                await ReceiveMessagesOnSubscribeOnAddress();
            }
            catch
            {
                // ignored
            }
        }

        public async Task SubscribeOnBlocks()
        {
            var requestData = new BlockchainHeadersSubscribeRequest().GetRequestData();
            try
            {
                await Connect();
                await SendMessageToSubcribe(requestData);
                await ReceiveMessagesOnSubscribeOnBlocks();
            }
            catch
            {
                // ignored
            }
        }

        private async Task ReceiveMessagesOnSubscribeOnBlocks()
        {
            if (!IsConnected)
                return;
            while (IsConnected)
            {
                var responseSb = new StringBuilder();
                var buffer = new byte[Buffersize];
                var bytes = await TcpStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytes <= 0)
                    continue;
                do
                {
                    responseSb.Append(Encoding.UTF8.GetString(buffer, 0, bytes));
                }
                while (TcpStream.DataAvailable);
                
                var message = responseSb.ToString();
                if (message.Contains("\"result\": null,"))
                {
                    Console.WriteLine("Received ping message");
                    continue;
                }
                var response = JsonConvert.DeserializeObject<BlockchainHeadersSubscribeResponse>(message);
                if (response != null) OnReceiveMessageOnBlockHeader?.Invoke(this, response.GetResultModel());
            }
        }

        private async Task Connect()
        {
            if (IsConnected)
                return;
            try
            {
                TcpClient = new TcpClient();
                await TcpClient.ConnectAsync(Host, Port);
                //not using ssl here
                TcpStream = TcpClient.GetStream();
                IsConnected = true;
                new Thread(StartTimerPing).Start();

            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void Ping(object state)
        {
            if (!IsConnected)
                return;
            var requestData = new ServerPingRequest().GetRequestData();
            TcpStream.Write(requestData, 0, requestData.Length);
        }

        private void StartTimerPing()
        {
                Console.WriteLine("Timed Background Ping Service is starting.");
                
                _timer = new Timer(Ping, null, TimeSpan.FromSeconds(0),
                    TimeSpan.FromMinutes(9));
        }

        private async Task SendMessageToSubcribe(byte[] requestData)
        {
            await TcpStream.WriteAsync(requestData, 0, requestData.Length);
        }

        private async Task ReceiveMessagesOnSubscribeOnAddress()
        {
            if (!IsConnected)
                return;
            while(IsConnected)
            {
                var responseSb = new StringBuilder();
                var buffer = new byte[Buffersize];
                var bytes = await TcpStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytes <= 0) continue;
                do
                {
                    responseSb.Append(Encoding.UTF8.GetString(buffer, 0, bytes));
                }
                while (TcpStream.DataAvailable);
                var message = responseSb.ToString();
                
                if (message.Contains("\"result\": null,"))
                {
                    Console.WriteLine("Received ping message");
                    continue;
                }
                var response = !string.IsNullOrEmpty(message) ? JsonConvert.DeserializeObject<BlockchainScriphashSubscribeResponse>(message) : null;
                if (response != null) OnReceiveMessageOnAddress?.Invoke(this, response.Params[0]);
            }
        }

        public void Disconnect()
        {
            IsConnected = false;
            TcpStream?.Close();
            TcpClient?.Close();
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;
            _timer?.Dispose();
            TcpClient?.Dispose();
            TcpStream?.Dispose();
            Disconnect();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

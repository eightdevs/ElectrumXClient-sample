namespace ElectrumX.Request
{
    internal class BlockchainHeadersSubscribeRequest : RequestBase<string>
    {
        internal BlockchainHeadersSubscribeRequest()
        {
            Method = "blockchain.headers.subscribe";
            Parameters = new string[] { };
        }
    }
}

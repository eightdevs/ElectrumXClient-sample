using System.Collections.Generic;
using ElectrumX.ResultsModel;

namespace ElectrumX.Response
{
    public class BlockchainScripthashListunspentResponse : ResponseBase<BlockchainScripthashListunspentResult>
    {
        [Newtonsoft.Json.JsonProperty("result")]
        public List<BlockchainScripthashListunspentResult> Result { get; set; }
        public List<BlockchainScripthashListunspentResult> GetResultModel()
        {
            return Result;
        }
    }
}

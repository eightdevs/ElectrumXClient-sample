﻿using ElectrumX.ResultsModel;

namespace ElectrumX.Response
{
    public class BlockchainTransactionBroadcastResponse : ResponseBase<BlockchainTransactionBroadcastResult>
    {
        [Newtonsoft.Json.JsonProperty("result")]
        public string Result { get; set; }
        public BlockchainTransactionBroadcastResult GetResultModel()
        {
            return new BlockchainTransactionBroadcastResult() {TxHash = Result};
        }
    }
}

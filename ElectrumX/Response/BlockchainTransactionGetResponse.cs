﻿using ElectrumX.ResultsModel;

namespace ElectrumX.Response
{
    public class BlockchainTransactionGetResponse : ResponseBase<BlockchainTransactionGetResult>
    {
        [Newtonsoft.Json.JsonProperty("result")]
        public string Result { get; set; }
        public BlockchainTransactionGetResult GetResultModel()
        {
            return new BlockchainTransactionGetResult()
            {
                RawTx = Result
            };
        }
    }
}

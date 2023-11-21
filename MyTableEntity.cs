using Azure;
using Azure.Data.Tables;
using System;

namespace FunctionApp1
{
    public class MyTableEntity : ITableEntity
    { 
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string EntityName { get; set; }
        public string FeatureName { get; set; }
        public int FeatureID { get; set; }
        public string Value { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

    }
}

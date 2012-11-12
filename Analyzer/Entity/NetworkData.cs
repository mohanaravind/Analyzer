using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace Analyzer.Entity
{
    class NetworkData
    {
        #region Entity


        public ObjectId Id { get; set; }
        public String ClientIP { get; set; }
        public String HostName { get; set; }
        public String URLFullPath { get; set; }
        public String RequestType { get; set; }
        public Dictionary<String, String> RequestParameters { get; set; }
        public Boolean IsHTTPS { get; set; }
        public String RequestedAt { get; set; }

        #endregion
    }
}

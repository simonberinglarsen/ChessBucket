using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoSite.Models
{
    public class BatchQueueItem
    {
        public int Id { get; set; }
        public QState QState { get; set; }
        public QType QType { get; set; }
        public string Reference { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime? ProcessingAt { get; set; }
        public string Error { get; set; }
        public string Info { get; internal set; }
    }

    public enum QState : int
    {
        Pending = 0,
        Processing = 1,
        Done = 2,
        Error = 3,
        Cancel = 4,
        ForceProcessing = 5,
    }

    public enum QType : int
    {
        GameAnalysis
    }
}

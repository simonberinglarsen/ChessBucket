using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessBucket.Models.AdminViewModels
{
    public class QueueViewModel
    {
        public List<QueueItemViewModel> QueueItems { get; set; }
    }

    public class QueueItemViewModel
    {
        public int Id { get; set; }
        public string QState { get; set; }
        public string QType { get; set; }
        public string Reference { get; set; }
        public string CreatedAtDate { get; set; }
        public string ModifiedAtDate { get; set; }
        public string ProcessingAtDate { get; set; }
        public string CreatedAtTimeSpan { get; set; }
        public string ModifiedAtTimeSpan { get; set; }
        public string ProcessingAtTimeSpan { get; set; }
        public TimeSpan? ElapsedProcessingTime { get; set; }
        public bool IsProcessing { get; set; }
        public string Error { get; set; }
        public string Info { get; internal set; }

        public static QueueItemViewModel CreateFrom(BatchQueueItem item)
        {
            QueueItemViewModel vm = new QueueItemViewModel();
            vm.Id = item.Id;
            vm.QState = item.QState.ToString();
            vm.QType = item.QType.ToString();
            vm.Reference = item.Reference;
            vm.CreatedAtDate = item.CreatedAt.ToString();
            vm.ModifiedAtDate = item.ModifiedAt.ToString();
            vm.ProcessingAtDate = item.ProcessingAt.ToString(); 
            vm.CreatedAtTimeSpan = item.CreatedAt.TimeOfDay.ToString();
            vm.ModifiedAtTimeSpan = item.ModifiedAt.TimeOfDay.ToString();
            vm.ProcessingAtTimeSpan = item.ProcessingAt.HasValue ? item.ProcessingAt.Value.TimeOfDay.ToString() : "";
            vm.ElapsedProcessingTime = item.ProcessingAt.HasValue ? (item.ModifiedAt - item.ProcessingAt.Value) : (TimeSpan?)null;
            vm.IsProcessing = item.QState == Models.QState.Processing;
            vm.Error = item.Error;
            vm.Info = item.Info;
            return vm;

        }
    }
}

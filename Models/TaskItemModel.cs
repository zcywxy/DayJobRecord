using System;
using System.ComponentModel;

namespace DayJobRecord.Models
{
    public class TaskItemModel : INotifyPropertyChanged
    {
        private int _id;
        private int _taskId;
        private string _content;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private bool _isSelected;
        private bool _isReportItem = true;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public int TaskId
        {
            get => _taskId;
            set { _taskId = value; OnPropertyChanged(nameof(TaskId)); }
        }

        public string Content
        {
            get => _content;
            set { _content = value; OnPropertyChanged(nameof(Content)); }
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(nameof(StartDate)); OnPropertyChanged(nameof(StartDateDisplay)); }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(nameof(EndDate)); OnPropertyChanged(nameof(EndDateDisplay)); }
        }

        public string StartDateDisplay => StartDate?.ToString("MM-dd") ?? "";
        public string EndDateDisplay => EndDate?.ToString("MM-dd") ?? "";

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public bool IsReportItem
        {
            get => _isReportItem;
            set { _isReportItem = value; OnPropertyChanged(nameof(IsReportItem)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

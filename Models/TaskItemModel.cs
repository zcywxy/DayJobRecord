using System;
using System.ComponentModel;

namespace DayJobRecord.Models
{
    public class TaskItemModel : INotifyPropertyChanged
    {
        private int _id;
        private int _taskId;
        private string _content;
        private string _completeDate;
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

        public string CompleteDate
        {
            get => _completeDate;
            set { _completeDate = value; OnPropertyChanged(nameof(CompleteDate)); }
        }

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

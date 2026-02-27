using System;
using System.ComponentModel;

namespace DayJobRecord.Models
{
    public class TaskModel : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private TaskType _taskType;
        private string _status;
        private int _priority;
        private bool _isVisible = true;
        private bool _isSelected;
        private bool _isShow = true;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public TaskType TaskType
        {
            get => _taskType;
            set { _taskType = value; OnPropertyChanged(nameof(TaskType)); OnPropertyChanged(nameof(TaskTypeDisplay)); }
        }

        public string TaskTypeDisplay => TaskType == TaskType.Development ? "开发任务" : "问题处理";

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public int Priority
        {
            get => _priority;
            set { _priority = value; OnPropertyChanged(nameof(Priority)); }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public bool IsShow
        {
            get => _isShow;
            set { _isShow = value; OnPropertyChanged(nameof(IsShow)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

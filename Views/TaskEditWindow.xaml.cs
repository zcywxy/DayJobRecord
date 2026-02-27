using System.Windows;
using DayJobRecord.Models;

namespace DayJobRecord.Views
{
    public partial class TaskEditWindow : Window
    {
        public TaskModel Task { get; private set; }

        public string TaskName
        {
            get => Task.Name;
            set => Task.Name = value;
        }

        public int TaskTypeIndex
        {
            get => (int)Task.TaskType;
            set => Task.TaskType = (TaskType)value;
        }

        public string Status
        {
            get => Task.Status;
            set => Task.Status = value;
        }

        public string PriorityText
        {
            get => Task.Priority.ToString();
            set
            {
                if (int.TryParse(value, out int result))
                {
                    Task.Priority = result;
                }
            }
        }

        public bool IsShow
        {
            get => Task.IsShow;
            set => Task.IsShow = value;
        }

        public TaskEditWindow() : this(null)
        {
        }

        public TaskEditWindow(TaskModel task)
        {
            InitializeComponent();
            Task = task ?? new TaskModel { Priority = 0, IsShow = true };
            DataContext = this;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskName))
            {
                MessageBox.Show("请输入任务名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

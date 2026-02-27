using System.Windows;
using DayJobRecord.Models;

namespace DayJobRecord.Views
{
    public partial class TaskItemEditWindow : Window
    {
        public TaskItemModel TaskItem { get; private set; }

        public string ItemContent
        {
            get => TaskItem.Content;
            set => TaskItem.Content = value;
        }

        public string ItemCompleteDate
        {
            get => TaskItem.CompleteDate;
            set => TaskItem.CompleteDate = value;
        }

        public bool IsReportItem
        {
            get => TaskItem.IsReportItem;
            set => TaskItem.IsReportItem = value;
        }

        public TaskItemEditWindow(int taskId) : this(taskId, null)
        {
        }

        public TaskItemEditWindow(int taskId, TaskItemModel item)
        {
            InitializeComponent();
            TaskItem = item ?? new TaskItemModel { TaskId = taskId, IsReportItem = true };
            DataContext = this;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemContent))
            {
                MessageBox.Show("请输入完成内容", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DayJobRecord.Models;
using DayJobRecord.Services;
using MaterialDesignThemes.Wpf;

namespace DayJobRecord.Views
{
    public partial class TaskEditWindow : Window
    {
        private readonly ConfigService _config = ConfigService.Instance;

        public TaskModel Task { get; private set; }

        public List<TaskTypeOption> TaskTypes => _config.GetTaskTypes();
        public List<string> Statuses => _config.GetStatuses();
        public List<PriorityOption> Priorities => _config.GetPriorities();

        public string TaskName
        {
            get => Task.Name;
            set => Task.Name = value;
        }

        public int TaskType
        {
            get => (int)Task.TaskType;
            set => Task.TaskType = (TaskType)value;
        }

        public string Status
        {
            get => Task.Status;
            set => Task.Status = value;
        }

        public int Priority
        {
            get => Task.Priority;
            set => Task.Priority = value;
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
            Task = task ?? new TaskModel { Priority = 0, IsShow = true, Status = "" };
            DataContext = this;
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskName))
            {
                await ShowMessageDialog("请输入任务名称");
                return;
            }
            DialogResult = true;
        }

        private async System.Threading.Tasks.Task ShowMessageDialog(string message)
        {
            var view = new StackPanel
            {
                Margin = new Thickness(24),
                Width = 200
            };
            var textBlock = new TextBlock
            {
                Text = message,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16)
            };
            var button = new Button
            {
                Content = "确定",
                Style = (Style)FindResource("MaterialDesignRaisedButton"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Command = DialogHost.CloseDialogCommand,
                CommandParameter = true
            };
            view.Children.Add(textBlock);
            view.Children.Add(button);
            await DialogHost.Show(view, "TaskEditDialog");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

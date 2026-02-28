using System.Windows;
using System.Windows.Controls;
using DayJobRecord.Models;
using MaterialDesignThemes.Wpf;

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

        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemContent))
            {
                await ShowMessageDialog("请输入完成内容");
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
            await DialogHost.Show(view, "TaskItemEditDialog");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

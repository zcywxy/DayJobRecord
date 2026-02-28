using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using DayJobRecord.Models;
using DayJobRecord.Services;
using MaterialDesignThemes.Wpf;

namespace DayJobRecord.Views
{
    public partial class DailyReportWindow : Window
    {
        public string ReportText { get; private set; }

        public DailyReportWindow(List<TaskModel> selectedTasks, DatabaseService db)
        {
            InitializeComponent();
            ReportText = GenerateReport(selectedTasks, db);
            DataContext = this;
        }

        private string GenerateReport(List<TaskModel> tasks, DatabaseService db)
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var devTasks = tasks.Where(t => t.TaskType == TaskType.Development).ToList();
            var issueTasks = tasks.Where(t => t.TaskType == TaskType.Issue).ToList();

            if (devTasks.Any())
            {
                sb.AppendLine("开发任务：");
                int index = 1;
                foreach (var task in devTasks)
                {
                    sb.AppendLine($"{index}、{task.Name}");
                    sb.AppendLine($"\t状态：{task.Status}");
                    
                    var items = db.GetTaskItemsByTaskId(task.Id);
                    var reportItems = items.Where(i => i.IsReportItem).ToList();
                    foreach (var item in reportItems)
                    {
                        var dateStr = item.EndDate?.ToString("MM-dd") ?? "";
                        if (!string.IsNullOrEmpty(dateStr))
                        {
                            sb.AppendLine($"\t{dateStr}：{item.Content}");
                        }
                        else
                        {
                            sb.AppendLine($"\t{item.Content}");
                        }
                    }
                    index++;
                }
            }

            if (issueTasks.Any())
            {
                sb.AppendLine("问题处理：");
                int index = 1;
                foreach (var task in issueTasks)
                {
                    sb.AppendLine($"{index}、{task.Name}");
                    sb.AppendLine($"\t状态：{task.Status}");

                    var items = db.GetTaskItemsByTaskId(task.Id);
                    var reportItems = items.Where(i => i.IsReportItem).ToList();
                    foreach (var item in reportItems)
                    {
                        var dateStr = item.EndDate?.ToString("MM-dd") ?? "";
                        if (!string.IsNullOrEmpty(dateStr))
                        {
                            sb.AppendLine($"\t{dateStr}处理：{item.Content}");
                        }
                        else
                        {
                            sb.AppendLine($"\t{item.Content}");
                        }
                    }
                    index++;
                }
            }

            return sb.ToString();
        }

        private async void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ReportText);
            var view = new StackPanel
            {
                Margin = new Thickness(24),
                Width = 200
            };
            var textBlock = new TextBlock
            {
                Text = "日报已复制到剪贴板",
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
            await DialogHost.Show(view, "RootDialog");
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

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
    public enum ReportType
    {
        DailyReport,
        PerformanceSummary
    }

    public partial class DailyReportWindow : Window
    {
        public string ReportText { get; private set; }
        public string WindowTitle { get; private set; }

        public DailyReportWindow(List<TaskModel> selectedTasks, DatabaseService db, ReportType reportType = ReportType.DailyReport)
        {
            InitializeComponent();
            WindowTitle = reportType == ReportType.DailyReport ? "日报预览" : "绩效总结预览";
            ReportText = reportType == ReportType.DailyReport 
                ? GenerateDailyReport(selectedTasks, db) 
                : GeneratePerformanceSummary(selectedTasks, db);
            DataContext = this;
        }

        private string GenerateDailyReport(List<TaskModel> tasks, DatabaseService db)
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var config = ConfigService.Instance;
            var taskTypes = config.GetTaskTypes();

            foreach (var taskType in taskTypes)
            {
                var tasksOfType = tasks.Where(t => t.TaskType == taskType.Value).ToList();
                if (!tasksOfType.Any()) continue;

                sb.AppendLine($"{taskType.Display}：");
                int index = 1;
                foreach (var task in tasksOfType)
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

            return sb.ToString();
        }

        private string GeneratePerformanceSummary(List<TaskModel> tasks, DatabaseService db)
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var tasksByProject = tasks.GroupBy(t => t.Project ?? "").OrderBy(g => g.Key);

            foreach (var projectGroup in tasksByProject)
            {
                var projectName = string.IsNullOrEmpty(projectGroup.Key) ? "其他" : projectGroup.Key;
                sb.AppendLine($"本月完成了{projectName}的如下任务：");
                
                int taskIndex = 1;
                foreach (var task in projectGroup)
                {
                    sb.AppendLine($"{taskIndex}、{task.Name}");
                    
                    var items = db.GetTaskItemsByTaskId(task.Id);
                    var reportItems = items.Where(i => i.IsReportItem).ToList();
                    
                    if (reportItems.Any())
                    {
                        int itemIndex = 1;
                        foreach (var item in reportItems)
                        {
                            sb.AppendLine($"\t{itemIndex}) {item.Content}；");
                            itemIndex++;
                        }
                    }
                    taskIndex++;
                }
                sb.AppendLine();
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
                Text = "内容已复制到剪贴板",
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

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DayJobRecord.Models;
using DayJobRecord.Services;
using DayJobRecord.Views;
using MaterialDesignThemes.Wpf;

namespace DayJobRecord.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _db;
        private readonly ConfigService _config = ConfigService.Instance;
        private ObservableCollection<TaskModel> _tasks;
        private ObservableCollection<TaskItemModel> _taskItems;
        private TaskModel _selectedTask;
        private TaskItemModel _selectedTaskItem;
        private string _filterStatus = "";
        private string _filterName = "";
        private string _filterProject = "";
        private ShowStatus _selectedShowStatus = ShowStatus.All;

        public enum ShowStatus
        {
            All,
            ShowOnly,
            HideOnly
        }

        public class ShowStatusOption
        {
            public ShowStatus Value { get; set; }
            public string Display { get; set; }
            public string Icon { get; set; }
        }

        public ObservableCollection<ShowStatusOption> ShowStatusOptions { get; }

        public ShowStatusOption SelectedShowStatusOption
        {
            get => ShowStatusOptions.FirstOrDefault(o => o.Value == _selectedShowStatus);
            set
            {
                if (value != null && value.Value != _selectedShowStatus)
                {
                    _selectedShowStatus = value.Value;
                    OnPropertyChanged(nameof(SelectedShowStatusOption));
                    OnPropertyChanged(nameof(SelectedShowStatus));
                    FilterTasks();
                }
            }
        }

        public ShowStatus SelectedShowStatus
        {
            get => _selectedShowStatus;
            set
            {
                _selectedShowStatus = value;
                OnPropertyChanged(nameof(SelectedShowStatus));
                OnPropertyChanged(nameof(SelectedShowStatusOption));
                FilterTasks();
            }
        }

        public ObservableCollection<TaskModel> Tasks
        {
            get => _tasks;
            set { _tasks = value; OnPropertyChanged(nameof(Tasks)); }
        }

        public ObservableCollection<TaskItemModel> TaskItems
        {
            get => _taskItems;
            set { _taskItems = value; OnPropertyChanged(nameof(TaskItems)); }
        }

        public TaskModel SelectedTask
        {
            get => _selectedTask;
            set
            {
                if (_selectedTask != null)
                {
                    _selectedTask.PropertyChanged -= OnTaskPropertyChanged;
                }
                _selectedTask = value;
                if (_selectedTask != null)
                {
                    _selectedTask.PropertyChanged += OnTaskPropertyChanged;
                }
                OnPropertyChanged(nameof(SelectedTask));
                LoadTaskItems();
                RaiseTaskCommandsCanExecuteChanged();
            }
        }

        public TaskItemModel SelectedTaskItem
        {
            get => _selectedTaskItem;
            set
            {
                _selectedTaskItem = value;
                OnPropertyChanged(nameof(SelectedTaskItem));
                RaiseTaskItemCommandsCanExecuteChanged();
            }
        }

        public ObservableCollection<string> Statuses { get; }
        public ObservableCollection<string> Projects { get; }

        public string FilterStatus
        {
            get => _filterStatus;
            set
            {
                _filterStatus = value ?? "";
                OnPropertyChanged(nameof(FilterStatus));
                FilterTasks();
            }
        }

        public string FilterName
        {
            get => _filterName;
            set
            {
                _filterName = value ?? "";
                OnPropertyChanged(nameof(FilterName));
                FilterTasks();
            }
        }

        public string FilterProject
        {
            get => _filterProject;
            set
            {
                _filterProject = value ?? "";
                OnPropertyChanged(nameof(FilterProject));
                FilterTasks();
            }
        }

        public RelayCommand AddTaskCommand { get; }
        public RelayCommand EditTaskCommand { get; }
        public RelayCommand DeleteTaskCommand { get; }
        public RelayCommand AddTaskItemCommand { get; }
        public RelayCommand EditTaskItemCommand { get; }
        public RelayCommand DeleteTaskItemCommand { get; }
        public RelayCommand GenerateReportCommand { get; }
        public RelayCommand GeneratePerformanceSummaryCommand { get; }
        public RelayCommand InvertSelectionCommand { get; }
        public RelayCommand ClearSelectionCommand { get; }
        public RelayCommand<TaskModel> ToggleShowCommand { get; }

        public MainViewModel()
        {
            _db = DatabaseService.Instance;
            Tasks = new ObservableCollection<TaskModel>();
            TaskItems = new ObservableCollection<TaskItemModel>();
            Statuses = new ObservableCollection<string> { "" };
            foreach (var status in _config.GetStatuses())
            {
                Statuses.Add(status);
            }

            Projects = new ObservableCollection<string> { "" };
            foreach (var project in _config.GetProjects())
            {
                Projects.Add(project);
            }

            ShowStatusOptions = new ObservableCollection<ShowStatusOption>
            {
                new ShowStatusOption { Value = ShowStatus.All, Display = "全显示", Icon = "Eye" },
                new ShowStatusOption { Value = ShowStatus.ShowOnly, Display = "显示", Icon = "Visibility" },
                new ShowStatusOption { Value = ShowStatus.HideOnly, Display = "不显示", Icon = "VisibilityOff" }
            };
            _selectedShowStatus = ShowStatus.All;

            AddTaskCommand = new RelayCommand(AddTask);
            EditTaskCommand = new RelayCommand(EditTask, CanEditTask);
            DeleteTaskCommand = new RelayCommand(DeleteTask, CanDeleteTask);
            AddTaskItemCommand = new RelayCommand(AddTaskItem, CanAddTaskItem);
            EditTaskItemCommand = new RelayCommand(EditTaskItem, CanEditTaskItem);
            DeleteTaskItemCommand = new RelayCommand(DeleteTaskItem, CanDeleteTaskItem);
            GenerateReportCommand = new RelayCommand(GenerateReport, CanGenerateReport);
            GeneratePerformanceSummaryCommand = new RelayCommand(GeneratePerformanceSummary, CanGenerateReport);
            InvertSelectionCommand = new RelayCommand(InvertSelection);
            ClearSelectionCommand = new RelayCommand(ClearSelection);
            ToggleShowCommand = new RelayCommand<TaskModel>(ToggleShow);

            LoadTasks();
        }

        private void OnTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TaskModel.IsSelected))
            {
                GenerateReportCommand.RaiseCanExecuteChanged();
                GeneratePerformanceSummaryCommand.RaiseCanExecuteChanged();
            }
        }

        private void RaiseTaskCommandsCanExecuteChanged()
        {
            EditTaskCommand.RaiseCanExecuteChanged();
            DeleteTaskCommand.RaiseCanExecuteChanged();
            AddTaskItemCommand.RaiseCanExecuteChanged();
        }

        private void RaiseTaskItemCommandsCanExecuteChanged()
        {
            EditTaskItemCommand.RaiseCanExecuteChanged();
            DeleteTaskItemCommand.RaiseCanExecuteChanged();
        }

        private void LoadTasks()
        {
            Tasks.Clear();
            var tasks = _db.GetAllTasks();
            foreach (var task in tasks)
            {
                task.PropertyChanged += OnTaskPropertyChanged;
                Tasks.Add(task);
            }
            FilterTasks();
            GenerateReportCommand.RaiseCanExecuteChanged();
            GeneratePerformanceSummaryCommand.RaiseCanExecuteChanged();
        }

        private void FilterTasks()
        {
            foreach (var task in Tasks)
            {
                var isVisibleByStatus = string.IsNullOrEmpty(FilterStatus) || task.Status == FilterStatus;
                var isVisibleByProject = string.IsNullOrEmpty(FilterProject) || task.Project == FilterProject;
                var isVisibleByName = string.IsNullOrEmpty(FilterName) || 
                    (task.Name?.Contains(FilterName) ?? false);
                
                bool isVisibleByShowStatus;
                switch (SelectedShowStatus)
                {
                    case ShowStatus.ShowOnly:
                        isVisibleByShowStatus = task.IsShow;
                        break;
                    case ShowStatus.HideOnly:
                        isVisibleByShowStatus = !task.IsShow;
                        break;
                    default:
                        isVisibleByShowStatus = true;
                        break;
                }
                
                task.IsVisible = isVisibleByStatus && isVisibleByProject && isVisibleByName && isVisibleByShowStatus;
            }
        }

        private void LoadTaskItems()
        {
            TaskItems.Clear();
            SelectedTaskItem = null;
            if (SelectedTask != null)
            {
                var items = _db.GetTaskItemsByTaskId(SelectedTask.Id);
                foreach (var item in items)
                {
                    TaskItems.Add(item);
                }
            }
        }

        private void AddTask()
        {
            var window = new TaskEditWindow();
            if (window.ShowDialog() == true)
            {
                var task = window.Task;
                task.CreatedAt = DateTime.Now;
                task.Id = _db.AddTask(task);
                task.PropertyChanged += OnTaskPropertyChanged;
                Tasks.Add(task);
                SortTasks();
                SelectedTask = task;
            }
        }

        private bool CanEditTask() => SelectedTask != null;

        private void EditTask()
        {
            if (SelectedTask == null) return;
            var window = new TaskEditWindow(SelectedTask);
            if (window.ShowDialog() == true)
            {
                _db.UpdateTask(SelectedTask);
                SortTasks();
                FilterTasks();
            }
        }

        private bool CanDeleteTask() => SelectedTask != null;

        private async void DeleteTask()
        {
            if (SelectedTask == null) return;
            var taskToDelete = SelectedTask;
            if (await ShowConfirmDialog($"确定要删除任务 \"{taskToDelete.Name}\" 吗？"))
            {
                taskToDelete.PropertyChanged -= OnTaskPropertyChanged;
                _db.DeleteTask(taskToDelete.Id);
                Tasks.Remove(taskToDelete);
                TaskItems.Clear();
                SelectedTask = null;
                GenerateReportCommand.RaiseCanExecuteChanged();
                GeneratePerformanceSummaryCommand.RaiseCanExecuteChanged();
            }
        }

        private bool CanAddTaskItem() => SelectedTask != null;

        private void InvertSelection()
        {
            foreach (var task in Tasks)
            {
                if (task.IsVisible)
                {
                    task.IsSelected = !task.IsSelected;
                }
                else
                {
                    task.IsSelected = false;
                }
            }
        }

        private void ClearSelection()
        {
            foreach (var task in Tasks)
            {
                task.IsSelected = false;
            }
        }

        private void ToggleShow(TaskModel task)
        {
            if (task != null)
            {
                task.IsShow = !task.IsShow;
                _db.UpdateTask(task);
            }
        }

        private void AddTaskItem()
        {
            if (SelectedTask == null) return;
            var window = new TaskItemEditWindow(SelectedTask.Id);
            if (window.ShowDialog() == true)
            {
                var item = window.TaskItem;
                item.Id = _db.AddTaskItem(item);
                TaskItems.Insert(0, item);
            }
        }

        private bool CanEditTaskItem() => SelectedTaskItem != null;

        private void EditTaskItem()
        {
            if (SelectedTaskItem == null) return;
            var window = new TaskItemEditWindow(SelectedTaskItem.TaskId, SelectedTaskItem);
            if (window.ShowDialog() == true)
            {
                _db.UpdateTaskItem(SelectedTaskItem);
            }
        }

        private bool CanDeleteTaskItem() => SelectedTaskItem != null;

        private async void DeleteTaskItem()
        {
            if (SelectedTaskItem == null) return;
            if (await ShowConfirmDialog("确定要删除该任务细项吗？"))
            {
                _db.DeleteTaskItem(SelectedTaskItem.Id);
                TaskItems.Remove(SelectedTaskItem);
                SelectedTaskItem = null;
            }
        }

        private bool CanGenerateReport()
        {
            return Tasks.Any(t => t.IsSelected);
        }

        private async void GenerateReport()
        {
            var selectedTasks = Tasks.Where(t => t.IsSelected).OrderByDescending(t => t.Priority).ToList();
            if (!selectedTasks.Any())
            {
                await ShowMessageDialog("请先勾选要生成日报的任务");
                return;
            }

            var reportWindow = new DailyReportWindow(selectedTasks, _db, ReportType.DailyReport);
            reportWindow.ShowDialog();
        }

        private async void GeneratePerformanceSummary()
        {
            var selectedTasks = Tasks.Where(t => t.IsSelected).OrderByDescending(t => t.Priority).ToList();
            if (!selectedTasks.Any())
            {
                await ShowMessageDialog("请先勾选要生成绩效总结的任务");
                return;
            }

            var reportWindow = new DailyReportWindow(selectedTasks, _db, ReportType.PerformanceSummary);
            reportWindow.ShowDialog();
        }

        private async Task<bool> ShowConfirmDialog(string message)
        {
            var view = new StackPanel
            {
                Margin = new Thickness(24),
                Width = 260
            };
            var textBlock = new TextBlock
            {
                Text = message,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var cancelButton = new Button
            {
                Content = "取消",
                Style = Application.Current.FindResource("MaterialDesignOutlinedButton") as Style,
                Width = 70,
                Margin = new Thickness(0, 0, 12, 0),
                Command = DialogHost.CloseDialogCommand,
                CommandParameter = false
            };
            var confirmButton = new Button
            {
                Content = "确定",
                Style = Application.Current.FindResource("MaterialDesignRaisedButton") as Style,
                Width = 70,
                Command = DialogHost.CloseDialogCommand,
                CommandParameter = true
            };
            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(confirmButton);
            view.Children.Add(textBlock);
            view.Children.Add(buttonPanel);
            var result = await DialogHost.Show(view, "MainDialog");
            return result is bool b && b;
        }

        private async Task ShowMessageDialog(string message)
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
                Style = Application.Current.FindResource("MaterialDesignRaisedButton") as Style,
                HorizontalAlignment = HorizontalAlignment.Center,
                Command = DialogHost.CloseDialogCommand,
                CommandParameter = true
            };
            view.Children.Add(textBlock);
            view.Children.Add(button);
            await DialogHost.Show(view, "MainDialog");
        }

        private void SortTasks()
        {
            var sorted = Tasks.OrderByDescending(t => t.Priority).ThenBy(t => t.Id).ToList();
            Tasks.Clear();
            foreach (var task in sorted)
            {
                Tasks.Add(task);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : System.Windows.Input.ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _execute();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class RelayCommand<T> : System.Windows.Input.ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _execute((T)parameter);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

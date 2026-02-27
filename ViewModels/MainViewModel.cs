using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using DayJobRecord.Models;
using DayJobRecord.Services;
using DayJobRecord.Views;

namespace DayJobRecord.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _db;
        private ObservableCollection<TaskModel> _tasks;
        private ObservableCollection<TaskItemModel> _taskItems;
        private TaskModel _selectedTask;
        private TaskItemModel _selectedTaskItem;
        private bool _showAllTasks = true;

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

        public bool ShowAllTasks
        {
            get => _showAllTasks;
            set
            {
                _showAllTasks = value;
                OnPropertyChanged(nameof(ShowAllTasks));
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

        public MainViewModel()
        {
            _db = DatabaseService.Instance;
            Tasks = new ObservableCollection<TaskModel>();
            TaskItems = new ObservableCollection<TaskItemModel>();

            AddTaskCommand = new RelayCommand(AddTask);
            EditTaskCommand = new RelayCommand(EditTask, CanEditTask);
            DeleteTaskCommand = new RelayCommand(DeleteTask, CanDeleteTask);
            AddTaskItemCommand = new RelayCommand(AddTaskItem, CanAddTaskItem);
            EditTaskItemCommand = new RelayCommand(EditTaskItem, CanEditTaskItem);
            DeleteTaskItemCommand = new RelayCommand(DeleteTaskItem, CanDeleteTaskItem);
            GenerateReportCommand = new RelayCommand(GenerateReport, CanGenerateReport);

            LoadTasks();
        }

        private void OnTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TaskModel.IsSelected))
            {
                GenerateReportCommand.RaiseCanExecuteChanged();
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
        }

        private void FilterTasks()
        {
            foreach (var task in Tasks)
            {
                task.IsVisible = ShowAllTasks || task.IsShow;
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
                task.Id = _db.AddTask(task);
                task.PropertyChanged += OnTaskPropertyChanged;
                Tasks.Add(task);
                SortTasks();
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

        private void DeleteTask()
        {
            if (SelectedTask == null) return;
            var taskToDelete = SelectedTask;
            if (MessageBox.Show($"确定要删除任务 \"{taskToDelete.Name}\" 吗？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                taskToDelete.PropertyChanged -= OnTaskPropertyChanged;
                _db.DeleteTask(taskToDelete.Id);
                Tasks.Remove(taskToDelete);
                TaskItems.Clear();
                SelectedTask = null;
                GenerateReportCommand.RaiseCanExecuteChanged();
            }
        }

        private bool CanAddTaskItem() => SelectedTask != null;

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

        private void DeleteTaskItem()
        {
            if (SelectedTaskItem == null) return;
            if (MessageBox.Show($"确定要删除该任务细项吗？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
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

        private void GenerateReport()
        {
            var selectedTasks = Tasks.Where(t => t.IsSelected).OrderByDescending(t => t.Priority).ToList();
            if (!selectedTasks.Any())
            {
                MessageBox.Show("请先勾选要生成日报的任务", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var reportWindow = new DailyReportWindow(selectedTasks, _db);
            reportWindow.ShowDialog();
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
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DayJobRecord.Services
{
    public class DropdownOptionsConfig
    {
        public List<TaskTypeOption> TaskTypes { get; set; }
        public List<string> Statuses { get; set; }
        public List<PriorityOption> Priorities { get; set; }
        public List<string> Projects { get; set; }
    }

    public class TaskTypeOption
    {
        public int Value { get; set; }
        public string Display { get; set; }
    }

    public class PriorityOption
    {
        public int Value { get; set; }
        public string Display { get; set; }
    }

    public class ConfigService
    {
        private static ConfigService _instance;
        private static readonly object _lock = new object();
        private readonly string _configPath;
        private DropdownOptionsConfig _config;

        public static ConfigService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConfigService();
                        }
                    }
                }
                return _instance;
            }
        }

        private ConfigService()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "DropdownOptions.json");
            LoadConfig();
        }

        private void LoadConfig()
        {
            if (File.Exists(_configPath))
            {
                try
                {
                    var json = File.ReadAllText(_configPath);
                    _config = JsonSerializer.Deserialize<DropdownOptionsConfig>(json);
                    if (_config != null)
                    {
                        if (_config.Projects == null)
                        {
                            _config.Projects = GetDefaultProjects();
                            SaveConfig();
                        }
                        return;
                    }
                }
                catch
                {
                }
            }

            _config = GetDefaultConfig();
            SaveConfig();
        }

        private void SaveConfig()
        {
            var directory = Path.GetDirectoryName(_configPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_config, options);
            File.WriteAllText(_configPath, json);
        }

        private List<string> GetDefaultProjects()
        {
            return new List<string>
            {
                "内部项目",
                "项目A",
                "项目B"
            };
        }

        private DropdownOptionsConfig GetDefaultConfig()
        {
            return new DropdownOptionsConfig
            {
                TaskTypes = new List<TaskTypeOption>
                {
                    new TaskTypeOption { Value = 0, Display = "开发任务" },
                    new TaskTypeOption { Value = 1, Display = "问题处理" }
                },
                Statuses = new List<string>
                {
                    "进行中",
                    "已完成",
                    "暂停",
                    "待开始",
                    "已取消"
                },
                Priorities = new List<PriorityOption>
                {
                    new PriorityOption { Value = 0, Display = "普通" },
                    new PriorityOption { Value = 1, Display = "较高" },
                    new PriorityOption { Value = 2, Display = "高" },
                    new PriorityOption { Value = 3, Display = "紧急" }
                },
                Projects = GetDefaultProjects()
            };
        }

        public List<TaskTypeOption> GetTaskTypes() => _config.TaskTypes;
        public List<string> GetStatuses() => _config.Statuses;
        public List<PriorityOption> GetPriorities() => _config.Priorities;
        public List<string> GetProjects() => _config.Projects;
    }
}

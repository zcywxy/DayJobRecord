using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using DayJobRecord.Models;

namespace DayJobRecord.Services
{
    public class DatabaseService
    {
        private static DatabaseService _instance;
        private static readonly object _lock = new object();
        private readonly string _dbPath;
        private readonly string _connectionString;

        public static DatabaseService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DatabaseService();
                        }
                    }
                }
                return _instance;
            }
        }

        private DatabaseService()
        {
            _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DayJobRecord.db");
            _connectionString = $"Data Source={_dbPath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(_dbPath))
            {
                SQLiteConnection.CreateFile(_dbPath);
            }

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string createTaskTable = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        TaskType INTEGER NOT NULL,
                        Status TEXT,
                        Priority INTEGER DEFAULT 0,
                        IsShow INTEGER DEFAULT 1
                    )";
                string createTaskItemTable = @"
                    CREATE TABLE IF NOT EXISTS TaskItems (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        TaskId INTEGER NOT NULL,
                        Content TEXT,
                        CompleteDate TEXT,
                        IsReportItem INTEGER DEFAULT 1,
                        FOREIGN KEY(TaskId) REFERENCES Tasks(Id)
                    )";

                using (var command = new SQLiteCommand(createTaskTable, connection))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand(createTaskItemTable, connection))
                {
                    command.ExecuteNonQuery();
                }
                
                AddColumnIfNotExists(connection, "TaskItems", "IsReportItem", "INTEGER DEFAULT 1");
                AddColumnIfNotExists(connection, "Tasks", "CreatedAt", "TEXT DEFAULT ''");
                AddColumnIfNotExists(connection, "Tasks", "Project", "TEXT DEFAULT ''");
                AddColumnIfNotExists(connection, "TaskItems", "StartDate", "TEXT DEFAULT ''");
                AddColumnIfNotExists(connection, "TaskItems", "EndDate", "TEXT DEFAULT ''");
            }
        }

        private void AddColumnIfNotExists(SQLiteConnection connection, string tableName, string columnName, string columnDef)
        {
            string checkSql = $"PRAGMA table_info({tableName})";
            bool columnExists = false;
            using (var command = new SQLiteCommand(checkSql, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader["name"].ToString() == columnName)
                    {
                        columnExists = true;
                        break;
                    }
                }
            }
            
            if (!columnExists)
            {
                string addColumnSql = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDef}";
                using (var command = new SQLiteCommand(addColumnSql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<TaskModel> GetAllTasks()
        {
            var tasks = new List<TaskModel>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM Tasks ORDER BY Priority DESC, Id";
                using (var command = new SQLiteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var task = new TaskModel
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            TaskType = (TaskType)Convert.ToInt32(reader["TaskType"]),
                            Status = reader["Status"]?.ToString() ?? "",
                            Priority = Convert.ToInt32(reader["Priority"]),
                            IsShow = Convert.ToInt32(reader["IsShow"]) == 1
                        };
                        
                        if (reader["CreatedAt"] != DBNull.Value && DateTime.TryParse(reader["CreatedAt"]?.ToString(), out var createdAt))
                        {
                            task.CreatedAt = createdAt;
                        }
                        else
                        {
                            task.CreatedAt = DateTime.MinValue;
                        }
                        
                        task.Project = reader["Project"]?.ToString() ?? "";
                        
                        tasks.Add(task);
                    }
                }
            }
            return tasks;
        }

        public int AddTask(TaskModel task)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "INSERT INTO Tasks (Name, TaskType, Status, Priority, IsShow, CreatedAt, Project) VALUES (@Name, @TaskType, @Status, @Priority, @IsShow, @CreatedAt, @Project); SELECT last_insert_rowid();";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", task.Name);
                    command.Parameters.AddWithValue("@TaskType", (int)task.TaskType);
                    command.Parameters.AddWithValue("@Status", task.Status ?? "");
                    command.Parameters.AddWithValue("@Priority", task.Priority);
                    command.Parameters.AddWithValue("@IsShow", task.IsShow ? 1 : 0);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@Project", task.Project ?? "");
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public void UpdateTask(TaskModel task)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "UPDATE Tasks SET Name = @Name, TaskType = @TaskType, Status = @Status, Priority = @Priority, IsShow = @IsShow, Project = @Project WHERE Id = @Id";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", task.Name);
                    command.Parameters.AddWithValue("@TaskType", (int)task.TaskType);
                    command.Parameters.AddWithValue("@Status", task.Status ?? "");
                    command.Parameters.AddWithValue("@Priority", task.Priority);
                    command.Parameters.AddWithValue("@IsShow", task.IsShow ? 1 : 0);
                    command.Parameters.AddWithValue("@Project", task.Project ?? "");
                    command.Parameters.AddWithValue("@Id", task.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteTask(int taskId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = new SQLiteCommand("DELETE FROM TaskItems WHERE TaskId = @TaskId", connection, transaction))
                    {
                        command.Parameters.AddWithValue("@TaskId", taskId);
                        command.ExecuteNonQuery();
                    }
                    using (var command = new SQLiteCommand("DELETE FROM Tasks WHERE Id = @Id", connection, transaction))
                    {
                        command.Parameters.AddWithValue("@Id", taskId);
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }

        public List<TaskItemModel> GetTaskItemsByTaskId(int taskId)
        {
            var items = new List<TaskItemModel>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM TaskItems WHERE TaskId = @TaskId ORDER BY Id DESC";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@TaskId", taskId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new TaskItemModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                TaskId = Convert.ToInt32(reader["TaskId"]),
                                Content = reader["Content"]?.ToString() ?? ""
                            };
                            
                            if (reader["IsReportItem"] != DBNull.Value)
                            {
                                item.IsReportItem = Convert.ToInt32(reader["IsReportItem"]) == 1;
                            }
                            
                            if (reader["StartDate"] != DBNull.Value && DateTime.TryParse(reader["StartDate"]?.ToString(), out var startDate))
                            {
                                item.StartDate = startDate;
                            }
                            
                            if (reader["EndDate"] != DBNull.Value && DateTime.TryParse(reader["EndDate"]?.ToString(), out var endDate))
                            {
                                item.EndDate = endDate;
                            }
                            
                            items.Add(item);
                        }
                    }
                }
            }
            return items;
        }

        public int AddTaskItem(TaskItemModel item)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "INSERT INTO TaskItems (TaskId, Content, StartDate, EndDate, IsReportItem) VALUES (@TaskId, @Content, @StartDate, @EndDate, @IsReportItem); SELECT last_insert_rowid();";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@TaskId", item.TaskId);
                    command.Parameters.AddWithValue("@Content", item.Content ?? "");
                    command.Parameters.AddWithValue("@StartDate", item.StartDate?.ToString("yyyy-MM-dd") ?? "");
                    command.Parameters.AddWithValue("@EndDate", item.EndDate?.ToString("yyyy-MM-dd") ?? "");
                    command.Parameters.AddWithValue("@IsReportItem", item.IsReportItem ? 1 : 0);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public void UpdateTaskItem(TaskItemModel item)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "UPDATE TaskItems SET Content = @Content, StartDate = @StartDate, EndDate = @EndDate, IsReportItem = @IsReportItem WHERE Id = @Id";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Content", item.Content ?? "");
                    command.Parameters.AddWithValue("@StartDate", item.StartDate?.ToString("yyyy-MM-dd") ?? "");
                    command.Parameters.AddWithValue("@EndDate", item.EndDate?.ToString("yyyy-MM-dd") ?? "");
                    command.Parameters.AddWithValue("@IsReportItem", item.IsReportItem ? 1 : 0);
                    command.Parameters.AddWithValue("@Id", item.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteTaskItem(int itemId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "DELETE FROM TaskItems WHERE Id = @Id";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", itemId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}

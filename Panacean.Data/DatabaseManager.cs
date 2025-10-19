using System;
using System.IO;
using Panacean.Data.Interface;
using FreeSql;
using Microsoft.Extensions.Logging;

namespace Panacean.Data
{
    /// <summary>
    /// 数据库管理器，负责数据库初始化和连接共享
    /// </summary>
    public class DatabaseManager : IDatabaseManager
    {
        private readonly IFreeSql _db;
        private readonly ILogger _logger;
        public string DbFilePath { get; }
        
        // 数据库修改版本号表名
        public const string VERSION_TABLE_NAME = "__db_version";
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbFileName">数据库文件名</param>
        /// <param name="cleanDatabaseOnStart">启动时是否清除现有数据库文件</param>
        public DatabaseManager(ILoggerFactory loggerFactory, string? dbFileName = null, bool cleanDatabaseOnStart = false)
        {
            // 获取日志记录器
            _logger = loggerFactory.CreateLogger<DatabaseManager>();
            
            // 初始化文件数据库
            DbFilePath = dbFileName ?? "app_database.db";
            
            // 如果需要，删除现有数据库文件
            if (cleanDatabaseOnStart)
            {
#if DEBUG
                DeleteDatabaseFile();
#endif
            }
            
            _db = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, $"Data Source={DbFilePath}")
                .UseAutoSyncStructure(true)
                .UseMonitorCommand(cmd => _logger.LogDebug("[SQL] {CommandText}", cmd.CommandText))
                .Build();
            
            // 启用WAL模式提高并发性能
            //_db.Ado.ExecuteNonQuery("PRAGMA journal_mode = WAL;");
            // 设置锁超时
            //_db.Ado.ExecuteNonQuery("PRAGMA busy_timeout = 3000;");
            
            // 创建并初始化版本跟踪表
            InitVersionTable();
            
            _logger.LogInformation("数据库管理器初始化完成，数据库路径：{DbPath}", DbFilePath);
        }
        
        /// <summary>
        /// 获取数据库访问对象
        /// </summary>
        public IFreeSql GetDatabase() => _db;
        
        /// <summary>
        /// 删除数据库文件及相关的WAL和SHM文件
        /// </summary>
        private void DeleteDatabaseFile()
        {
            try
            {
                // 获取完整路径
                string fullPath = Path.GetFullPath(DbFilePath);
                
                // 删除主数据库文件
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("已删除数据库文件: {FilePath}", fullPath);
                }
                
                // 删除WAL文件
                string walFile = fullPath + "-wal";
                if (File.Exists(walFile))
                {
                    File.Delete(walFile);
                    _logger.LogInformation("已删除WAL文件: {FilePath}", walFile);
                }
                
                // 删除SHM文件
                string shmFile = fullPath + "-shm";
                if (File.Exists(shmFile))
                {
                    File.Delete(shmFile);
                    _logger.LogInformation("已删除SHM文件: {FilePath}", shmFile);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除数据库文件失败: {Message}", ex.Message);
            }
        }
        
        /// <summary>
        /// 初始化版本跟踪表
        /// </summary>
        private void InitVersionTable()
        {
            try
            {
                // 创建版本表（如果不存在）
                string sql = $@"CREATE TABLE IF NOT EXISTS {VERSION_TABLE_NAME} (
                    id INTEGER PRIMARY KEY,
                    table_name TEXT NOT NULL,
                    version INTEGER NOT NULL,
                    last_updated DATETIME NOT NULL
                )";
                _db.Ado.ExecuteNonQuery(sql);
                
                _logger.LogDebug("初始化版本表完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化版本表失败: {Message}", ex.Message);
            }
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _logger.LogInformation("数据库管理器已释放");
            GC.SuppressFinalize(this);
        }
    }
} 
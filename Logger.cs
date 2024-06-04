using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nuoro.Utilities.LogService
{
    public static class Logger
    {
        private static readonly string _logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ErrLog.txt");
        private static readonly object _lockObject = new object();
        private static readonly BlockingCollection<string> _logQueue = new BlockingCollection<string>();

        // یک CancellationTokenSource برای ایجاد امکان لغو نخ نویسنده
        private static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        static Logger()
        {
            // شروع یک نخ جدید برای نوشتن به فایل از صف
            Task.Factory.StartNew(LogWriter, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public static void Log(Exception ex)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"DateTime: {DateTime.Now}");
            stringBuilder.AppendLine($"Exception Message: {ex.Message}");
            stringBuilder.AppendLine($"Stack Trace:\n{ex.StackTrace}");
            if (ex is System.Data.SqlClient.SqlException sqlException)
            {
                stringBuilder.AppendLine($"SQL Server Error Number: {sqlException.Number}");
            }
            if (ex.InnerException != null)
            {
                stringBuilder.AppendLine($"Inner Exception:");
                Log(ex.InnerException);
            }
            stringBuilder.AppendLine(new string('-', 50));

            // الگوی Producer-Consumer: اضافه کردن لاگ به صف
            _logQueue.Add(stringBuilder.ToString());
        }

        public static void Log(string text)
        {
            try
            {
                // الگوی Producer-Consumer: اضافه کردن لاگ به صف
                _logQueue.Add($"{text}\n{new string('-', 50)}");
            }
            catch { }
        }

        private static void LogWriter()
        {
            try
            {
                foreach (var logEntry in _logQueue.GetConsumingEnumerable())
                {
                    // الگوی Producer-Consumer: خواندن از صف و نوشتن به فایل
                    lock (_lockObject)
                    {
                        File.AppendAllText(_logFile, logEntry);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // اگر لغو شود، ممکن است به دلایل خاتمه نخ یا خاتمه برنامه باشد
            }
        }

        // تابعی برای لغو نخ نویسنده
        public static void StopLogWriter()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}

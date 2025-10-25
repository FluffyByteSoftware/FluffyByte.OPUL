using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;

/// <summary>
/// Centralized logging system for FluffyByte.OPUL server.
/// Provides color-coded console output, file logging, and performance measurement.
/// This is the ONLY class in the project that directly writes to files using built-in C# I/O.
/// </summary>
public static class Scribe
{
    // ============================================================
    // CONFIGURATION
    // ============================================================

    /// <summary>
    /// Provides configuration settings for logging and debugging features.
    /// </summary>
    /// <remarks>This class contains static properties to configure various aspects of logging and debugging,
    /// such as enabling debug logs, file logging, network logging, and console color coding. It also includes settings
    /// for maximum log file size, verbosity of stack traces, and inclusion of server context in log entries. These
    /// settings can be adjusted to suit different development and production environments.</remarks>
    public static class Config
    {
        public static bool EnableDebugLogs { get; set; } = false;
        public static bool EnableFileLogging { get; set; } = true;
        public static bool EnableNetworkLogging { get; set; } = false;
        public static bool ColorizeConsole { get; set; } = true;
        public static bool VerboseStackTraces { get; set; } = false;
        public static bool ShowServerContext { get; set; } = false;
        public static int MaxLogFileSizeMB { get; set; } = 50;
    }

    // ============================================================
    // PRIVATE FIELDS
    // ============================================================

    // NOTE: Using 'object' for lock in C# 9.0 / .NET 5
    // If you upgrade to .NET 9 (C# 13), you can use: private static readonly Lock _fileLock = new();
    private static readonly Lock _fileLock = new();
    private static readonly string _logDirectory = "Logs";
    private static readonly string _mainLogFileName = "serverLog.txt";
    private static readonly string _errorLogFileName = "serverLog_errors.txt";

    // Server context tracking (optional)
    private static long _currentTick = 0;
    private static int _currentPlayerCount = 0;
    private static int _maxPlayerCount = 8;

    // Exception aggregation tracking
    private static readonly Dictionary<string, ExceptionOccurrence> _exceptionTracker = [];

    // ============================================================
    // INITIALIZATION
    // ============================================================

    /// <summary>
    /// Initializes the Scribe logging system. Call this once at server startup.
    /// Creates log directory and writes startup banner.
    /// </summary>
    public static void Initialize()
    {
        // Create logs directory if it doesn't exist
        if (Config.EnableFileLogging && !Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }

        // Check if log rotation is needed
        if (Config.EnableFileLogging)
        {
            CheckAndRotateLogs();
        }

        // WriteTextAsync startup banner
        Banner();
        Info($"Scribe logging system initialized");
        Info($"Log directory: {Path.GetFullPath(_logDirectory)}");
    }

    /// <summary>
    /// Updates server context information for log entries.
    /// Call this periodically from your main game loop.
    /// </summary>
    public static void UpdateContext(long tick, int playerCount)
    {
        _currentTick = tick;
        _currentPlayerCount = playerCount;
    }

    /// <summary>
    /// Sets the maximum player count for context display.
    /// </summary>
    public static void SetMaxPlayers(int maxPlayers)
    {
        _maxPlayerCount = maxPlayers;
    }

    // ============================================================
    // PUBLIC LOGGING METHODS
    // ============================================================

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    public static void Info(
        string message,
        string? context = null,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        LogMessage(FluffyLogLevel.Info, message, context, null, file, line);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public static void Warning(
        string message,
        string? context = null,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        LogMessage(FluffyLogLevel.Warning, message, context, null, file, line);
    }

    /// <summary>
    /// Logs an error with exception details.
    /// </summary>
    public static void Error(
        string message,
        Exception ex,
        string? context = null,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        LogMessage(FluffyLogLevel.Error, message, context, ex, file, line);
    }

    /// <summary>
    /// Logs a critical/fatal error. Always writes to file even if file logging is disabled.
    /// </summary>
    public static void Critical(
        string message,
        Exception? ex = null,
        string? context = null,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        LogMessage(FluffyLogLevel.Critical, message, context, ex, file, line);
    }

    /// <summary>
    /// Logs a debug message. Only outputs if Config.EnableDebugLogs is true.
    /// </summary>
    public static void Debug(
        string message,
        string? context = null,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        if (!Config.EnableDebugLogs) return;
        LogMessage(FluffyLogLevel.Debug, message, context, null, file, line);
    }

    /// <summary>
    /// Logs network traffic. Only outputs if Config.EnableNetworkLogging is true.
    /// </summary>
    public static void Network(
        string message,
        string? direction = null,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        if (!Config.EnableNetworkLogging) return;
        string contextTag = direction != null ? $"NET-{direction}" : "NET";
        LogMessage(FluffyLogLevel.Network, message, contextTag, null, file, line);
    }

    /// <summary>
    /// Logs a performance measurement. Useful for tracking tick execution times.
    /// </summary>
    public static void Performance(
        string operationName,
        long elapsedMs,
        long warnThresholdMs = 0,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        string message = $"{operationName} took {elapsedMs}ms";

        if (warnThresholdMs > 0 && elapsedMs > warnThresholdMs)
        {
            message += $" (exceeded {warnThresholdMs}ms threshold)";
            LogMessage(FluffyLogLevel.Performance, message, "PERF", null, file, line);
        }
        else if (Config.EnableDebugLogs)
        {
            LogMessage(FluffyLogLevel.Performance, message, "PERF", null, file, line);
        }
    }

    // ============================================================
    // DISPLAY METHODS
    // ============================================================

    /// <summary>
    /// Displays a startup banner in the console.
    /// </summary>
    public static void Banner()
    {
        string banner = @"
╔════════════════════════════════════════════════════════════╗
║                                                            ║
║          FluffyByte.OPUL Game Server Engine                ║
║                    Version 0.1.0                           ║
║                                                            ║
║              Initializing Server Systems...                ║
║                                                            ║
╚════════════════════════════════════════════════════════════╝
";

        if (Config.ColorizeConsole)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
        }

        Console.WriteLine(banner);
        Console.WriteLine($"Startup Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        Console.WriteLine();

        if (Config.ColorizeConsole)
        {
            Console.ResetColor();
        }

        // WriteTextAsync banner to file as well
        if (Config.EnableFileLogging)
        {
            WriteToFile(_mainLogFileName, banner + $"\nStartup Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\n\n");
        }
    }

    /// <summary>
    /// Displays a separator line for visual organization in logs.
    /// </summary>
    public static void Separator(string? title = null)
    {
        string line = title != null
            ? $"═══════════════ {title} ═══════════════"
            : "═══════════════════════════════════════════════════════════";

        if (Config.ColorizeConsole)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }

        Console.WriteLine(line);

        if (Config.ColorizeConsole)
        {
            Console.ResetColor();
        }

        if (Config.EnableFileLogging)
        {
            WriteToFile(_mainLogFileName, line + "\n");
        }
    }

    // ============================================================
    // PRIVATE CORE LOGIC
    // ============================================================

    private static void LogMessage(
        FluffyLogLevel level,
        string message,
        string? context,
        Exception? ex,
        string file,
        int line)
    {
        // Build timestamp
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

        // Build caller info
        string fileName = Path.GetFileName(file);
        string callerInfo = $"{fileName}:{line}";

        // Build context prefix
        string contextPrefix = "";
        if (Config.ShowServerContext)
        {
            contextPrefix = $"[ Tick: {_currentTick} | Players: {_currentPlayerCount}/{_maxPlayerCount} ] ";
        }

        // Build level tag
        string levelTag = GetLevelTag(level);
        if (context != null)
        {
            levelTag = $"{levelTag} | {context}";
        }

        // Build main log line
        string logLine = $"{contextPrefix}[ {timestamp} ] [ {levelTag} :: {callerInfo} ] - [ {message} ]";

        // Console output with color
        WriteToConsole(level, logLine);

        // File output (always write Critical, even if file logging is disabled)
        if (Config.EnableFileLogging || level == FluffyLogLevel.Critical)
        {
            WriteToFile(_mainLogFileName, logLine + "\n");
        }

        // Handle exception details
        if (ex != null)
        {
            string exceptionDetails = FormatException(ex);

            // Console output
            if (Config.ColorizeConsole)
            {
                Console.ForegroundColor = GetLevelColor(level);
            }
            Console.WriteLine(exceptionDetails);
            if (Config.ColorizeConsole)
            {
                Console.ResetColor();
            }

            // File output
            if (Config.EnableFileLogging || level == FluffyLogLevel.Critical)
            {
                WriteToFile(_mainLogFileName, exceptionDetails + "\n");

                // Also write errors to separate error log
                if (level == FluffyLogLevel.Error || level == FluffyLogLevel.Critical)
                {
                    WriteToFile(_errorLogFileName, logLine + "\n" + exceptionDetails + "\n\n");
                }
            }

            // Check for repeated exceptions
            CheckExceptionAggregation(ex);
        }
    }

    private static string FormatException(Exception ex)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"   Exception Type: {ex.GetType().Name}");
        sb.AppendLine($"   Exception Message: {ex.Message}");

        // Stack trace
        if (Config.VerboseStackTraces)
        {
            sb.AppendLine($"   Stack Trace:");
            sb.AppendLine(ex.StackTrace);
        }
        else
        {
            // Filtered stack trace - only show relevant lines
            sb.AppendLine($"   Stack Trace (filtered):");
            string[] lines = ex.StackTrace?.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries) ?? [];
            foreach (var line in lines)
            {
                // Skip framework internals
                if (line.Contains("System.Threading") ||
                    line.Contains("System.Runtime") ||
                    line.Contains("System.Collections"))
                    continue;

                sb.AppendLine($"      {line.Trim()}");
            }
        }

        // Inner exception
        if (ex.InnerException != null)
        {
            sb.AppendLine($"   Inner Exception Type: {ex.InnerException.GetType().Name}");
            sb.AppendLine($"   Inner Exception Message: {ex.InnerException.Message}");

            if (Config.VerboseStackTraces)
            {
                sb.AppendLine($"   Inner Exception Stack Trace:");
                sb.AppendLine(ex.InnerException.StackTrace);
            }
        }

        return sb.ToString();
    }

    private static void WriteToConsole(FluffyLogLevel level, string message)
    {
        if (Config.ColorizeConsole)
        {
            // Critical gets special background treatment
            if (level == FluffyLogLevel.Critical)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = GetLevelColor(level);
            }
        }

        Console.WriteLine(message);

        if (Config.ColorizeConsole)
        {
            Console.ResetColor();
        }
    }

    private static void WriteToFile(string fileName, string content)
    {
        try
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
            
            string filePath = Path.Combine(_logDirectory, fileName);

            // Thread-safe file writing
            lock (_fileLock)
            {
                File.AppendAllText(filePath, content);
            }

            // Check if rotation is needed after writing
            CheckAndRotateLogs();
        }
        catch (Exception ex)
        {
            // If we can't write to file, at least show it in console
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[SCRIBE] Failed to write to log file: {ex.Message}");
            Console.ResetColor();
        }
    }

    private static void CheckAndRotateLogs()
    {
        try
        {
            string mainLogPath = Path.Combine(_logDirectory, _mainLogFileName);

            if (!File.Exists(mainLogPath))
                return;

            var fileInfo = new FileInfo(mainLogPath);
            long fileSizeInMB = fileInfo.Length / (1024 * 1024);

            if (fileSizeInMB >= Config.MaxLogFileSizeMB)
            {
                // Rotate the log file
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string rotatedFileName = $"serverLog_{timestamp}.txt";
                string rotatedPath = Path.Combine(_logDirectory, rotatedFileName);

                lock (_fileLock)
                {
                    File.Move(mainLogPath, rotatedPath);
                }

                Info($"Log file rotated: {rotatedFileName} ({fileSizeInMB}MB)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SCRIBE] Failed to rotate logs: {ex.Message}");
        }
    }

    private static void CheckExceptionAggregation(Exception ex)
    {
        string exceptionKey = $"{ex.GetType().Name}:{ex.Message}";

        lock (_exceptionTracker)
        {
            if (_exceptionTracker.TryGetValue(exceptionKey, out var occurrence))
            {
                occurrence.Count++;
                occurrence.LastOccurrence = DateTime.Now;

                // Log aggregation warning every 10 occurrences
                if (occurrence.Count % 10 == 0)
                {
                    Warning($"Exception '{ex.GetType().Name}' has occurred {occurrence.Count} times (last: {occurrence.LastOccurrence:HH:mm:ss})");
                }
            }
            else
            {
                _exceptionTracker[exceptionKey] = new ExceptionOccurrence
                {
                    Count = 1,
                    FirstOccurrence = DateTime.Now,
                    LastOccurrence = DateTime.Now
                };
            }
        }
    }

    // ============================================================
    // HELPER METHODS
    // ============================================================

    private static string GetLevelTag(FluffyLogLevel level)
    {
        return level switch
        {
            FluffyLogLevel.Debug => "DEBUG",
            FluffyLogLevel.Info => "INFO",
            FluffyLogLevel.Warning => "WARN",
            FluffyLogLevel.Error => "ERROR",
            FluffyLogLevel.Critical => "CRITICAL",
            FluffyLogLevel.Network => "NET",
            FluffyLogLevel.Performance => "PERF",
            _ => "LOG"
        };
    }

    private static ConsoleColor GetLevelColor(FluffyLogLevel level)
    {
        return level switch
        {
            FluffyLogLevel.Debug => ConsoleColor.Gray,
            FluffyLogLevel.Info => ConsoleColor.White,
            FluffyLogLevel.Warning => ConsoleColor.Yellow,
            FluffyLogLevel.Error => ConsoleColor.Red,
            FluffyLogLevel.Critical => ConsoleColor.White, // Background is red
            FluffyLogLevel.Network => ConsoleColor.Cyan,
            FluffyLogLevel.Performance => ConsoleColor.Magenta,
            _ => ConsoleColor.White
        };
    }

    // ============================================================
    // NESTED TYPES
    // ============================================================

    private class ExceptionOccurrence
    {
        public int Count { get; set; }
        public DateTime FirstOccurrence { get; set; }
        public DateTime LastOccurrence { get; set; }
    }
}
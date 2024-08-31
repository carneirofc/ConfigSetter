// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace ConfigSetter.Logging;

internal class SimpleConsoleLogger : ILogger
{
    private readonly object _gate = new object();

    private readonly LogLevel _minimalLogLevel;
    private readonly LogLevel _minimalErrorLevel;

    private static ImmutableDictionary<LogLevel, ConsoleColor> LogLevelColorMap => new Dictionary<LogLevel, ConsoleColor>
    {
        [LogLevel.Critical] = ConsoleColor.Red,
        [LogLevel.Error] = ConsoleColor.Red,
        [LogLevel.Warning] = ConsoleColor.Yellow,
        [LogLevel.Information] = ConsoleColor.White,
        [LogLevel.Debug] = ConsoleColor.DarkGray,
        [LogLevel.Trace] = ConsoleColor.DarkGray,
        [LogLevel.None] = ConsoleColor.White,
    }.ToImmutableDictionary();

    public SimpleConsoleLogger(LogLevel minimalLogLevel, LogLevel minimalErrorLevel)
    {
        _minimalLogLevel = minimalLogLevel;
        _minimalErrorLevel = minimalErrorLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        lock (_gate)
        {
            var message = formatter(state, exception);
            var logToErrorStream = logLevel >= _minimalErrorLevel;
            if (Console.IsOutputRedirected)
            {
                LogToConsole(message, logToErrorStream);
            }
            else
            {
                LogToTerminal(message, logLevel, logToErrorStream);
            }
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return (int)logLevel >= (int)_minimalLogLevel;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return NullScope.Instance;
    }

    private void LogToTerminal(string message, LogLevel logLevel, bool logToErrorStream)
    {
        var messageColor = LogLevelColorMap[logLevel];
        Console.ForegroundColor = messageColor;
        LogToConsole(message, logToErrorStream);
        Console.ResetColor();
    }

    private static void LogToConsole(string message, bool logToErrorStream)
    {
        if (logToErrorStream)
        {
            Console.Error.Write($"{message}{Environment.NewLine}");
        }
        else
        {
            Console.Out.Write($"  {message}{Environment.NewLine}");
        }
    }
}
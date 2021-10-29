using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace NorthwindCookieAuth.Extensions
{
    public class FileLogProvider : ILoggerProvider
    {
        private readonly Func<string,LogLevel,bool> _filter;

        public FileLogProvider(Func<string, LogLevel, bool> filter)
        {
            _filter = filter;
        }

        // we can have different signature constructor or using diffetent extension method signature with the same name
        // public FileLogProvider(LogLevel minimumLogLevel):this((category,logLevel)=>logLevel>=minimumLogLevel) {}
        // 
        // public FileLogProvider(LogLevel minimumLogLevel) {
        //     _filter=(categoryName,logLevel)=>logLevel>=minimumLogLevel;
        // }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName,_filter);
        }

        public void Dispose()
        {
            
        }
    }

    public class FileLogger : ILogger
    {
        private readonly Func<string,LogLevel,bool> _filter;
        private readonly string _categoryName;

        public FileLogger(string categoryName,Func<string, LogLevel, bool> filter)
        {
            _filter = filter;
            _categoryName=categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
            // we can make an empty dispoable and return it too. this empty has an empty dispose method and inherit IDisposable
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _filter(_categoryName,logLevel); // we use filter function to enable or disable Logging
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // This condition should check here always
            if (IsEnabled(logLevel)) {
                var msg=formatter(state,exception)+"\n";
                var today=DateTime.UtcNow.ToString("yyyy-MM-dd");
                var fileName=$"{_categoryName}-{today}.log";
                var path=Path.Combine(Environment.CurrentDirectory,fileName);
                File.AppendAllText(path,msg);
            }
        }
    }

    public static class LoggerFactoryExtension {

        // Using Logger Factory
        public static ILoggerFactory AddFileLogger(this ILoggerFactory loggerFactory,Func<string,LogLevel,bool> filter) {
            loggerFactory.AddProvider(new FileLogProvider(filter)); // We add provider using log provider and use it using createlogger method which generate a logger
            return loggerFactory;
        }

        // Using Logger Builder (used at Program.cs)
        public static ILoggingBuilder AddFileLogger(this ILoggingBuilder loggingBuilder,Func<string,LogLevel,bool> filter) {
            return loggingBuilder.AddProvider(new FileLogProvider(filter));
        }

        // Define another overload for it which internaly use the above extension methods
        public static ILoggerFactory AddFileLogger(this ILoggerFactory loggerFactory,LogLevel minimumLogeLevel) {
            return AddFileLogger(loggerFactory,(categoryName,loglevel)=>loglevel>=minimumLogeLevel);
        }

        public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder,LogLevel minimumLogeLevel) {
            return AddFileLogger(builder,(categoryName,loglevel)=>loglevel>=minimumLogeLevel);
        }
    }
}
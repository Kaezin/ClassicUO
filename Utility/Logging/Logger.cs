﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClassicUO.Utility.Logging
{
    public class Logger
    {
        public static readonly Dictionary<LogTypes, Tuple<ConsoleColor, string>> LogTypeInfo = new Dictionary<LogTypes, Tuple<ConsoleColor, string>> { { LogTypes.None, Tuple.Create(ConsoleColor.White, "") }, { LogTypes.Info, Tuple.Create(ConsoleColor.Green, "  Info    ") }, { LogTypes.Debug, Tuple.Create(ConsoleColor.DarkGreen, "  Debug   ") }, { LogTypes.Trace, Tuple.Create(ConsoleColor.Green, "  Trace   ") }, { LogTypes.Warning, Tuple.Create(ConsoleColor.Yellow, "  Warning ") }, { LogTypes.Error, Tuple.Create(ConsoleColor.Red, "  Error   ") }, { LogTypes.Panic, Tuple.Create(ConsoleColor.Red, "  Panic   ") } };

        private readonly BlockingCollection<Tuple<LogTypes, string, string>> _logQueue = new BlockingCollection<Tuple<LogTypes, string, string>>();
        private bool _isLogging;

        public Logger()
        {
        }

        // No volatile support for properties, let's use a private backing field.
        public LogTypes LogTypes { get; set; }



        public void Start(LogFile logFile = null)
        {
            var logThread = new Thread(async () =>
            {
                using (_logQueue)
                using (logFile)
                {
                    _isLogging = true;

                    while (_isLogging)
                    {
                        Thread.Sleep(1);

                        // Do nothing if logging is turned off (LogTypes.None) & the log queue is empty, but continue the loop.
                        if (LogTypes == LogTypes.None || !_logQueue.TryTake(out Tuple<LogTypes, string, string> log))
                            continue;

                        if (log.Item1 == LogTypes.Table)
                        {
                            Console.WriteLine(string.Format(log.Item3));
                            continue;
                        }

                        // LogTypes.None is also used for empty/simple log lines (without timestamp, etc.).
                        if (log.Item1 != LogTypes.None)
                        {

                            Console.ForegroundColor = ConsoleColor.White;

                            Console.Write($"{log.Item2} |");

                            Console.ForegroundColor = LogTypeInfo[log.Item1].Item1;
                            Console.Write(LogTypeInfo[log.Item1].Item2);
                            Console.ForegroundColor = ConsoleColor.White;

                            Console.WriteLine($"| {log.Item3}");
                            

                            if (logFile != null)
                                await logFile.WriteAsync($"{log.Item2} |{LogTypeInfo[log.Item1].Item2}| {log.Item3}");
                        }
                        else
                        {
                            Console.WriteLine(log.Item3);

                            if (logFile != null)
                                await logFile.WriteAsync(log.Item3);
                        }
                    }
                }
            })
            { IsBackground = true };
            logThread.Start();

            _isLogging = logThread.ThreadState == ThreadState.Running || logThread.ThreadState == ThreadState.Background;
        }

        public void Stop()
        {
            _isLogging = false;
        }

        public void Message(LogTypes logType, string text)
        {
            SetLogger(logType, text);
        }

        public void NewLine()
        {
            SetLogger(LogTypes.None, "");
        }

        public void WaitForKey()
        {
            Console.ReadKey(true);
        }

        public void Clear()
        {
            Console.Clear();
        }

        private void SetLogger(LogTypes type, string text)
        {
            if ((LogTypes & type) == type)
            {
                _logQueue.Add(type == LogTypes.None
                    ? Tuple.Create(type, "", text)
                    : Tuple.Create(type, DateTime.Now.ToString("T"), text));
            }
        }
    }
}

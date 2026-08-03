using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using SurfaceTypeCoverManager.Core.Enums;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.Services.Services
{
    public class EventLogService : IEventLogService
    {
        private readonly List<LogEventEntry> _recentLogs = new List<LogEventEntry>();
        private EventLogWatcher? _watcher;

        public event EventHandler<LogEventEntry>? EventLogged;

        public IReadOnlyList<LogEventEntry> RecentLogs
        {
            get
            {
                lock (_recentLogs)
                {
                    return _recentLogs.ToList();
                }
            }
        }

        public void AddLog(string source, string message, DiagnosticLevel level = DiagnosticLevel.Info)
        {
            var entry = new LogEventEntry
            {
                Timestamp = DateTime.Now,
                Source = source,
                Message = message,
                Level = level
            };

            lock (_recentLogs)
            {
                _recentLogs.Add(entry);
                if (_recentLogs.Count > 500) _recentLogs.RemoveAt(0);
            }

            EventLogged?.Invoke(this, entry);
        }

        public Task<IReadOnlyList<LogEventEntry>> FetchSystemPnpEventsAsync(int maxEvents = 50)
        {
            return Task.Run(() =>
            {
                var list = new List<LogEventEntry>();
                try
                {
                    string query = "*[System[(Provider[@Name='Microsoft-Windows-Kernel-PnP'] or Provider[@Name='DeviceSetupManager']) and TimeCreated[timediff(@SystemTime) <= 86400000]]]";
                    var elq = new EventLogQuery("System", PathType.LogName, query) { ReverseDirection = true };

                    using var reader = new EventLogReader(elq);
                    EventRecord record;
                    int count = 0;

                    while ((record = reader.ReadEvent()) != null && count < maxEvents)
                    {
                        using (record)
                        {
                            list.Add(new LogEventEntry
                            {
                                Timestamp = record.TimeCreated ?? DateTime.Now,
                                Source = record.ProviderName,
                                Message = record.FormatDescription() ?? record.TaskDisplayName ?? "PnP Event Record",
                                Level = record.Level == 2 ? DiagnosticLevel.Error : (record.Level == 3 ? DiagnosticLevel.Warning : DiagnosticLevel.Info)
                            });
                            count++;
                        }
                    }
                }
                catch
                {
                    // If EventLog query fails due to security permissions, gracefully add fallback entry
                    list.Add(new LogEventEntry
                    {
                        Timestamp = DateTime.Now,
                        Source = "EventLogService",
                        Message = "EventLog Access Restricted or Event Log Empty",
                        Level = DiagnosticLevel.Warning
                    });
                }

                return (IReadOnlyList<LogEventEntry>)list;
            });
        }

        public void StartListening()
        {
            try
            {
                StopListening();
                string query = "*[System[Provider[@Name='Microsoft-Windows-Kernel-PnP']]]";
                var elq = new EventLogQuery("System", PathType.LogName, query);
                _watcher = new EventLogWatcher(elq);
                _watcher.EventRecordWritten += (s, e) =>
                {
                    if (e.EventRecord != null)
                    {
                        using var record = e.EventRecord;
                        AddLog(record.ProviderName, record.FormatDescription() ?? "PnP Event", DiagnosticLevel.Info);
                    }
                };
                _watcher.Enabled = true;
            }
            catch
            {
                // Graceful fallback
            }
        }

        public void StopListening()
        {
            if (_watcher != null)
            {
                _watcher.Enabled = false;
                _watcher.Dispose();
                _watcher = null;
            }
        }
    }
}

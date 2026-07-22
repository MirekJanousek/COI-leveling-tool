using System;
using System.Collections.Generic;

namespace COILevelingTool.Diagnostics;

public sealed class LevelingDiagnostics
{
    private readonly Action<string> m_log;
    private readonly Action<string>? m_notify;
    private readonly HashSet<LevelingFailureCode> m_notified = new();

    public LevelingDiagnostics(Action<string> log, Action<string>? notify = null)
    {
        m_log = log ?? throw new ArgumentNullException(nameof(log));
        m_notify = notify;
    }

    public void Report(LevelingDiagnosticContext context, bool notifyOnce = false)
    {
        m_log(Format(context));
        if (notifyOnce && m_notify != null && m_notified.Add(context.Code))
            m_notify($"COI Leveling Tool disabled: {context.Code}. Check the game log for details.");
    }

    public static string Format(LevelingDiagnosticContext value) =>
        $"code={value.Code}; game={value.GameVersion}; core={value.CoreVersion}; " +
        $"prototype={value.PrototypeId ?? "n/a"}; target={value.Target?.ToString() ?? "n/a"}; " +
        $"parent={value.ParentOrigin?.ToString() ?? "n/a"}; elevation={value.Elevation?.RawValue.ToString() ?? "n/a"}; " +
        $"cost={value.CostMode ?? "n/a"}; rollback={value.RollbackStatus ?? "n/a"}; " +
        $"cleanup={value.CleanupStatus ?? "n/a"}; detail={value.Detail ?? "n/a"}";
}

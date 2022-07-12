namespace wtwd;

internal class PcSession : IComparable<PcSession>, IEquatable<PcSession>
{
    PcStateChangeHow SessionStartHow { get; }
    DateTime SessionStartWhen { get; }
    PcStateChangeHow? SessionTrueStartHow { get; set;  }
    DateTime? SessionTrueStartWhen { get; set;  }
    PcStateChangeHow? SessionEndHow { get; set; }
    DateTime? SessionEndWhen { get; set; }
    PcStateChangeHow? SessionIdleEndHow { get; set; }
    DateTime? SessionIdleEndWhen { get; set; }

    TimeSpan IdleStartSpan { get => SessionTrueStartWhen?.Subtract(SessionStartWhen) ?? TimeSpan.Zero; }
    TimeSpan? SessionSpan { get => (SessionEndWhen ?? SessionIdleEndWhen)?.Subtract(SessionTrueStartWhen ?? SessionStartWhen); }
    TimeSpan? IdleEndSpan { get => SessionIdleEndWhen?.Subtract(SessionEndWhen ?? DateTime.Now); }

    internal PcSession(PcStateChangeHow sessionStartHow, DateTime sessionStartWhen)
    {
        SessionStartHow = sessionStartHow;
        SessionStartWhen = sessionStartWhen;
    }

    public int CompareTo(PcSession? other)
    {
        if (other == null) return 1;

        int result = 0;

        if (result == 0)
            result = 2 * SessionStartHow.CompareTo(other.SessionStartHow);

        if (result == 0)
            result = 3 * SessionStartWhen.CompareTo(other.SessionStartWhen);

        if (result == 0)
            result = 4 * (SessionTrueStartHow?.CompareTo(other.SessionTrueStartHow) ?? 1);

        if (result == 0)
            result = 5 * (SessionTrueStartWhen?.CompareTo(other.SessionTrueStartWhen) ?? 1);

        if (result == 0)
            result = 6 * (SessionEndHow?.CompareTo(other.SessionEndHow) ?? 1);

        if (result == 0)
            result = 7 * (SessionEndWhen?.CompareTo(other.SessionEndWhen) ?? 1);

        if (result == 0)
            result = 8 * (SessionIdleEndHow?.CompareTo(other.SessionIdleEndHow) ?? 1);

        if (result == 0)
            result = 9 * (SessionIdleEndWhen?.CompareTo(other.SessionIdleEndWhen) ?? 1);

        return result;
    }

    public bool Equals(PcSession? other)
    {
        return this.CompareTo(other) == 0;
    }
}

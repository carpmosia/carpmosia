using System.Linq;
using Content.Shared.GameTicking;
using Robust.Server.DataMetrics;
using System.Diagnostics.Metrics;
using System.Diagnostics;

namespace Content.Server.GameTicking;

public sealed partial class GameTicker
{
    [Dependency] private IMetricsManager _metrics = default!;
    [Dependency] private IMeterFactory _meterFactory = default!;

    private Dictionary<PlayerGameStatus, int>? _playerStatusCounts;

    [Conditional("RELEASE")]
    private void InitializeMetrics()
    {
        _metrics.UpdateMetrics += MetricsOnUpdateMetrics;

        var meter = _meterFactory.Create("SS14.GameTicker");

        meter.CreateObservableGauge(
            "player_status_count",
            MeasureAdminCount,
            null,
            "The status of online players");
    }

    private void MetricsOnUpdateMetrics()
    {
        _sawmill.Verbose("Updating metrics");

        var dict = new Dictionary<PlayerGameStatus, int>();

        foreach (var status in Enum.GetValues<PlayerGameStatus>())
        {
            dict.Add(status, _playerGameStatuses.Values.Count(x => x == status));
        }

        _playerStatusCounts = dict;
    }

    private IEnumerable<Measurement<int>> MeasureAdminCount()
    {
        if (_playerStatusCounts == null)
            yield break;

        foreach (var (status, count) in _playerStatusCounts)
        {
            yield return new Measurement<int>(
                count,
                new KeyValuePair<string, object?>("status", status.ToString()));
        }
    }
}


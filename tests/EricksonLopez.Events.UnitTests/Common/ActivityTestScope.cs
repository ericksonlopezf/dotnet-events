// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EricksonLopez.Events.UnitTests.Common;

/// <summary>
/// Helper fixture to scope, filter, and capture Activity instances produced by an ActivitySource during tests.
/// </summary>
public sealed class ActivityTestScope : IDisposable
{
    private readonly ActivityListener _listener;
    private readonly List<Activity> _stoppedActivities = new();
    private readonly List<Activity> _startedActivities = new();
    private readonly object _lock = new();

    /// <summary>
    /// Gets all stopped activities recorded within this scope.
    /// </summary>
    public IReadOnlyList<Activity> StoppedActivities
    {
        get { lock (_lock) { return _stoppedActivities.ToList(); } }
    }

    /// <summary>
    /// Gets all started activities recorded within this scope.
    /// </summary>
    public IReadOnlyList<Activity> StartedActivities
    {
        get { lock (_lock) { return _startedActivities.ToList(); } }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ActivityTestScope"/> for the specified source name.
    /// </summary>
    /// <param name="sourceName">The name of the ActivitySource to listen to.</param>
    /// <param name="activityPredicate">Optional predicate to filter activities.</param>
    public ActivityTestScope(string sourceName, Func<Activity, bool>? activityPredicate = null)
    {
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == sourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = act =>
            {
                if (activityPredicate == null || activityPredicate(act))
                {
                    lock (_lock) { _startedActivities.Add(act); }
                }
            },
            ActivityStopped = act =>
            {
                if (activityPredicate == null || activityPredicate(act))
                {
                    lock (_lock) { _stoppedActivities.Add(act); }
                }
            }
        };
        ActivitySource.AddActivityListener(_listener);
    }

    /// <summary>
    /// Gets activities matching the specified operation name.
    /// </summary>
    public IReadOnlyList<Activity> GetActivitiesForOperation(string operationName)
    {
        lock (_lock)
        {
            return _stoppedActivities.Where(a => a.OperationName == operationName).ToList();
        }
    }

    /// <summary>
    /// Clears all recorded activities in this scope.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _startedActivities.Clear();
            _stoppedActivities.Clear();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _listener.Dispose();
    }
}

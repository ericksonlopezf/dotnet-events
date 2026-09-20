// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

namespace EricksonLopez.Events.UnitTests.Common;

/// <summary>
/// Helper fixture to scope, filter, and capture metrics measurements emitted by a Meter during tests.
/// </summary>
public sealed class MeterTestScope : IDisposable
{
    private readonly MeterListener _listener;
    private readonly string _sourceName;
    private readonly List<Instrument> _publishedInstruments = new();
    private readonly List<(string InstrumentName, long Value, Dictionary<string, object?> Tags)> _longMeasurements = new();
    private readonly List<(string InstrumentName, double Value, Dictionary<string, object?> Tags)> _doubleMeasurements = new();
    private readonly object _lock = new();

    /// <summary>
    /// Gets all instruments published by the meter within this scope.
    /// </summary>
    public IReadOnlyList<Instrument> PublishedInstruments
    {
        get { lock (_lock) { return _publishedInstruments.ToList(); } }
    }

    /// <summary>
    /// Gets all long (integer) measurements captured within this scope.
    /// </summary>
    public IReadOnlyList<(string InstrumentName, long Value, Dictionary<string, object?> Tags)> LongMeasurements
    {
        get { lock (_lock) { return _longMeasurements.ToList(); } }
    }

    /// <summary>
    /// Gets all double (floating point) measurements captured within this scope.
    /// </summary>
    public IReadOnlyList<(string InstrumentName, double Value, Dictionary<string, object?> Tags)> DoubleMeasurements
    {
        get { lock (_lock) { return _doubleMeasurements.ToList(); } }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="MeterTestScope"/> for the specified meter name.
    /// </summary>
    /// <param name="sourceName">The name of the Meter to listen to.</param>
    public MeterTestScope(string sourceName)
    {
        _sourceName = sourceName;
        _listener = new MeterListener();

        _listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == _sourceName)
            {
                lock (_lock)
                {
                    _publishedInstruments.Add(instrument);
                }
                listener.EnableMeasurementEvents(instrument);
            }
        };

        _listener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
        {
            var tagDict = tags.ToArray().ToDictionary(t => t.Key, t => t.Value);
            lock (_lock)
            {
                _longMeasurements.Add((instrument.Name, measurement, tagDict));
            }
        });

        _listener.SetMeasurementEventCallback<double>((instrument, measurement, tags, state) =>
        {
            var tagDict = tags.ToArray().ToDictionary(t => t.Key, t => t.Value);
            lock (_lock)
            {
                _doubleMeasurements.Add((instrument.Name, measurement, tagDict));
            }
        });

        _listener.Start();
    }

    /// <summary>
    /// Gets all long measurements for a specific instrument and event type tag.
    /// </summary>
    public IReadOnlyList<(string InstrumentName, long Value, Dictionary<string, object?> Tags)> GetLongMeasurementsForEvent(string instrumentName, string eventType)
    {
        lock (_lock)
        {
            return _longMeasurements
                .Where(m => m.InstrumentName == instrumentName &&
                            m.Tags.TryGetValue("event.type", out var typeVal) &&
                            Equals(typeVal, eventType))
                .ToList();
        }
    }

    /// <summary>
    /// Forces recording of observable instruments.
    /// </summary>
    public void RecordObservableInstruments() => _listener.RecordObservableInstruments();

    /// <summary>
    /// Clears all recorded measurements in this scope.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _longMeasurements.Clear();
            _doubleMeasurements.Clear();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _listener.Dispose();
    }
}

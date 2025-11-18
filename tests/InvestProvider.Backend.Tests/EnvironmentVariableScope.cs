using System;
using System.Collections.Generic;

namespace InvestProvider.Backend.Tests;

public sealed class EnvironmentVariableScope : IDisposable
{
    private readonly IReadOnlyList<string> _keys;
    private readonly string?[] _values;

    private EnvironmentVariableScope(IReadOnlyList<string> keys, string?[] values)
    {
        _keys = keys;
        _values = values;
    }

    public static EnvironmentVariableScope Set(string key, string? value)
    {
        var previousValue = Environment.GetEnvironmentVariable(key);
        Environment.SetEnvironmentVariable(key, value);
        return new EnvironmentVariableScope([key], [previousValue]);
    }

    public static EnvironmentVariableScope Set(IDictionary<string, string?> variables)
    {
        var keys = new List<string>(variables.Count);
        var values = new string?[variables.Count];
        var index = 0;

        foreach (var (key, value) in variables)
        {
            keys.Add(key);
            values[index] = Environment.GetEnvironmentVariable(key);
            Environment.SetEnvironmentVariable(key, value);
            index++;
        }

        return new EnvironmentVariableScope(keys, values);
    }

    public void Dispose()
    {
        for (var i = 0; i < _keys.Count; i++)
        {
            Environment.SetEnvironmentVariable(_keys[i], _values[i]);
        }
    }
}
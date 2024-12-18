﻿using ScriptProfiler.Lib.Automation.Output;
using ScriptProfiler.Lib.Automation.Script;
using ScriptProfiler.Lib.Utility;
using System.Text.Json.Serialization;

namespace ScriptProfiler.Lib.Automation.Job;

public class ScriptJobResult : IDisposable
{
    public int Id { get; private set; }
    public string JobName { get; private set; }
    public string ScriptName { get; private set; }
    public string DeviceName { get; private set; }
    public DateTime ExecutionTime { get; private set; }
    public ScriptOutputList ScriptOutput { get; private set; }
    public ScriptExecutionState ExecutionState { get; private set; }
    public event EventHandler<ScriptExecutionState>? ExecutionResultReceived;

    [JsonIgnore]
    public Task<ScriptExecutionState>? RunJobTask { get; private set; }

    private ScriptJobResult(int id, string jobName, string scriptName, string deviceName, DateTime executionTime, ScriptOutputList scriptOutput)
    {
        Id = id;
        JobName = jobName;
        // We want to store a copy of the current script name and device name since they might change in the future
        ScriptName = scriptName;
        DeviceName = deviceName;
        ExecutionTime = executionTime;
        ScriptOutput = scriptOutput;
    }

    public ScriptJobResult(int id, string jobName, string scriptName, string deviceName, DateTime executionTime, ScriptOutputList scriptOutput,
        Task<ScriptExecutionState> runJobTask) : this(id, jobName, scriptName, deviceName, executionTime, scriptOutput)
    {
        RunJobTask = runJobTask;
    }

    [JsonConstructor]
    public ScriptJobResult(int id, string jobName, string scriptName, string deviceName, DateTime executionTime, ScriptOutputList scriptOutput,
        ScriptExecutionState executionState) : this(id, jobName, scriptName, deviceName, executionTime, scriptOutput)
    {
        ExecutionState = executionState;
    }

    internal void InvokeExecutionResultReceived(ScriptExecutionState newState)
    {
        ExecutionState = newState;
        ExecutionResultReceived?.Invoke(this, newState);
    }

    public void Dispose()
    {
        ExecutionResultReceived.DisposeSubscriptions();
        ScriptOutput.Dispose();
        GC.SuppressFinalize(this);
    }
}

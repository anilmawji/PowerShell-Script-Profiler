﻿using ITPortal.Lib.Automation.Job;
using ITPortal.Lib.Automation.Output;

namespace ITPortal.Lib.Services;

public sealed class ScriptJobService : IScriptJobService
{
    public const int MaxResults = 50;

    public Dictionary<string, ScriptJob> Jobs { get; set; } = new();
    public List<ScriptJobResult> JobResults { get; set; } = new();

    private int _nextResultId = 0;

    public void AddJob(ScriptJob job)
    {
        ArgumentNullException.ThrowIfNull(job.Name, nameof(job.Name));

        Jobs.Add(job.Name, job);
    }

    public ScriptJobResult RunJob(ScriptJob job, string deviceName, ScriptOutputList scriptOutput)
    {
        ArgumentNullException.ThrowIfNull(job.Name, nameof(job.Name));
        ArgumentNullException.ThrowIfNull(job.Script.FileName, nameof(job.Script.FileName));

        ScriptJobResult result = new(
            _nextResultId++,
            job.Name,
            job.Script.FileName,
            deviceName,
            DateTime.Now,
            scriptOutput
        );

        JobResults.Add(result);
        // Cap the results list to store only the most recent results
        if (JobResults.Count > MaxResults)
        {
            JobResults.RemoveAt(0);
        }

        job.Run(deviceName, result, ScriptOutputList.FormatAsSystemMessage("Script execution was cancelled"))
            .ConfigureAwait(false);

        return result;
    }

    public ScriptJobResult GetJobResult(int jobResultId)
    {
        return JobResults.ElementAt(jobResultId);
    }

    public bool HasJob(string jobName)
    {
        return Jobs.GetValueOrDefault(jobName) != default(ScriptJob?);
    }
}

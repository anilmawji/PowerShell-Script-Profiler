﻿using ITPortal.Lib.Services.Automation.Output;

namespace ITPortal.Lib.Services.Automation.Job;

public class ScriptJobService : IScriptJobService
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

    public ScriptJobResult RunJob(ScriptJob job, IOutputStreamService outputStreamService, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(job.Script.FileName, nameof(job.Script.FileName));

        ScriptJobResult result = new(_nextResultId++, job.Script.FileName, job.Script.DeviceName, DateTime.Now, outputStreamService);

        job.Run(outputStreamService, result, cancellationToken)
            .ConfigureAwait(false);

        JobResults.Add(result);
        if (JobResults.Count > MaxResults)
        {
            JobResults.RemoveAt(JobResults.Count - 1);
        }
        return result;
    }

    public ScriptJobResult GetJobResult(int jobResultId)
    {
        return JobResults.ElementAt(jobResultId);
    }

    public ScriptJob? TryGetJob(string jobName)
    {
       return HasJob(jobName) ? Jobs[jobName] : null;
    }

    public bool HasJob(string jobName)
    {
        return Jobs.GetValueOrDefault(jobName) != default(ScriptJob);
    }
}

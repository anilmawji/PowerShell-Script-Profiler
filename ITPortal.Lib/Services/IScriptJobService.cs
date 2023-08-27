﻿using ITPortal.Lib.Automation.Job;
using ITPortal.Lib.Automation.Output;

namespace ITPortal.Lib.Services;

public interface IScriptJobService
{
    public Dictionary<string, ScriptJob> Jobs { get; set; }
    public List<ScriptJobResult> JobResults { get; set; }

    public void AddJob(ScriptJob job);
    public ScriptJobResult RunJob(ScriptJob job, ScriptOutputList scriptOutput);
    public ScriptJobResult GetJobResult(int jobResultId);
    public ScriptJob? TryGetJob(string jobName);
    public bool HasJob(string jobName);
}
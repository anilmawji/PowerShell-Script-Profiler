﻿namespace ScriptProfiler.Utility;

public static class PageRoute
{
    public const string Login = "/login";

    public const string CreateScriptJob = "/new-job";

    public const string EditScriptJobBase = "/edit-job";
    public const string EditScriptJob = "/edit-job/{JobName}";

    public const string ScriptJobs = "/jobs";

    public const string ScriptJobResults = "/job-results";
    public const string ScriptJobResultDetails = "/job-results/{Id:int}";

    public const string DeviceInventory = "/devices";

    public static string ScriptJobResultDetailsWithId(int jobId)
    {
        return $"{ScriptJobResults}/{jobId}";
    }

    public static string EditScriptJobWithName(string jobName)
    {
        return $"{EditScriptJobBase}/{jobName}";
    }
}

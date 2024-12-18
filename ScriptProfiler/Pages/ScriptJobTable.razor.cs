﻿using CommunityToolkit.Maui.Storage;
using ScriptProfiler.Components.Models.Dialog;
using ScriptProfiler.Components.Shared.Dialog;
using ScriptProfiler.Components.Utility;
using ScriptProfiler.Lib.Automation.Job;
using ScriptProfiler.Lib.Automation.Output;
using ScriptProfiler.Utility;
using MudBlazor;

namespace ScriptProfiler.Pages;

public sealed partial class ScriptJobTable
{
    private static readonly DialogOptions s_dialogOptions = new()
    {
        CloseButton = true,
        MaxWidth = MaxWidth.ExtraSmall
    };
    private static readonly DialogParameters<Dialog> s_deleteJobDialogParameters = [];
    private static readonly DialogParameters<RunScriptJobDialog> s_runJobDialogParameters = [];
    private static readonly DialogParameters<Dialog> s_cancelJobDialogParameters = [];
    private static readonly int[] s_pageSizeOptions = { 20, 50, 100 };
    private static readonly PickOptions s_filePickOptions = new()
    {
        // PickerTitle is used by macOS but not Windows - unreliable for providing information to user
        PickerTitle = "Please select a Job file",
        FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, [".json"] },
            { DevicePlatform.macOS, ["json"] }
        })
    };

    private string _searchString = "";
    private HashSet<ScriptJob> selectedJobs = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            LoadSavedFiles();
            InitializeDialogParameters();
            StateHasChanged();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private void LoadSavedFiles()
    {
        if (ScriptJobService.Jobs.Count == 0)
        {
            ScriptJobSerializer.LoadFromSaveDirectory(ScriptJobService.Jobs);
        }

        if (ScriptJobService.JobResults.Count == 0)
        {
            ScriptJobResultSerializer.LoadFromSaveDirectory(ScriptJobService.JobResults);
        }
    }

    private static void InitializeDialogParameters()
    {
        s_deleteJobDialogParameters.Add(dialog => dialog.ContentText, "Are you sure you want to delete this job? This action cannot be undone.");
        s_deleteJobDialogParameters.Add(dialog => dialog.SubmitButtonText, "Delete");
        s_deleteJobDialogParameters.Add(dialog => dialog.Color, MudBlazor.Color.Error);

        s_runJobDialogParameters.Add(dialog => dialog.SubmitButtonText, "Done");
        s_runJobDialogParameters.Add(dialog => dialog.Color, MudBlazor.Color.Secondary);

        s_cancelJobDialogParameters.Add(dialog => dialog.ContentText, "Are you sure you want to stop execution of this job?");
        s_cancelJobDialogParameters.Add(dialog => dialog.SubmitButtonText, "Yes");
        s_cancelJobDialogParameters.Add(dialog => dialog.CancelButtonText, "No");
        s_cancelJobDialogParameters.Add(dialog => dialog.Color, MudBlazor.Color.Secondary);
    }

    private void GoToNewJobPage()
    {
        NavigationManager.NavigateTo(PageRoute.CreateScriptJob);
    }

    private async Task DownloadSelectedJobsAsync()
    {
        FolderPickerResult folderResult = await FolderPicker.Default.PickAsync(CancellationToken.None);

        if (!folderResult.IsSuccessful) return;

        foreach (ScriptJob selectedJob in selectedJobs)
        {
            foreach (ScriptJob job in ScriptJobService.Jobs)
            {
                if (job.Name == selectedJob.Name)
                {
                    string jsonFilePath = Path.Combine(folderResult.Folder.Path, job.Name + ".json");

                    ScriptJobSerializer.TryCreateFile(selectedJob, jsonFilePath);
                    break;
                }
            }
        }
    }

    private async Task PickJobFileAsync()
    {
        FileResult? fileResult = await MauiFileHelper.PickFileAsync(s_filePickOptions);

        if (fileResult == null) return;

        ScriptJob? job = ScriptJobSerializer.LoadFromFile(fileResult.FullPath);

        if (job == null) return;

        if (ScriptJobService.Jobs.TryAdd(job))
        {
            StateHasChanged();
        }
    }

    private async Task OpenRunJobDialog(ScriptJob job)
    {
        IDialogReference dialog = DialogService.Show<RunScriptJobDialog>("Run Job", s_runJobDialogParameters, s_dialogOptions);
        DialogResult? dialogResult = await dialog.Result;

        if (dialogResult is null || dialogResult.Canceled) return;

        RunScriptJobDialogResult? dialogResultData = dialogResult.Data as RunScriptJobDialogResult;

        if (dialogResultData == null) return;

        ScriptJobResult jobResult = RunJob(
            job, job.Script.NewScriptOutputList(),
            dialogResultData.DeviceName,
            dialogResultData.ShouldRunJobNow,
            dialogResultData.ErrorAction
        );

        if (dialogResultData.ShouldViewJobResult)
        {
            NavigationManager.NavigateTo(PageRoute.ScriptJobResultDetailsWithId(jobResult.Id));
        }
    }

    private ScriptJobResult RunJob(ScriptJob job, ScriptOutputList outputList, string deviceName,
        bool runJobNow, ScriptJobErrorAction errorAction)
    {
        ScriptJobResult jobResult;

        if (errorAction == ScriptJobErrorAction.Cancel)
        {
            CancelJobOnErrorOutputReceived(job, outputList);
        }

        if (runJobNow)
        {
            jobResult = ScriptJobService.RunJob(job, deviceName, outputList);
        }
        else
        {
            //TODO: Schedule job to run at _runDate instead
            jobResult = ScriptJobService.RunJob(job, deviceName, outputList);
        }

        // This is executed when the job is finished running
        jobResult.RunJobTask?.ContinueWith((resultTask) =>
        {
            string filePath = ScriptJobResultSerializer.GetFilePath(jobResult.Id.ToString());
            ScriptJobResultSerializer.TryCreateFile(jobResult, filePath);

            InvokeAsync(StateHasChanged);
            outputList.Dispose();
        });

        return jobResult;
    }

    private void CancelJobOnErrorOutputReceived(ScriptJob job, ScriptOutputList outputList)
    {
        outputList.OutputChanged += (object? sender, ScriptOutputChangedEventArgs e) =>
        {
            if (e.StreamType == ScriptOutputStreamType.Error)
            {
                CancelJob(job);
            }
        };
    }

    private async Task OpenCancelJobDialog(ScriptJob job)
    {
        IDialogReference dialog = DialogService.Show<Dialog>("Confirm Job Cancellation", s_cancelJobDialogParameters, s_dialogOptions);
        DialogResult? dialogResult = await dialog.Result;

        if (dialogResult is not null && dialogResult.Canceled) return;

        CancelJob(job);
    }

    private void GoToEditJobPage(ScriptJob job)
    {
        // TODO: toast notification of job not found error
        if (!ScriptJobService.Jobs.Contains(job.Name)) return;

        NavigationManager.NavigateTo(PageRoute.EditScriptJobWithName(job.Name));
    }

    private async Task OpenDeleteJobDialog(string jobName)
    {
        IDialogReference dialog = DialogService.Show<DeleteScriptJobDialog>("Confirm Job Deletion", s_deleteJobDialogParameters, s_dialogOptions);
        DialogResult? dialogResult = await dialog.Result;

        if (dialogResult == null || dialogResult.Canceled) return;

        var dialogResultData = dialogResult.Data as DeleteScriptJobDialogResult;
        ScriptJob? job = ScriptJobService.Jobs.GetJob(jobName);

        if (dialogResultData != null && dialogResultData.ShouldDeleteJobResults && job != null)
        {
            DeleteJobResults(job);
        }
        DeleteJob(jobName);
    }

    private void CancelJob(ScriptJob job)
    {
        job.Cancel();

        InvokeAsync(StateHasChanged);
    }

    private void DeleteJob(string jobName)
    {
        ScriptJobService.Jobs.Remove(jobName);
        ScriptJobSerializer.TryDeleteFile(jobName);

        InvokeAsync(StateHasChanged);
    }

    private void DeleteJobResults(ScriptJob job)
    {
        List<ScriptJobResult> results = ScriptJobService.JobResults.Remove(job);

        foreach (ScriptJobResult result in results)
        {
            ScriptJobSerializer.TryDeleteFile(result.Id.ToString());
            result.Dispose();
        }
    }

    private bool FilterJobFunc(ScriptJob job) => DoFilterJobFunc(job, _searchString);

    private static bool DoFilterJobFunc(ScriptJob job, string searchString)
    {
        return string.IsNullOrWhiteSpace(searchString)
                    || job.Name.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    || (job.Script.FileName != null && job.Script.FileName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    || job.State.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    || job.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase);
    }
}

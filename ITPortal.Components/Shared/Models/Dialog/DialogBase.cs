﻿using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ITPortal.Components.Shared.Models.Dialog;

public class DialogBase : ComponentBase
{
    [CascadingParameter, EditorRequired]
    public MudDialogInstance? MudDialog { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? ContentText { get; set; }

    [Parameter]
    public string SubmitButtonText { get; set; } = "Submit";

    [Parameter]
    public string CancelButtonText { get; set; } = "Cancel";

    [Parameter]
    public Color Color { get; set; }

    internal virtual void Submit() => MudDialog?.Close(DialogResult.Ok(true));
    internal virtual void Cancel() => MudDialog?.Cancel();
}

﻿using ITPortal.Lib.Automation.Output;
using Microsoft.JSInterop;

namespace ITPortal.Components.Shared.Script;

public sealed partial class ScriptOutputView
{
    private IReadOnlyList<ScriptOutputMessage>? _messages;

    protected override void OnInitialized()
    {
        _messages = StreamType != null
            ? OutputList.GetMessagesFilteredByStream((ScriptOutputStreamType)StreamType)
            : OutputList.GetMessages();

        OutputList.OutputChanged += OnOutputChanged;
    }

    private void OnOutputChanged(object? sender, ScriptOutputChangedEventArgs args)
    {
        JSRuntime.InvokeVoidAsync("scrollToBottom", "output-container");
        InvokeAsync(this.StateHasChanged);
    }

    private static string GetMessageStyle(ScriptOutputMessage message)
    {
        return message != null && message.StreamType != ScriptOutputStreamType.Standard
                    ? $"color:{message.StreamType.GetColor()}"
                    : string.Empty;
    }

    public void Dispose()
    {
        OutputList.OutputChanged -= OnOutputChanged;
    }
}

﻿using ITPortal.Lib.Utilities;

namespace ITPortal.Lib.Automation.Script;

public enum ScriptLoadState
{
    Unloaded,
    Failed,
    Success
}

public static class ScriptLoadStateExtensions
{
    public static readonly Dictionary<ScriptLoadState, string> Colors = new()
    {
        { ScriptLoadState.Unloaded, StateColors.Grey  },
        { ScriptLoadState.Success,  StateColors.Green },
        { ScriptLoadState.Failed,   StateColors.Red   },
    };

    public static string GetColor(this ScriptLoadState state)
    {
        return Colors[state];
    }
}
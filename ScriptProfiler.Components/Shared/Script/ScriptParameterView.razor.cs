﻿using ScriptProfiler.Lib.Automation.Script;
using ScriptProfiler.Lib.Utility;

namespace ScriptProfiler.Components.Shared.Script;

public sealed partial class ScriptParameterView
{
    private static readonly string[] s_extendedInputFieldTriggers = { "body", "description", "message" };

    private const string Delimiter = " ";
    private const int ChipsMaxWidth = 80;

    private string ParameterName = string.Empty;
    private string RequiredError = string.Empty;

    protected override void OnInitialized()
    {
        ParameterName = ScriptParameter.Name.FirstCharToUpper();
        RequiredError = ParameterName + " is a required field";
    }

    // Provide a larger input field depending on the name of the parameter
    private int GetNumLinesInTextField()
    {
        return s_extendedInputFieldTriggers.Contains(ScriptParameter.Name) ? 10 : 1;
    }

    private IEnumerable<string> ValidateArray<T>(T newValue)
    {
        if (ScriptParameter.Value is not List<string> arrayValues)
        {
            yield return ParameterName + "must be a valid list";
            yield break;
        }
        if (ScriptParameter.IsMandatory && !ScriptParameter.AllowEmptyCollection && arrayValues.Count == 0)
        {
            yield return RequiredError;
        }
    }
}

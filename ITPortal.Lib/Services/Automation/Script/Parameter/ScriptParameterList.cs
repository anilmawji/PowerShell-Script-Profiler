﻿using System.Collections;

namespace ITPortal.Lib.Services.Automation.Script.Parameter;

// Extends from IEnumerable instead of List<PSParameter> to force caller to interact with the list through exposed methods only
public class ScriptParameterList : IEnumerable<ScriptParameter>
{
    public List<ScriptParameter> Parameters { get; set; } = new();

    public void Add(string parameterName, Type parameterType, bool mandatory = false)
    {
        Parameters.Add(new ScriptParameter(parameterName, parameterType, mandatory));
    }

    public IEnumerator<ScriptParameter> GetEnumerator()
    {
        return Parameters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Parameters.GetEnumerator();
    }
}

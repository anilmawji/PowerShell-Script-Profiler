﻿namespace ITPortal.Components.Models.Dialog
{
    public class RunScriptJobDialogResult
    {
        public bool ShouldRunJobImmediately { get; set; } = true;
        public bool ShouldViewJobResult { get; set; } = true;
        public DateTime RunDate { get; set; }
    }
}
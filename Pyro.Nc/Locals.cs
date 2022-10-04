namespace Pyro.Nc
{
    public static class Locals
    {
        public const string Trans = "Work Offset";
        public const string Lims = "Spindle Speed Limiter";
        public const string Comment = "Comment";
        public const string ToolSetter = "Sets the tool";
        public const string SpindleSpeedSetter = "Sets the tool's spindle speed";
        public const string FeedRateSetter = "Sets the tool's feed rate";
        public const string G00 = "Fast Move";
        public const string G01 = "Cut Move";
        public const string G02 = "Circular Move (Clockwise)";
        public const string G03 = "Circular Move (Counter-Clockwise)";
        public const string CIP = "Circular Interpolation (Arc)";
        public const string G04 = "Dwell (Pause)";
        public const string G05 = "High-precision contour control (HPCC)";
        public const string G09 = "Exact stop check (Non-Modal)";
        public const string G54 = "";
        public const string G60 = "Save Position";
        public const string G61 = "Exact stop check (Modal)";
        public const string G61_1="Return to saved position";
        public const string G64 = "Default cutting mode (cancel exact stop check mode)";
        public const string G70 = "Imperial units ( Inches - {2.54 x mm} )";
        public const string G71 = "Metric units ( MM )";
        public const string G80 = "Cancel canned cycle - G73, G81, G83 - Return to initial Z (Vector3.Z)";
        public const string G81 = "Drill";
        public const string G82 = "Drill with dwell at bottom";
        public const string G90 = "Absolute programming";
        public const string G91 = "Incremental programming";
        public const string G96 = "Activate spindle speed limit";
        public const string G97 = "Disable spindle speed limit";
        public const string G331 = "Drill";
        public const string G332 = "Retract (Drill)";
        public const string M00 = "Unconditional Stop (Pause)";
        public const string M01 = "Optional Stop (Pause)";
        public const string M03 = "Spindle On (Clockwise)";
        public const string M04 = "Spindle On (Counter-Clockwise)";
        public const string M05 = "Spindle Stop";
        public const string M06 = "Tool Change";
    }
}
namespace UnitedTools.SettingsCtrl
{
    public class SettingsCtrlNameAttribute : System.Attribute
    {
        public string Name { get; set; } = @"";
        public string GroupName { get; set; } = @"";
        public string SubGroupName { get; set; } = @"";
    }
}

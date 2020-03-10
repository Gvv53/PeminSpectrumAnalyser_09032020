using System;
using System.Reflection;
using System.Windows.Controls;

namespace UnitedTools.SettingsCtrl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
        }

        public void LinkToObject<T>(T obj)
        {

            PropertyInfo[] propertys;
            Type currentType = typeof(T);

            propertys = currentType.GetProperties();
            //object[] attrs;

            //foreach (PropertyInfo property in propertys)
            //{
            //    property.Name

            //    attrs = property.GetCustomAttributes(false);
            //    foreach (SettingsCtrlNameAttribute current in attrs)
            //    {

            //    }
            //}
        }
    }
}

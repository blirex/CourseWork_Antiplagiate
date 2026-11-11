
using System.Windows;

namespace CourseWork_Zubarev
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DatabaseService.Initialize();
        }
    }
}


using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace CourseWork_Zubarev
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                try
                {
                    File.WriteAllText(
                        "error_log.txt",
                        args.ExceptionObject.ToString()
                    );
                }
                catch { }
            };

            DispatcherUnhandledException += (s, args) =>
            {
                try
                {
                    File.WriteAllText(
                        "error_log.txt",
                        args.Exception.ToString()
                    );
                }
                catch { }

                MessageBox.Show(
                    args.Exception.ToString(),
                    "Ошибка приложения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                args.Handled = true;
            };

            try
            {
                DatabaseService.Initialize();
            }
            catch (Exception ex)
            {
                File.WriteAllText("error_log.txt", ex.ToString());

                MessageBox.Show(
                    ex.ToString(),
                    "Ошибка базы данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}

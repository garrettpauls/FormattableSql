using SimpleNotes.Services;

using System.Windows;

namespace SimpleNotes
{
    public partial class App : Application
    {
        public static ServiceManager Services { get; } = new ServiceManager();

        protected override void OnExit(ExitEventArgs e)
        {
            Services.Dispose();
            base.OnExit(e);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await Services.InitializeAsync();
            base.OnStartup(e);
        }
    }
}

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;

namespace ProgrammersToolKit
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();
            Application.Start((p) => {
                var context = new DispatcherQueueSynchronizationContext(
                    DispatcherQueue.GetForCurrentThread());
                System.Threading.SynchronizationContext.SetSynchronizationContext(context);
                new App();
            });
        }
    }
}

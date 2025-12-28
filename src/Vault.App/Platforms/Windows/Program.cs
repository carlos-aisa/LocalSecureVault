using Microsoft.UI.Xaml;
using System;

namespace Vault.App.WinUI
{
    public static class Program
    {
        [global::System.Runtime.InteropServices.DllImport("Microsoft.ui.xaml.dll")]
        private static extern void XamlCheckProcessRequirements();

        [global::System.STAThread]
        static void Main(string[] args)
        {
            XamlCheckProcessRequirements();

            global::WinRT.ComWrappersSupport.InitializeComWrappers();
            
            // For self-contained unpackaged apps, we do NOT call Bootstrap.Initialize
            // global::Microsoft.Windows.ApplicationModel.DynamicDependency.Bootstrap.Initialize(0x00010004);

            global::Microsoft.UI.Xaml.Application.Start((p) => {
                var context = new global::Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(global::Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
                global::System.Threading.SynchronizationContext.SetSynchronizationContext(context);
                new App();
            });
        }
    }
}

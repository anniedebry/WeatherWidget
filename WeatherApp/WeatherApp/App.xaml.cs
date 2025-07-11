using Microsoft.Maui;
using Microsoft.Maui.Controls;
//so it doesn't through errors if it isn't a windows build
#if WINDOWS
using Microsoft.UI.Windowing;
using Windows.Graphics;
using WinRT.Interop;
using System.Runtime.InteropServices;
#endif

namespace WeatherApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);
//so it doesn't through errors if it isn't a windows build
#if WINDOWS
        window.HandlerChanged += (s, e) =>
        {
            var mauiWindow = s as Window;
            var handler = mauiWindow?.Handler;
            var nativeWindow = handler?.PlatformView as Microsoft.UI.Xaml.Window;
            var hwnd = WindowNative.GetWindowHandle(nativeWindow);

            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            var size = new SizeInt32 { Width = 500, Height = 420 };
            appWindow.Resize(size);

            //for the widget making it stay a specific size and making it appear on the top layer always
            appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
            //appWindow.SetPresenter(AppWindowPresenterKind.CompactOverlay); //this one is for widgets on top but its only one size

            var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
            int posX = displayArea.WorkArea.X + displayArea.WorkArea.Width - size.Width;
            int posY = displayArea.WorkArea.Y + displayArea.WorkArea.Height - size.Height;

            appWindow.Move(new Windows.Graphics.PointInt32(posX, posY));
        };
#endif


            return window;
        }
    }
}

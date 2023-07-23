using System;
using System.Runtime.InteropServices;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace DontQuitMeMod
{
    internal sealed class ModEntry : Mod
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public delegate bool EnumWindowsCallback(IntPtr hwnd, int lParam);
        [DllImport("user32.dll")]
        private static extern int EnumWindows(EnumWindowsCallback callPtr, int lParam);

        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        internal const UInt32 SC_CLOSE = 0xF060;
        internal const UInt32 MF_GRAYED = 0x00000001;
        internal const UInt32 MF_ENABLED = 0x00000000;

        private IModHelper _helper;
        private bool _buttonEnabled = true;
        private bool WindowCloseButtonHandler(IntPtr hwnd, int lParam)
        {
            uint tid = GetWindowThreadProcessId(hwnd, out uint procId);

            if (procId == Environment.ProcessId)
            {
                IntPtr hSystemMenu = GetSystemMenu(hwnd, false);

                _helper.Events.GameLoop.GameLaunched += delegate
                {
                    EnableMenuItem(hSystemMenu, SC_CLOSE, (uint)(MF_ENABLED | MF_GRAYED));
                    _buttonEnabled = false;
                };

                _helper.Events.Input.ButtonPressed += delegate(object sender, ButtonPressedEventArgs e)
                {
                    if(e.Button == SButton.F10)
                    {
                        if(_buttonEnabled)
                            EnableMenuItem(hSystemMenu, SC_CLOSE, (uint)(MF_ENABLED | MF_GRAYED));
                        else
                            EnableMenuItem(hSystemMenu, SC_CLOSE, (uint)(MF_ENABLED | MF_ENABLED));
                        _buttonEnabled = !_buttonEnabled;
                    }
                };
            }

            return true;
        }

        public override void Entry(IModHelper helper)
        {
            _helper = helper;
            EnumWindowsCallback callBackFn = new EnumWindowsCallback(WindowCloseButtonHandler);
            EnumWindows(callBackFn, 0);
        }
    }
}

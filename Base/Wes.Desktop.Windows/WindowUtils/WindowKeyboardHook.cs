using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using Wes.Utilities;
using static Wes.Utilities.WindowsAPI;

namespace Wes.Desktop.Windows
{
    /// <summary>
    /// 鍵盤hook
    /// </summary>
    public class WindowKeyboardHook
    {
        private static int _hHook;
        private static WesHookProc _wesHookProcDelegate;

        public static event EventHandler<KeyPressEventArgs> OnKeyDown;

        private static void OnKeyDownEventHandler(KeyPressEventArgs args)
        {
            OnKeyDown?.Invoke(null, args);
        }

        public static event EventHandler<KeyPressEventArgs> OnKeyUp;

        private static void OnKeyUpEventHandler(KeyPressEventArgs args)
        {
            OnKeyUp?.Invoke(null, args);
        }

        public static void AddHook()
        {
            _wesHookProcDelegate = new WesHookProc(WindowKeyboardHookHandler);
            ProcessModule processModule = Process.GetCurrentProcess().MainModule;
            var mhandler = WindowsAPI.GetModuleHandle(processModule.ModuleName);
            _hHook = WindowsAPI.SetWindowsHookEx(WindowsAPI.WH_KEYBOARD_LL, _wesHookProcDelegate, mhandler, 0);
        }

        public static void RemoveHook()
        {
            WindowsAPI.UnhookWindowsHookEx(_hHook);
        }

        private static int WindowKeyboardHookHandler(int nCode, Int32 wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                KeyboardHookStruct keyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                int keyData = keyboardHookStruct.vkCode;
                Key key = KeyInterop.KeyFromVirtualKey(keyData);
                if (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN)
                {
                    OnKeyDownEventHandler(new KeyPressEventArgs(key));
                }
                if(wParam == WM_KEYUP || wParam == WM_SYSKEYUP)
                {
                    OnKeyUpEventHandler(new KeyPressEventArgs(key));
                }
            }
            return WindowsAPI.CallNextHookEx(_hHook, nCode, wParam, lParam);
        }
    }

    public class KeyPressEventArgs : EventArgs
    {
        public Key Key { get; set; }

        public KeyPressEventArgs(Key key)
        {
            this.Key = key;
        }
    }
}

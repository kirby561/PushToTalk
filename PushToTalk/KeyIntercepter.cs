using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows;

namespace PushToTalk {
    delegate void OnKeyActionDelegate(int keycode, Boolean isDown);

    class KeyInterceptor {
        // Hook constants
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        
        // Key Constants
        private const int WM_KEYUP = 0x0101;
        private const int WM_KEYDOWN = 0x0100;

        // Mouse Constants
        private const int WM_XBUTTONDOWN = 0x020B;
        private const int WM_XBUTTONUP = 0x020C;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private const int MK_XBUTTON1 = 131072;
        private const int MK_XBUTTON2 = 65536;

        // Mouse Codes
        private const int WM_XBUTTON1_CODE = 0x100;
        private const int WM_XBUTTON2_CODE = 0x101;
        private const int WM_MBUTTON_CODE = 0x102;
        private const int WM_LBUTTON_CODE = 0x103;
        private const int WM_RBUTTON_CODE = 0x104;

        // Members
        private LowLevelProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        private IntPtr _mouseHookId = IntPtr.Zero;        
        private List<OnKeyActionDelegate> _callbacks;

        public void Initialize() {
            _callbacks = new List<OnKeyActionDelegate>();
            _proc = HookCallback;
            _hookID = SetHookKeyboard(_proc);
            _mouseHookId = SetHookMouse(_proc);
        }

        public void Uninitialize() {
            UnhookWindowsHookEx(_hookID);
            UnhookWindowsHookEx(_mouseHookId);
        }

        public void AddCallback(OnKeyActionDelegate callback) {
            _callbacks.Add(callback);
        }

        public void RemoveCallback(OnKeyActionDelegate callback) {
            _callbacks.Remove(callback);
        }

        private IntPtr SetHookKeyboard(LowLevelProc proc) {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule) {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr SetHookMouse(LowLevelProc proc) {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule) {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private void notifyCallback(int code, Boolean isDown) {
            foreach (OnKeyActionDelegate callback in _callbacks)
                callback(code, isDown);
        }

        private IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0) {
                int vkCode = Marshal.ReadInt32(lParam);

                if (wParam == (IntPtr)WM_KEYDOWN) {
                    notifyCallback(vkCode, true);
                } else if (wParam == (IntPtr)WM_KEYUP) {
                    notifyCallback(vkCode, false);
                } else if (wParam == (IntPtr)WM_LBUTTONDOWN) {
                    notifyCallback(WM_LBUTTON_CODE, true);
                } else if (wParam == (IntPtr)WM_LBUTTONUP) {
                    notifyCallback(WM_LBUTTON_CODE, false);
                } else if (wParam == (IntPtr)WM_RBUTTONDOWN) {
                    notifyCallback(WM_RBUTTON_CODE, true);
                } else if (wParam == (IntPtr)WM_RBUTTONUP) {
                    notifyCallback(WM_RBUTTON_CODE, false);
                } else if (wParam == (IntPtr)WM_MBUTTONDOWN) {
                    notifyCallback(WM_MBUTTON_CODE, true);
                } else if (wParam == (IntPtr)WM_MBUTTONUP) {
                    notifyCallback(WM_MBUTTON_CODE, false);
                } else if (wParam == (IntPtr)WM_XBUTTONDOWN) {

                    MSLLHOOKSTRUCT xButtonInfo = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    if ((xButtonInfo.mouseData & 0xffff0000) == MK_XBUTTON1)
                        notifyCallback(WM_XBUTTON1_CODE, true);
                    else
                        notifyCallback(WM_XBUTTON2_CODE, true);

                } else if (wParam == (IntPtr)WM_XBUTTONUP) {

                    MSLLHOOKSTRUCT xButtonInfo = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    if ((xButtonInfo.mouseData & 0xffff0000) == MK_XBUTTON1)
                        notifyCallback(WM_XBUTTON1_CODE, false);
                    else
                        notifyCallback(WM_XBUTTON2_CODE, false);

                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            public int X;
            public int Y;

            public POINT(int x, int y) {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator Point(POINT p) {
                return new Point(p.X, p.Y);
            }

            public static implicit operator POINT(Point p) {
                return new POINT((int)Math.Round(p.X), (int)Math.Round(p.Y));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT {
            public POINT pt;
            public int mouseData; // be careful, this must be ints, not uints (was wrong before I changed it...). regards, cmew.
            public int flags;
            public int time;
            public UIntPtr dwExtraInfo;
        }
    }
}

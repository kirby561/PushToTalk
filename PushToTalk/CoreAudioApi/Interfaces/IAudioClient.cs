using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CoreAudioApi.Interfaces {
    [StructLayout(LayoutKind.Sequential)]
    public struct WaveFormatEx{
      ushort  wFormatTag;
      ushort  nChannels;
      UInt32 nSamplesPerSec;
      UInt32 nAvgBytesPerSec;
      ushort  nBlockAlign;
      ushort  wBitsPerSample;
      ushort  cbSize;
    }

    [Guid("1CB9AD4C-DBFA-4C32-B178-C2F568A703B2"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioClient {
        [PreserveSig]
        int GetBufferSize(out UInt32 pNumBufferFrames);
        [PreserveSig]
        int GetCurrentPadding(out UInt32 pNumPaddingFrames);
        [PreserveSig]
        int GetDevicePeriod(out long phnsDefaultDevicePeriod, out long phnsMinimumDevicePeriod);
        [PreserveSig]
        int GetMixFormat(out IntPtr ppDeviceFormat);
        [PreserveSig]
        int SetEventHandle(HandleRef eventHandle);
    }
}

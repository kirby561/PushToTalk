using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreAudioApi.Interfaces;
using System.Runtime.InteropServices;
using CoreAudioApi;

namespace PushToTalk {
    public class AudioClientSessionEventsListener : IAudioSessionEvents {

        public int OnDisplayNameChanged([MarshalAs(UnmanagedType.LPWStr)] string NewDisplayName, Guid EventContext) {
            return 0;
        }

        public int OnIconPathChanged([MarshalAs(UnmanagedType.LPWStr)] string NewIconPath, Guid EventContext) {
            return 0;
        }

        public int OnSimpleVolumeChanged(float NewVolume, bool newMute, Guid EventContext) {
            return 0;
        }

        public int OnChannelVolumeChanged(UInt32 ChannelCount, IntPtr NewChannelVolumeArray, UInt32 ChangedChannel, Guid EventContext) {
            return 0;
        }

        public int OnGroupingParamChanged(Guid NewGroupingParam, Guid EventContext) {
            return 0;
        }

        public int OnStateChanged(AudioSessionState NewState) {
            return 0;
        }

        public int OnSessionDisconnected(AudioSessionDisconnectReason DisconnectReason) {
            return 0;
        }
    }
}

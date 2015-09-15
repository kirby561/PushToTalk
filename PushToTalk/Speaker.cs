using CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PushToTalk {
    public class Speaker {

        public Speaker(MMDevice device, AudioEndPointVolumeVolumeRange volumeRange, float normalVolume) {
            Device = device;
            VolumeRange = volumeRange;
            NormalVolume = normalVolume;
        }

        /// <summary>
        /// Sets the volume to the desired value.
        /// </summary>
        /// <param name="volume">  A float between 0.0 and 1.0 that represents 
        ///                         the % of the maximum volume when the mic is down.  </param>
        public void SetVolume(float volume) {
            float dB = -20.0f * (float)Math.Log10(volume);
            Device.AudioEndpointVolume.MasterVolumeLevel = Math.Max(NormalVolume - dB, VolumeRange.MindB);
        }

        public MMDevice Device {
            get;
            set;
        }

        public AudioEndPointVolumeVolumeRange VolumeRange {
            get;
            set;
        }

        public float NormalVolume {
            get;
            set;
        }

        public String FriendlyName {
            get {
                return Device.FriendlyName;
            }
        }

        public String ID {
            get {
                return Device.ID;
            }
        }

    }
}

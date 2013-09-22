using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreAudioApi.Interfaces;
using System.Runtime.InteropServices;

namespace CoreAudioApi {
    public class AudioClient {
        private IAudioClient _audioClient;

        public AudioClient(IAudioClient client) { 
            _audioClient = client; 
        }

        public DevicePeriod GetDevicePeriod() {
            long def;
            long min;
            int result = (_audioClient.GetDevicePeriod(out def, out min));
            Console.WriteLine(result);
            return new DevicePeriod(def, min);
        }
    }
}

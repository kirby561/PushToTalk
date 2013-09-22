using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreAudioApi {
    public struct DevicePeriod {
        private long _defaultPeriod;
        private long _minimumPeriod;

        public DevicePeriod(long defaultPeriod, long minimumPeriod) {
            _defaultPeriod = defaultPeriod;
            _minimumPeriod = minimumPeriod;
        }

        public long DefaultPeriod {
            get {
                return _defaultPeriod;
            }
        }

        public long MinimumPeriod {
            get {
                return _minimumPeriod;
            }
        }
    }
}

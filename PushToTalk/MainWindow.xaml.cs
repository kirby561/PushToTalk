using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CoreAudioApi;
using PushToTalk.Properties;
using System.Resources;
using System.Drawing;
using System.Windows.Interop;

namespace PushToTalk {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private Boolean _waitingForKey = false;
        private int _key = 0xA3;
        private Boolean _keyIsDown = false;
        private KeyInterceptor _interceptor = new KeyInterceptor();
        private KeyMessageLookup _lookup = new KeyMessageLookup();
        private List<MMDevice> _microphones = new List<MMDevice>();
        private List<MMDevice> _speakers = new List<MMDevice>();
        private float _normalSpeakerVolume;
        private Boolean _minimizeToTray = true;
        private AudioEndPointVolumeVolumeRange _volumeRange;
        private MMDevice _activeSpeaker;
        private SolidColorBrush _keyDownBrush = new SolidColorBrush(Colors.Green);
        private SolidColorBrush _keyUpBrush = new SolidColorBrush(Colors.Red);
        private Settings _settings;
        private Icon _muteIcon = PushToTalk.Properties.Resources.RedMic;
        private Icon _unmuteIcon = PushToTalk.Properties.Resources.GreenMic;
        private Icon _mainIcon = PushToTalk.Properties.Resources.mic;
        private float _keyDownVolumeLevel;
        private Boolean _loaded = false;

        public MainWindow() {
            InitializeComponent();

            _interceptor.Initialize();
            _interceptor.AddCallback(OnKeyAction);

            // Get the active microphone and speakers
            MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();
            MMDeviceCollection micList = deviceEnumerator.EnumerateAudioEndPoints(EDataFlow.eCapture, EDeviceState.DEVICE_STATE_ACTIVE);
            MMDeviceCollection speakerList = deviceEnumerator.EnumerateAudioEndPoints(EDataFlow.eRender, EDeviceState.DEVICE_STATE_ACTIVE);

            _activeSpeaker = deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
            _volumeRange = _activeSpeaker.AudioEndpointVolume.VolumeRange;
            _normalSpeakerVolume = _activeSpeaker.AudioEndpointVolume.MasterVolumeLevel;

            for (int i = 0; i < micList.Count; i++) {
                MMDevice mic = micList[i];
                _microphones.Add(mic);
                Console.WriteLine("Found microphone: " + mic.FriendlyName + " " + mic.ID);
            }

            for (int i = 0; i < speakerList.Count; i++) {
                MMDevice speaker = speakerList[i];
                _speakers.Add(speaker);
                Console.WriteLine("Found speaker: " + speaker.FriendlyName + " " + speaker.ID);
            }

            MinimizeToTray.Initialize(this, _muteIcon);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
            SaveSettings();
            _interceptor.Uninitialize();
            UnmuteMic();            
            base.OnClosing(e);
        }

        private void btnChangeKey_Click(object sender, RoutedEventArgs e) {
            _waitingForKey = true;
            lblDirections.Content = "Enter a new key...";
            lblKey.Content = "";
        }

        /// <summary>
        /// Sets the volume tot he desired value.
        /// </summary>
        /// <param name="volume">  A float between 0.0 and 1.0 that represents 
        ///                         the % of the maximum volume when the mic is down.  </param>
        private void SetSpeakerVolume(float volume) {
            // Get the difference in dB
            float dB = -20.0f * (float)Math.Log10(volume);

            Console.WriteLine("Setting active speaker volume to " + volume);
            _activeSpeaker.AudioEndpointVolume.MasterVolumeLevel = Math.Max(_normalSpeakerVolume - dB, _volumeRange.MindB);
        }

        private void MuteMic() {
            foreach (MMDevice mic in _microphones)
                mic.AudioEndpointVolume.Mute = true;
            Console.WriteLine("Muting Microphones");
            MinimizeToTray.ChangeIcon(_muteIcon);
        }

        private void UnmuteMic() {
            foreach (MMDevice mic in _microphones)
                mic.AudioEndpointVolume.Mute = false;
            Console.WriteLine("Unmuting Microphones");
            MinimizeToTray.ChangeIcon(_unmuteIcon);
        }

        private void OnKeyAction(int keycode, Boolean isDown) {
            // If we're waiting for a new key, 
            //    set it as the new key.
            if (_waitingForKey && isDown == false) {
                String message = _lookup.GetMessage(keycode);
                _key = keycode;
                SaveSettings();
                lblDirections.Content = "Current Key: ";
                lblKey.Content = message;
                _waitingForKey = false;
                _keyIsDown = false;
            } else if (!_waitingForKey && keycode == _key) {
                if (isDown && !_keyIsDown) {
                    _keyIsDown = true;
                    UnmuteMic();
                    SetSpeakerVolume(_keyDownVolumeLevel);
                } else if (!isDown && _keyIsDown) {
                    _keyIsDown = false;
                    MuteMic();
                    SetSpeakerVolume(1.0f);
                }
            }
        }

        private void SaveSettings() {
            // Return if we haven't loaded yet
            //    so we don't write initial values
            //    before we've loaded the persisted ones.
            if (!_loaded) return;

            // Set the settings to the current state of the app
            _settings.DefaultKey = _key;
            _settings.VolumeLevelOnKeyDown = _keyDownVolumeLevel;
            _settings.MinimizeToTray = _minimizeCheckBox.IsChecked == true;
            Console.WriteLine("Saving settings");
            
            // Save to disk
            _settings.Save();
        }

        private void LoadSettings() {
            _settings = PushToTalk.Properties.Settings.Default;
            _settings.Reload();

            // Read settings
            _key = _settings.DefaultKey;
            String message = _lookup.GetMessage(_key);
            lblDirections.Content = "Current Key: ";
            lblKey.Content = message;

            _keyDownVolumeLevel = (float)_settings.VolumeLevelOnKeyDown;
            _volumeSlider.Value = _settings.VolumeLevelOnKeyDown * 100.0;
            Console.WriteLine("Loaded volume settings. Volume=" + _settings.VolumeLevelOnKeyDown);

            // Apply minimizetotray if applicable
            if (_settings.MinimizeToTray) {
                _minimizeCheckBox.IsChecked = true;
                MinimizeToTray.Enable();
            } else {
                _minimizeCheckBox.IsChecked = false;
                MinimizeToTray.Disable();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Icon = ToImageSource(_mainIcon);
            LoadSettings();
            MuteMic();
            _loaded = true;
        }

        public static ImageSource ToImageSource(Icon icon) {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            // Set the new value
            _keyDownVolumeLevel = (float)_volumeSlider.Value / 100.0f;
            _volumeLabel.Content = "" + Math.Round(_volumeSlider.Value) + "%";
            Console.WriteLine("Changing volume level on key down to " + _keyDownVolumeLevel);
        }

        private void _volumeSlider_MouseUp(object sender, MouseButtonEventArgs e) {
            // Persist the values
            SaveSettings();
        }

        private void _minimizeCheckBox_Checked(object sender, RoutedEventArgs e) {
            // Disable minimizing to task tray if desired
            if (_minimizeCheckBox.IsChecked == true)
                MinimizeToTray.Enable();
            else
                MinimizeToTray.Disable();

            SaveSettings();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            Console.WriteLine("Window size: " + _window.ActualWidth + ", " + _window.ActualHeight);
        }
    }
}

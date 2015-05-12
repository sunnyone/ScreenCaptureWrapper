using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenCaptureWrapper
{
    public class ShellViewModel : Caliburn.Micro.PropertyChangedBase
    {
        private string ffmpegPath;
        public string FFmpegPath
        {
            get { return ffmpegPath; }
            set { ffmpegPath = value; NotifyOfPropertyChange(() => FFmpegPath); NotifyOfPropertyChange(() => CanRecord); }
        }

        public BindableCollection<Preset> PresetCollection { get; private set; }
        private Preset selectedPreset;
        public Preset SelectedPreset
        {
            get { return selectedPreset; }
            set { selectedPreset = value; NotifyOfPropertyChange(() => SelectedPreset); NotifyOfPropertyChange(() => CanRecord); }
        }

        private string videoPath;
        public string VideoPath
        {
            get { return videoPath; }
            set { videoPath = value; NotifyOfPropertyChange(() => VideoPath); NotifyOfPropertyChange(() => CanRecord); }
        }

        private int videoX;
        public int VideoX
        {
            get { return videoX; }
            set { videoX = value; NotifyOfPropertyChange(() => VideoX); NotifyOfPropertyChange(() => CanRecord); }
        }

        private int videoY;
        public int VideoY
        {
            get { return videoY; }
            set { videoY = value; NotifyOfPropertyChange(() => VideoY); NotifyOfPropertyChange(() => CanRecord); }
        }

        private int videoWidth;
        public int VideoWidth
        {
            get { return videoWidth; }
            set { videoWidth = value; NotifyOfPropertyChange(() => VideoWidth); NotifyOfPropertyChange(() => CanRecord); }
        }

        private int videoHeight;
        public int VideoHeight
        {
            get { return videoHeight; }
            set { videoHeight = value; NotifyOfPropertyChange(() => VideoHeight); NotifyOfPropertyChange(() => CanRecord); }
        }

        private bool isRecording;
        public bool IsRecording
        {
            get { return isRecording; }
            set { 
                isRecording = value; 
                NotifyOfPropertyChange(() => IsRecording);
                NotifyOfPropertyChange(() => CanRecord);
                NotifyOfPropertyChange(() => CanStop);
            }
        }

        private string logText = "";
        public string LogText
        {
            get { return logText; }
            set { logText = value; NotifyOfPropertyChange(() => LogText); }
        }

        public ShellViewModel()
        {
            this.PresetCollection = new BindableCollection<Preset>();
            refreshConfig();
            loadSettings();
        }

        private void loadSettings()
        {
            this.FFmpegPath = Properties.Settings.Default.FFmpegPath;
            selectPresetByName(Properties.Settings.Default.PresetName);
        }

        private void saveSettings()
        {
            Properties.Settings.Default.FFmpegPath = this.FFmpegPath;
            Properties.Settings.Default.PresetName = this.SelectedPreset != null ? this.SelectedPreset.Name : null;
            Properties.Settings.Default.Save();
        }

        // TODO: may be slow
        private void addLog(string line)
        {
            this.LogText = this.LogText + line + "\n";
        }

        private string getConfigPath()
        {
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var exeDir = System.IO.Path.GetDirectoryName(exePath);
            var configPath = System.IO.Path.Combine(exeDir, ScreenCaptureConfig.ConfigFilename);
            return configPath;
        }

        private void refreshConfig()
        {
            var presetName = this.SelectedPreset != null ? this.SelectedPreset.Name : null;

            this.PresetCollection.Clear();
            try
            {
                var config = ScreenCaptureConfig.ReadConfig(getConfigPath());
                foreach (var preset in config.Presets)
                {
                    this.PresetCollection.Add(preset);
                }
            }
            catch (Exception ex)
            {
                addLog("Failed to load the config file: " + ex.ToString());
            }

            if (presetName != null)
                selectPresetByName(presetName);
        }

        private void selectPresetByName(string presetName)
        {
            var preset = this.PresetCollection.FirstOrDefault(x => x.Name == presetName);
            this.SelectedPreset = preset;
        }

        public void OpenConfig()
        {
            var configPath = getConfigPath();

            if (!System.IO.File.Exists(configPath))
            {
                System.IO.File.WriteAllText(configPath, "");
            }
            var proc = System.Diagnostics.Process.Start(configPath);
            proc.Exited += (sender, e) =>
            {
                refreshConfig();
            };
            proc.EnableRaisingEvents = true;
        }

        public void SelectFFmpegPath()
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                this.FFmpegPath = dialog.FileName;
            }
        }

        public void SelectFile()
        {
            using (var dialog = new System.Windows.Forms.SaveFileDialog())
            {
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                this.VideoPath = dialog.FileName;
            }
        }

        public void PickPosition()
        {
            SelectScreenPositionUtil.SelectScreenPositionAsync()
                .ContinueWith(t =>
                {
                    var exception = t.Exception; // HACK: ignore exception

                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        var result = t.Result;
                        this.VideoX = (int) result.X;
                        this.VideoY = (int) result.Y;
                        this.VideoWidth = (int) result.Width;
                        this.VideoHeight = (int) result.Height;
                    }
                });
        }

        private Int32Rect getRect()
        {
            return new Int32Rect(this.VideoX, this.VideoY, this.VideoWidth, this.VideoHeight);
        }

        public bool CanRecord
        { 
            get 
            {
                if (IsRecording)
                    return false;

                if (string.IsNullOrWhiteSpace(FFmpegPath))
                    return false;

                if (this.SelectedPreset == null)
                    return false;

                var rect = getRect();
                if (!(this.VideoX > 0 && this.VideoY > 0 &&
                      this.VideoHeight > 0 && this.VideoWidth > 0))
                    return false;

                return !string.IsNullOrWhiteSpace(this.VideoPath);
            } 
        }

        private CancellationTokenSource cancellationTokenSource;
        public void Record()
        {
            var rect = getRect();
            var recordParam = new RecordParam()
            {
                Left = rect.X,
                Top = rect.Y,
                Width = rect.Width,
                Height = rect.Height,
                OutputPath = this.VideoPath
            };

            cancellationTokenSource = new CancellationTokenSource();
            Recorder.Record(this.FFmpegPath, this.SelectedPreset.Arguments, recordParam, cancellationTokenSource.Token, new Progress<string>(s => this.addLog(s)))
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        this.addLog("Exception occured when recording: " + t.Exception.ToString());
                    }
                    
                    this.IsRecording = false;
                });
                
            this.IsRecording = true;
        }

        public bool CanStop { get { return IsRecording; } }
        public void Stop()
        {
            if (IsRecording)
                cancellationTokenSource.Cancel();
        }

        public void OnClosed()
        {
            saveSettings();
        }
    }
}

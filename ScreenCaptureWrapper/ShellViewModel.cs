using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenCaptureWrapper
{
    public class ShellViewModel : Caliburn.Micro.PropertyChangedBase
    {
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

        public void OpenConfig()
        {
            var configPath = getConfigPath();

            if (!System.IO.File.Exists(configPath))
            {
                System.IO.File.WriteAllText(configPath, "");
            }
            System.Diagnostics.Process.Start(configPath);
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
            SelectPositionWindow.SelectScreenPositionAsync()
                .ContinueWith(t =>
                {
                    var exception = t.Exception; // HACK: ignore exception

                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        var result = t.Result;
                        this.VideoX = result.X;
                        this.VideoY = result.Y;
                        this.VideoWidth = result.Width;
                        this.VideoHeight = result.Height;
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
            Recorder.Record(getConfigPath(), recordParam, cancellationTokenSource.Token, new Progress<string>(s => this.addLog(s)))
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
    }
}

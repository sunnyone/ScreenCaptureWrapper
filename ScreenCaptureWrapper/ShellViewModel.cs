using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenCaptureWrapper
{
    public class ShellViewModel : Caliburn.Micro.PropertyChangedBase
    {
        private string videoPath;
        public string VideoPath
        {
            get { return videoPath; }
            set { videoPath = value; NotifyOfPropertyChange(() => VideoPath); }
        }

        private int videoX;
        public int VideoX
        {
            get { return videoX; }
            set { videoX = value; NotifyOfPropertyChange(() => VideoX); }
        }

        private int videoY;
        public int VideoY
        {
            get { return videoY; }
            set { videoY = value; NotifyOfPropertyChange(() => VideoY);  }
        }

        private int videoWidth;
        public int VideoWidth
        {
            get { return videoWidth; }
            set { videoWidth = value; NotifyOfPropertyChange(() => VideoWidth); }
        }

        private int videoHeight;
        public int VideoHeight
        {
            get { return videoHeight; }
            set { videoHeight = value; NotifyOfPropertyChange(() => VideoHeight); }
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

        public bool CanRecord { get { return true; } }
        public void Record()
        {

        }

        public bool CanStop { get { return false; } }
        public void Stop()
        {

        }
    }
}

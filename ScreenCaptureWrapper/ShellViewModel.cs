using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenCaptureWrapper
{
    public class ShellViewModel : Caliburn.Micro.PropertyChangedBase
    {
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
        }

        public void PickPosition()
        {
            this.VideoX = 100;
            this.VideoY = 100;
            this.VideoHeight = 100;
            this.VideoWidth = 100;
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

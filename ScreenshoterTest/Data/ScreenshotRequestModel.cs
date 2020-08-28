using System;

namespace ScreenshoterTest.Data
{
    public class ScreenshotRequestModel
    {
        #region EventHandler

        public delegate void RequestModelEventHandler(object sender, EventArgs e);

        public event RequestModelEventHandler ModelChangeEvent;
        protected virtual void RaiseEvent()
        {
            ModelChangeEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        private string _result;
        public string Url { get; set; }
        public TimeSpan Elapsed { get; set; }
        public string Result
        {
            get => _result;
            set
            {
                _result = value;
                RaiseEvent();
            }
        }

        #endregion
    }
}
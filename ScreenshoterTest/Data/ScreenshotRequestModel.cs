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

        private DateTime _startedAt;
        private DateTime _finishedAt;
        private string _result;

        public string Url { get; set; }

        public DateTime StartedAt
        {
            get => _startedAt;
            set
            {
                _startedAt = value;
                if (value != default) RaiseEvent();
            }
        }

        public DateTime FinishedAt
        {
            get => _finishedAt;
            set
            {
                _finishedAt = value;
                if (value != default) RaiseEvent();
            }
        }

        public string Result
        {
            get => _result;
            set
            {
                _result = value;
                if (value != default) RaiseEvent();
            }
        }

        #endregion
    }
}
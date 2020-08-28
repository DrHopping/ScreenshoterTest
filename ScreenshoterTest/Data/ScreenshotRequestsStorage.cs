using System;
using System.Collections;
using System.Collections.Generic;

namespace ScreenshoterTest.Data
{
    public class ScreenshotRequestsStorage : ICollection<ScreenshotRequestModel>
    {
        private List<ScreenshotRequestModel> _requests;

        public ScreenshotRequestsStorage(List<string> urls)
        {
            _requests = new List<ScreenshotRequestModel>();
            urls.ForEach((url) => Add(new ScreenshotRequestModel { Url = url }));
        }

        #region EventHandler

        public delegate void StorageEventHandler(object sender, EventArgs e);

        public event StorageEventHandler StorageEvent;

        protected virtual void RaiseEvent(object sender, EventArgs e)
        {
            StorageEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IColletion

        public IEnumerator<ScreenshotRequestModel> GetEnumerator()
        {
            return _requests.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(ScreenshotRequestModel item)
        {
            _requests.Add(item);
            item.ModelChangeEvent += RaiseEvent;
        }

        public void Clear()
        {
            _requests.Clear(); ;
        }

        public bool Contains(ScreenshotRequestModel item)
        {
            return _requests.Contains(item);
        }

        public void CopyTo(ScreenshotRequestModel[] array, int arrayIndex)
        {
            _requests.CopyTo(array, arrayIndex);
        }

        public bool Remove(ScreenshotRequestModel item)
        {
            return _requests.Remove(item);
        }

        public int Count => _requests.Count;

        public bool IsReadOnly => false;

        #endregion
    }
}
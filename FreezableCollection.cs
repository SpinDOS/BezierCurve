using System;
using System.Collections.ObjectModel;

namespace BezierCurve
{
    public class FreezableCollection<T> : ObservableCollection<T>
    {
        public bool Frozen { get; private set; }

        public IDisposable Frost()
        {
            CheckFrozen();
            Frozen = true;
            return new DefrostOnDispose(this);
        }

        protected override void InsertItem(int index, T item)
        {
            CheckFrozen();
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            CheckFrozen();
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            CheckFrozen();
            base.SetItem(index, item);
        }

        protected override void ClearItems()
        {
            CheckFrozen();
            base.ClearItems();
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            CheckFrozen();
            base.MoveItem(oldIndex, newIndex);
        }

        private void CheckFrozen()
        {
            if (Frozen)
                throw new InvalidOperationException("Cannot modify frozen collection");
        }

        private class DefrostOnDispose : IDisposable
        {
            private FreezableCollection<T> _freezableCollection;

            public DefrostOnDispose(FreezableCollection<T> freezableCollection) =>
                _freezableCollection = freezableCollection;

            public void Dispose()
            {
                if (_freezableCollection == null)
                    return;
                
                _freezableCollection.Frozen = false;
                _freezableCollection = null;
            }
        }
    }
}
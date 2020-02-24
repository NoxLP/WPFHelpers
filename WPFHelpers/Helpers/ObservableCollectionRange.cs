﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFHelpers.Helpers
{
    public interface INotifyRangeCollectionChanged : INotifyCollectionChanged
    {
        event NotifyCollectionChangedEventHandler CollectionChangedRange;
    }
    /// <summary>
    /// An observable collection with support for addrange and clear
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ObservableCollectionRange<T> : ObservableCollection<T>, INotifyRangeCollectionChanged
    {
        private bool _addingRange;

        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler CollectionChangedRange;

        protected virtual void OnCollectionChangedRange(NotifyCollectionChangedEventArgs e)
        {
            if ((CollectionChangedRange == null) || _addingRange) return;
            using (BlockReentrancy())
            {
                CollectionChangedRange(this, e);
            }
        }
        
        public void AddRange(IEnumerable<T> collection)
        {
            CheckReentrancy();
            var newItems = new List<T>();
            if ((collection == null) || (Items == null)) return;
            using (var enumerator = collection.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    _addingRange = true;
                    Add(enumerator.Current);
                    _addingRange = false;
                    newItems.Add(enumerator.Current);
                }
            }
            OnCollectionChangedRange(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems));
        }
        
        protected override void ClearItems()
        {
            CheckReentrancy();
            var oldItems = new List<T>(this);
            base.ClearItems();
            OnCollectionChangedRange(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems));
        }

        protected override void InsertItem(int index, T item)
        {
            CheckReentrancy();
            base.InsertItem(index, item);
            OnCollectionChangedRange(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            CheckReentrancy();
            var item = base[oldIndex];
            base.MoveItem(oldIndex, newIndex);
            OnCollectionChangedRange(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }

        protected override void RemoveItem(int index)
        {
            CheckReentrancy();
            var item = base[index];
            base.RemoveItem(index);
            OnCollectionChangedRange(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        protected override void SetItem(int index, T item)
        {
            CheckReentrancy();
            var oldItem = base[index];
            base.SetItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, oldItem, item, index));
        }
    }

    /// <summary>
    /// A read only observable collection with support for addrange and clear
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ReadOnlyObservableCollectionRange<T> : ReadOnlyObservableCollection<T>
    {
        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler CollectionChangedRange;

        public ReadOnlyObservableCollectionRange(ObservableCollectionRange<T> list) : base(list)
        {
            list.CollectionChangedRange += HandleCollectionChangedRange;
        }

        private void HandleCollectionChangedRange(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChangedRange(e);
        }

        protected virtual void OnCollectionChangedRange(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChangedRange != null)
            {
                CollectionChangedRange(this, args);
            }
        }

    }
}

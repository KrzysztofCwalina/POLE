using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Azure.Core.Pole
{
    public class PoleArray<T> : IReadOnlyList<T>
    {
        const int LengthOffset = 0;
        const int ItemSizeOffset = 4; // TODO: we don't need size both here and in T. Maybe we should just encode the item schema ID?
        const int ItemsOffset = 8;

        readonly PoleReference _reference;
        readonly PoleType _itemType;

        public PoleArray(PoleReference reference, PoleType itemType)
        {
            // TODO: verify that the reference is indeed PoleArray<T>
            _reference = reference;
            _itemType = itemType;
        }
        public PoleReference Reference => _reference;

        public static PoleArray<T> Allocate(PoleHeap heap, int length)
        {
            var type = new PoleType(typeof(T));
            var bufferLength = type.Size * length + sizeof(int) + sizeof(int);
            var reference = heap.Allocate(bufferLength);
            reference.WriteInt32(LengthOffset, length);
            reference.WriteInt32(ItemSizeOffset, type.Size);
            return new PoleArray<T>(reference, type);
        }

        public T this[int index]
        {
            set
            {
                if (index >= Count) throw new IndexOutOfRangeException();
                int itemOffset = ItemSize * index + ItemsOffset;
                if (typeof(T) == typeof(int))
                {
                    _reference.WriteInt32(itemOffset, (int)(object)value);
                }
                else if (typeof(IObject).IsAssignableFrom(typeof(T)))
                {
                    IObject iobj = value as IObject;
                    var reference = iobj.Reference;
                    _reference.WriteInt32(itemOffset, reference.Address);
                }
                else
                {
                    // should this check be done at construction?
                    throw new NotSupportedException($"type {typeof(T)} not supported");
                }
            }
            get
            {
                if (index >= Count) throw new IndexOutOfRangeException();
                int itemOffset = ItemSize * index + ItemsOffset;
                if (typeof(T) == typeof(int))
                {

                    int value = _reference.ReadInt32(itemOffset);
                    var i = Unsafe.As<int, T>(ref value);
                    return i;
                }
                else if(typeof(T) == typeof(string))
                {
                    var str = _reference.ReadString(itemOffset);                    
                    return (T)(object)str;

                }
                else if (typeof(IObject).IsAssignableFrom(typeof(T)))
                {
                    int itemAddress = _reference.ReadInt32(itemOffset);
                    var reference = new PoleReference(_reference.Heap, itemAddress);
                    return (T)_reference.Heap.Deserialize(reference, typeof(T)); // TODO: can reflection be eliminated?
                }
                else
                {
                    // should this check be done at construction?
                    throw new NotSupportedException($"type {typeof(T)} not supported");
                }
            }
        }

        public int Count => _reference.ReadInt32(LengthOffset);

        private int ItemSize => _reference.ReadInt32(ItemSizeOffset);
        private IEnumerator<T> GetEnumeratorCore() => new Enumerator(this);
        class Enumerator : IEnumerator<T>
        {
            int _index = -1;
            PoleArray<T> _array;

            public Enumerator(PoleArray<T> array)
            {
                _index = 0;
                _array = array;
            }

            public T Current
            {
                get
                {
                    if (_index == -1) throw new InvalidOperationException("call MoveNext first");
                    return _array[_index];
                }
            } 

            public bool MoveNext()
            {
                if (_index + 1 < _array.Count)
                {
                    _index++;
                    return true;
                }
                return false;
            }

            object IEnumerator.Current => _array[_index];
            public void Dispose() { }
            public void Reset() => _index = -1;
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumeratorCore();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorCore();
    }
}

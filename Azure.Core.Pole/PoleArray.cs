using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Azure.Core.Pole
{
    public class PoleArray<T> : IReadOnlyList<T>
    {
        const ulong ArrayId = PoleType.ArrayId;
        const int LengthOffset = 8;
        const int ItemSizeOffset = 12;
        const int ItemsOffset = 16;
        const int SizeHeader = 20;

        readonly PoleReference _reference;

        public PoleArray(PoleReference reference)
        {
            var typeId = reference.ReadTypeId();
            if ((typeId & 0xFFFFFFFFFFFFFF00) != ArrayId) throw new InvalidCastException("invalid cast");
            _reference = reference;
        }
        public PoleReference Reference => _reference;

        public PoleArray(PoleHeap heap, int length)
        {
            if (!PoleType.TryGetSize(typeof(T), out int itemSize))
            {
                throw new InvalidOperationException($"{typeof(T)} is not a POLE type.");
            }

            int size = SizeHeader + length * itemSize;
            _reference = heap.AllocateObject(size, ArrayId);
            _reference.WriteInt32(LengthOffset, length);
            _reference.WriteInt32(ItemSizeOffset, itemSize);
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
                    var reference = new PoleReference(_reference.Heap, _reference.Address + itemAddress);
                    return (T)reference.Deserialize(typeof(T));
                }
                else
                {
                    return PoleType.Deserialize<T>(new PoleReference(_reference.Heap, _reference.Address + itemOffset));
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

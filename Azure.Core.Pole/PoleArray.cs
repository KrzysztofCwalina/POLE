// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Azure.Core.Pole
{
    public class PoleArray<T> : IReadOnlyList<T>
    {
        const ulong ArrayId = PoleType.ArrayId;
        const int LengthOffset = 0;
        const int ItemSizeOffset = 4;
        const int ItemsOffset = 8;

        readonly Reference _reference;

        public PoleArray(Reference reference)
        {
            var typeId = reference.ReadTypeId();
            if ((typeId & PoleType.SchemaIdMask) != ArrayId) throw new InvalidCastException("invalid cast");
            _reference = reference;
        }
        public Reference Reference => _reference;

        public PoleArray(PoleHeap heap, int length)
        {
            if (!PoleType.TryGetSize(typeof(T), out int itemSize))
            {
                throw new InvalidOperationException($"{typeof(T)} is not a POLE type.");
            }

            int size = ItemsOffset + length * itemSize;
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
                    var address = iobj.Address;
                    _reference.WriteInt32(itemOffset, address);
                }
                else if (typeof(Utf8) == typeof(T))
                {
                    var utf8 = (Utf8)(object)value;
                    _reference.WriteInt32(itemOffset, utf8.Address);
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
                    var reference = new Reference(_reference.Heap, _reference.Address + itemAddress);
                    return (T)reference.Deserialize(typeof(T));
                }
                else
                {
                    return PoleType.Deserialize<T>(new Reference(_reference.Heap, _reference.Address + itemOffset));
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

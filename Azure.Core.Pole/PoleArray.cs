using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Azure.Core.Pole
{
    internal class PoleArray<T> : IObject, IList<T>, IReadOnlyList<T>
    {
        readonly PoleReference _reference;
        readonly int _length;

        PoleReference IObject.Reference => _reference;

        public PoleArray(PoleReference reference)
        {
            _reference = reference;
            _length = _reference.ReadInt32(0);
        }

        public static PoleArray<T> Allocate(PoleHeap heap, int length)
        {
            var bufferLength = sizeof(int) * length + sizeof(int);
            var reference = heap.Allocate(bufferLength);
            Span<byte> buffer = heap.GetBytes(reference.Address, bufferLength);
            BinaryPrimitives.WriteInt32LittleEndian(buffer, length);
            return new PoleArray<T>(reference);
        }

        public T this[int index]
        {
            set
            {
                if (index >= _length) throw new IndexOutOfRangeException();
                if (typeof(T) == typeof(int))
                {
                    int i4 = Unsafe.As<T, int>(ref Unsafe.AsRef(value));
                    _reference.WriteInt32(sizeof(int) + index * sizeof(int), i4);
                }
                else if (typeof(IObject).IsAssignableFrom(typeof(T)))
                {
                    IObject iobj = value as IObject;
                    var reference = iobj.Reference;
                    _reference.WriteInt32(sizeof(int) + index * sizeof(int), reference.Address);
                }
                else
                {
                    // should this check be done at construction?
                    throw new NotSupportedException($"type {typeof(T)} not supported");
                }
            }
            get
            {
                if (index >= _length) throw new IndexOutOfRangeException();
                if (typeof(T) == typeof(int))
                {
                    int value = _reference.ReadInt32(sizeof(int) + index * sizeof(int));
                    var i = Unsafe.As<int, T>(ref value);
                    return i;
                }
                else if (typeof(IObject).IsAssignableFrom(typeof(T)))
                {
                    // TODO: can reflection be eliminated with static interfaces?
                    int address = _reference.ReadInt32(sizeof(int) + index * sizeof(int));
                    var reference = new PoleReference(_reference.Heap, address);
                    return (T)_reference.Heap.Deserialize(reference, typeof(T));
                }
                else
                {
                    // should this check be done at construction?
                    throw new NotSupportedException($"type {typeof(T)} not supported");
                }
            }
        }

        public int Count => _length;

        #region NYI_Interface
        int IList<T>.IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        bool ICollection<T>.IsReadOnly => throw new NotImplementedException();

        void IList<T>.Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<T>.Contains(T item)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

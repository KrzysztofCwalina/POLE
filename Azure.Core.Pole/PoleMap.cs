//using System;
//using System.Buffers.Binary;
//using System.Collections;
//using System.Collections.Generic;

//namespace Azure.Core.Pole
//{
//    internal class PoleMap<TKey, TValue> : IDictionary<TKey, TValue>
//    {
//        const int CountOffset = 0;
//        const int CapacityOffset = 4;
//        const int ListOffset = 8;

//        readonly PoleReference _reference;

//        private PoleMap(PoleReference reference) => _reference = reference;

//        public static PoleMap<TKey, TValue> Allocate(PoleHeap heap, int capacity)
//        {
//            var bufferLength = sizeof(int) * 2 * capacity + sizeof(int) + sizeof(int);
//            var reference = heap.Allocate(bufferLength);
//            Span<byte> buffer = heap.GetBytes(reference.Address, bufferLength);
//            BinaryPrimitives.WriteInt32LittleEndian(buffer, 0); // count
//            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(CapacityOffset), capacity); // count
//            return new PoleMap<TKey, TValue>(reference);
//        }

//        public int Count => _reference.ReadInt32(CountOffset);
//        public int Capacity => _reference.ReadInt32(CapacityOffset);

//        public bool TryGetValue(TKey key, out TValue value)
//        {
//            var count = Count;
//            for (int i = 0; i < count; i++) {
//                var kvpOffset = (i * 8) + ListOffset;
//                int keyAddress = _reference.ReadInt32(kvpOffset); 
//                if (Equals(key, keyAddress))
//                {
//                    var valueReference = _reference.ReadReference(kvpOffset + 4);
//                    throw new NotImplementedException();
//                }
//            }
//            value = default;
//            return false;
//        }

//        public bool Equals(TKey lefth, int rightAddress)
//        {
//            throw new NotImplementedException();
//        }

//        public bool ContainsKey(TKey key)
//        {
//            throw new NotImplementedException();
//        }

//        public TValue this[TKey key]
//        {
//            get
//            {
//                if(TryGetValue(key, out var value)) { return value; }
//                throw new KeyNotFoundException();
//            }
//        }

//        public IEnumerable<TKey> Keys => throw new NotImplementedException();

//        public IEnumerable<TValue> Values => throw new NotImplementedException();

//        ICollection<TKey> IDictionary<TKey, TValue>.Keys => throw new NotImplementedException();

//        ICollection<TValue> IDictionary<TKey, TValue>.Values => throw new NotImplementedException();

//        int ICollection<KeyValuePair<TKey, TValue>>.Count => throw new NotImplementedException();

//        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => throw new NotImplementedException();

//        TValue IDictionary<TKey, TValue>.this[TKey key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

//        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
//        {
//            throw new NotImplementedException();
//        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            throw new NotImplementedException();
//        }

//        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
//        {
//            throw new NotImplementedException();
//        }

//        bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
//        {
//            throw new NotImplementedException();
//        }

//        bool IDictionary<TKey, TValue>.Remove(TKey key)
//        {
//            throw new NotImplementedException();
//        }

//        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
//        {
//            throw new NotImplementedException();
//        }

//        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
//        {
//            throw new NotImplementedException();
//        }

//        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
//        {
//            throw new NotImplementedException();
//        }

//        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
//        {
//            throw new NotImplementedException();
//        }

//        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
//        {
//            throw new NotImplementedException();
//        }

//        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
//        {
//            throw new NotImplementedException();
//        }

//        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}

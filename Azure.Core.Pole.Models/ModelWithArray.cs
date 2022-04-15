using Azure.Core.Pole;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Azure.Core.Pole.TestModels
{
    internal struct ModelWithCollectionSchema
    {
        public const ulong TypeId = 0xfe106fc3b2994232;

        public const int AllOffset = 16;
        public const int Size = 20;
    }

    public class ModelWithArray
    {
        private readonly PoleReference _reference;
        private PoleArray<int> _array;

        private ModelWithArray(PoleReference reference) => _reference = reference;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ModelWithArray Deserialize(PoleHeap heap)
        {
            var reference = heap.GetAt(0);
            if (reference.ReadTypeId() != ModelWithCollectionSchema.TypeId) throw new InvalidCastException();
            return new (reference);
        } 
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ModelWithArray Deserialize(Stream stream)
        {
            var heap = PoleHeap.ReadFrom(stream);
            return Deserialize(heap);
        }

        public IReadOnlyList<int> All
        {
            get
            {
                var reference = _reference.ReadReference(ModelWithCollectionSchema.AllOffset);
                if (_array == null) _array = new PoleArray<int>(reference, new PoleType(typeof(int)));
                return _array;
            }
        }
    }
}

namespace Azure.Core.Pole.TestModels.Server
{
    public class ModelWithArray
    {
        private readonly PoleReference _reference;
        private PoleArray<int> _array;

        private ModelWithArray(PoleReference reference)
        {
            _reference = reference;
            _array = null;
        } 

        public static ModelWithArray Allocate(PoleHeap heap)
        {
            PoleReference reference = heap.Allocate(ModelWithCollectionSchema.Size);
            reference.WriteSchemaId(HelloModelSchema.V1);
            return new ModelWithArray(reference);
        }

        public PoleArray<int> All
        {
            get => _array;
            set {
                _array = value;
                _reference.WriteReference(ModelWithCollectionSchema.AllOffset, value.Reference);
            }
        }
    }
}

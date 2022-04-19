using Azure.Core.Pole;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Azure.Core.Pole.TestModels
{
    internal struct ModelWithCollectionSchema
    {
        public const ulong SchemaId = 0xfe106fc3b2994200;

        public const int IntegersOffset = 16;
        public const int StringsOffset = 20;
        public const int Size = 24;
    }

    public class ModelWithArray
    {
        private readonly PoleReference _reference;
        private PoleArray<int> _integers;
        private PoleArray<string> _strings;

        private ModelWithArray(PoleReference reference) => _reference = reference;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ModelWithArray Deserialize(PoleHeap heap)
        {
            var reference = heap.GetAt(0);
            if (reference.ReadTypeId() != ModelWithCollectionSchema.SchemaId) throw new InvalidCastException();
            return new (reference);
        } 
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ModelWithArray Deserialize(Stream stream)
        {
            var heap = PoleHeap.ReadFrom(stream);
            return Deserialize(heap);
        }

        public IReadOnlyList<int> Integers
        {
            get
            {
                var reference = _reference.ReadReference(ModelWithCollectionSchema.IntegersOffset);
                if (_integers == null) _integers = new PoleArray<int>(reference);
                return _integers;
            }
        }

        public IReadOnlyList<string> Strings
        {
            get
            {
                var reference = _reference.ReadReference(ModelWithCollectionSchema.StringsOffset);
                if (_strings == null) _strings = new PoleArray<string>(reference);
                return _strings;
            }
        }
    }
}

namespace Azure.Core.Pole.TestModels.Server
{
    public class ModelWithArray
    {
        private readonly PoleReference _reference;
        private PoleArray<int> _integers;
        private PoleArray<Utf8> _strings;

        private ModelWithArray(PoleReference reference)
        {
            _reference = reference;
            _integers = null;
            _strings = null;
        } 

        public static ModelWithArray Allocate(PoleHeap heap)
        {
            PoleReference reference = heap.Allocate(ModelWithCollectionSchema.Size);
            reference.WriteTypeId(ModelWithCollectionSchema.SchemaId);
            return new ModelWithArray(reference);
        }

        public PoleArray<int> Integers
        {
            get => _integers;
            set {
                _integers = value;
                _reference.WriteReference(ModelWithCollectionSchema.IntegersOffset, value.Reference);
            }
        }

        public PoleArray<Utf8> Strings
        {
            get => _strings;
            set
            {
                _strings = value;
                _reference.WriteReference(ModelWithCollectionSchema.StringsOffset, value.Reference);
            }
        }
    }
}

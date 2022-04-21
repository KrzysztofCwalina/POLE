// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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

        public const int IntegersOffset = 0;
        public const int StringsOffset = 4;
        public const int Size = 8;
    }

    public class ModelWithArray
    {
        private readonly PoleReference _reference;
        private PoleArray<int> _integers;
        private PoleArray<string> _strings;

        private ModelWithArray(PoleReference reference) => _reference = reference;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ModelWithArray Deserialize(ArrayPoolHeap heap)
        {
            var reference = heap.GetRoot();
            if (reference.ReadTypeId() != ModelWithCollectionSchema.SchemaId) throw new InvalidCastException();
            return new (reference);
        } 
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ModelWithArray Deserialize(Stream stream)
        {
            var heap = ArrayPoolHeap.ReadFrom(stream);
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

        public ModelWithArray(PoleHeap heap)
        {
            _reference = heap.AllocateObject(ModelWithCollectionSchema.Size, ModelWithCollectionSchema.SchemaId);
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

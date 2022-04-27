// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Pole;
using System;
using System.ComponentModel;
using System.IO;

namespace Azure.Core.Pole.TestModels
{
    internal struct HelloModelSchema
    {
        public const ulong SchemaId = 0xFFFFFFFFFFFFFE00; 

        public const int RepeatCountOffset = 0;
        public const int MessageOffset = 4; 
        public const int IsEnabledOffset = 8; // TODO: what about alignment?
        public const int Size = 9;
    }

    public struct HelloModel
    {
        private readonly ReadOnlyReference _reference;
        private HelloModel(ReadOnlyReference reference) => _reference = reference;

        public HelloModel(BinaryData poleData)
            => _reference = new ReadOnlyReference(poleData, HelloModelSchema.SchemaId);

        public int RepeatCount => _reference.ReadInt32(HelloModelSchema.RepeatCountOffset);

        public bool IsEnabled => _reference.ReadBoolean(HelloModelSchema.IsEnabledOffset);

        public string Message => _reference.ReadString(HelloModelSchema.MessageOffset);
    }
}

namespace Azure.Core.Pole.TestModels.Server
{
    public struct HelloModel
    {
        private readonly Reference _reference;
        private HelloModel(Reference reference) => _reference = reference;

        public HelloModel(PoleHeap heap)
        {
            _reference = heap.AllocateObject(HelloModelSchema.Size, HelloModelSchema.SchemaId);
        }

        public int RepeatCount
        {
            get => _reference.ReadInt32(HelloModelSchema.RepeatCountOffset);
            set => _reference.WriteInt32(HelloModelSchema.RepeatCountOffset, value);
        }
        public bool IsEnabled
        {
            get => _reference.ReadBoolean(HelloModelSchema.IsEnabledOffset);
            set => _reference.WriteBoolean(HelloModelSchema.IsEnabledOffset, value);
        }
        public string Message
        {
            get => _reference.ReadString(HelloModelSchema.MessageOffset);
            set => _reference.WriteString(HelloModelSchema.MessageOffset, value);
        }
    }
}

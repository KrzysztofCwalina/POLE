using Azure.Core.Pole;
using System;
using System.ComponentModel;
using System.IO;

namespace Azure.Core.Pole.TestModels
{
    internal struct ModelSchema
    {
        public const ulong SchemaId = 0xfe106fc3b2994200;

        public const int RepeatCountOffset = 16;
        public const int IsEnabledOffset = 20;
        public const int MessageOffset = 21;
        public const int Size = 25;
    }

    // used on the client to compose requests, aka input model
    public class ClientInputModel
    {
        public ClientInputModel() { }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Serialize(Stream stream)
        {
            var heap = new ArrayPoolHeap(); // TODO: this should write to stack spans, then to stream
            var reference = heap.Allocate(ModelSchema.Size);
            reference.WriteTypeId(ModelSchema.SchemaId);
            reference.WriteInt32(ModelSchema.RepeatCountOffset, RepeatCount);
            reference.WriteBoolean(ModelSchema.IsEnabledOffset, IsEnabled);
            reference.WriteString(ModelSchema.MessageOffset, Message);
            heap.WriteTo(stream);
        }

        // TODO: should the fields be on the heap? It would work for flat objects, but not if sub-objects are new'able.
        public int RepeatCount { get; set; }
        public bool IsEnabled { get; set; }
        public string Message { get; set; }
    }

    // used by the client to parse server responses, aka output model
    public readonly struct ClientOutputModel
    {
        private readonly PoleReference _reference;
        private ClientOutputModel(PoleReference reference) => _reference = reference;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ClientOutputModel Deserialize(ArrayPoolHeap heap)
        {
            var reference = heap.GetRoot();
            if (reference.ReadTypeId() != ModelSchema.SchemaId) throw new InvalidCastException();
            return new(reference);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ClientOutputModel Deserialize(Stream stream)
        {
            var heap = ArrayPoolHeap.ReadFrom(stream);
            return Deserialize(heap);
        }
        public int RepeatCount => _reference.ReadInt32(ModelSchema.RepeatCountOffset);

        public bool IsEnabled => _reference.ReadBoolean(ModelSchema.IsEnabledOffset);

        public string Message => _reference.ReadString(ModelSchema.MessageOffset);
    }

    // returned from clinet to user, user makes changes, sends changes back, aka roundtrip
    // TODO: this should support "patch"
    public class ClientRountripingModel 
    {
        private readonly PoleReference __reference;
        private string _message; // mutable variable size fields need to be on the GC heap
        private ClientRountripingModel(PoleReference reference) => __reference = reference;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ClientRountripingModel Deserialize(ArrayPoolHeap heap)
        {
            var reference = heap.GetRoot();
            if (reference.ReadTypeId() != ModelSchema.SchemaId) throw new InvalidCastException();
            return new(reference);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ClientRountripingModel Deserialize(Stream stream)
        {
            var heap = ArrayPoolHeap.ReadFrom(stream);
            return Deserialize(heap);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Serialize(Stream stream)
        {
            var heap = new ArrayPoolHeap(); // TODO: this should write directly to pipe, not to in memory buffers
            var reference = heap.Allocate(ModelSchema.Size);
            reference.WriteTypeId(ModelSchema.SchemaId);
            reference.WriteInt32(ModelSchema.RepeatCountOffset, RepeatCount);
            reference.WriteBoolean(ModelSchema.IsEnabledOffset, IsEnabled);

            if (_message != null) // mutated
            {
                var message = Utf8.Allocate(heap, _message);
                reference.WriteUtf8(ModelSchema.MessageOffset, message);
            }
            else
            {
                throw new NotImplementedException(); // should just copy bytes
            }
            heap.WriteTo(stream);
        }

        public int RepeatCount
        {
            get => __reference.ReadInt32(ModelSchema.RepeatCountOffset);
            set => __reference.WriteInt32(ModelSchema.RepeatCountOffset, value);
        }
        public bool IsEnabled
        {
            get => __reference.ReadBoolean(ModelSchema.IsEnabledOffset);
            set => __reference.WriteBoolean(ModelSchema.IsEnabledOffset, value);
        }
        public string Message
        {
            get
            {
                if (_message == null) return __reference.ReadString(ModelSchema.MessageOffset);
                return _message;
            }

            set => _message = value;
        }
    }

    // used by the server to parse client requests
    public readonly struct ServerRequestModel
    {
        private readonly PoleReference _reference;
        private ServerRequestModel(PoleReference reference) => _reference = reference;

        public static ServerRequestModel Deserialize(ArrayPoolHeap heap)
        {
            var reference = heap.GetRoot();
            if (reference.ReadTypeId() != ModelSchema.SchemaId) throw new InvalidCastException();
            return new(reference);
        }
        public static ServerRequestModel Deserialize(Stream stream)
        {
            var heap = ArrayPoolHeap.ReadFrom(stream);
            return Deserialize(heap);
        }

        public int RepeatCount => _reference.ReadInt32(ModelSchema.RepeatCountOffset);

        public bool IsEnabled => _reference.ReadBoolean(ModelSchema.IsEnabledOffset);

        public Utf8 Message => _reference.ReadUtf8(ModelSchema.MessageOffset);
    }

    // used on the server to compose responses
    public readonly struct ServerResponseModel
    {
        private readonly PoleReference _reference;
        private ServerResponseModel(PoleReference reference) => _reference = reference;

        public static ServerResponseModel Allocate(ArrayPoolHeap heap)
        {
            PoleReference reference = heap.Allocate(ModelSchema.Size);
            reference.WriteTypeId(ModelSchema.SchemaId);
            return new ServerResponseModel(reference);
        }

        public int RepeatCount
        {
            get => _reference.ReadInt32(ModelSchema.RepeatCountOffset);
            set => _reference.WriteInt32(ModelSchema.RepeatCountOffset, value);
        }
        public bool IsEnabled
        {
            get => _reference.ReadBoolean(ModelSchema.IsEnabledOffset);
            set => _reference.WriteBoolean(ModelSchema.IsEnabledOffset, value);
        }
        public Utf8 Message
        {
            get => _reference.ReadUtf8(ModelSchema.MessageOffset);
            set => _reference.WriteUtf8(ModelSchema.MessageOffset, value);
        }
    }
}

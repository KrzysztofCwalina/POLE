using Azure.Core.Pole;
using System;
using System.ComponentModel;
using System.IO;

namespace Azure.Core.Pole.TestModels
{
    internal struct ModelSchema
    {
        public const int RepeatCountOffset = 0;
        public const int IsEnabledOffset = 4;
        public const int MessageOffset = 5;
        public const int Size = 9;
    }

    // used on the client to compose requests, aka input model
    public class ClientRequestModel
    {
        public ClientRequestModel() { }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Serialize(Stream stream)
        {
            var heap = new PoleHeap(); // TODO: this should write directly to stream, not to in memory buffers
            var reference = heap.Allocate(ModelSchema.Size);
            reference.WriteInt32(ModelSchema.RepeatCountOffset, RepeatCount);
            reference.WriteBoolean(ModelSchema.IsEnabledOffset, IsEnabled);
            reference.WriteString(ModelSchema.MessageOffset, Message);
            heap.WriteTo(stream);
        }

        // TODO: should the fileds be on the heap? It would work for flat objects, but not if sub-objects are new'able.
        public int RepeatCount { get; set; }
        public bool IsEnabled { get; set; }
        public string Message { get; set; }
    }

    // used by the server to parse client requests
    public class ServerRequestModel
    {
        private readonly PoleReference _reference;
        private ServerRequestModel(PoleReference reference) => _reference = reference;

        public static ServerRequestModel Deserialize(PoleReference reference) => new(reference);

        public int RepeatCount => _reference.ReadInt32(ModelSchema.RepeatCountOffset);

        public bool IsEnabled => _reference.ReadBoolean(ModelSchema.IsEnabledOffset);

        public Utf8 Message => _reference.ReadUtf8(ModelSchema.MessageOffset);
    }

    // used on the server to compose responses
    public struct ServerResponseModel
    {
        private readonly PoleReference __reference;
        private ServerResponseModel(PoleReference reference) => __reference = reference;

        public static ServerResponseModel Allocate(PoleHeap heap) => new(heap.Allocate(ModelSchema.Size));

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
        public Utf8 Message
        {
            get => __reference.ReadUtf8(ModelSchema.MessageOffset);
            set => __reference.WriteUtf8(ModelSchema.MessageOffset, value);
        }
    }

    // used by the client to parse server responses, aka output model
    public class ClientResponseModel
    {
        private readonly PoleReference _reference;
        private ClientResponseModel(PoleReference reference) => _reference = reference;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ClientResponseModel Deserialize(PoleReference reference) => new(reference);

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
        public static ClientRountripingModel Deserialize(PoleReference reference) => new(reference);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Serialize(Stream stream)
        {
            var heap = new PoleHeap(); // TODO: this should write directly to stream, not to in memory buffers
            var reference = heap.Allocate(ModelSchema.Size);
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

    // used by the server to receive request from client, mutate payload, and send it back.
    public class ServerRountripingModel
    {
        private readonly PoleReference __reference;
        private string _message; // mutable variable size fields need to be on the GC heap
        private ServerRountripingModel(PoleReference reference) => __reference = reference;

        public static ServerRountripingModel Deserialize(PoleReference reference) => new(reference);
        public void Serialize(Stream stream)
        {
            var heap = new PoleHeap(); // TODO: this should write directly to stream, not to in memory buffers
            var reference = heap.Allocate(ModelSchema.Size);
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
}

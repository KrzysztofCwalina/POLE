//using Azure.Core.Pole;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.IO;

//namespace Azure.Core.Pole.TestModels
//{
//    internal struct ModelWithMapSchema
//    {
//        public const ulong IdH = 0xa177d25283a179b6;

//        public const int MapOffset = 16;
//        public const int Size = 20;
//    }

//    public struct ModelWithMap
//    {
//        private readonly PoleReference _reference;
//        private ModelWithMap(PoleReference reference) => _reference = reference;

//        [EditorBrowsable(EditorBrowsableState.Never)]
//        public static ModelWithMap Deserialize(PoleHeap heap)
//        {
//            var reference = heap.GetAt(0);
//            if (!reference.SchemaEquals(ModelWithMapSchema.IdL, ModelWithMapSchema.IdH)) throw new InvalidCastException();
//            return new (reference);
//        } 
//        [EditorBrowsable(EditorBrowsableState.Never)]
//        public static ModelWithMap Deserialize(Stream stream)
//        {
//            var heap = PoleHeap.ReadFrom(stream);
//            return Deserialize(heap);
//        }

//        public IReadOnlyDictionary<string, int> Map => _reference.ReadInt32(ModelWithMapSchema.MapOffset);
//    }
//}

//namespace Azure.Core.Pole.TestModels.Server
//{
//    public struct ModelWithMap
//    {
//        private readonly PoleReference _reference;
//        private ModelWithMap(PoleReference reference) => _reference = reference;

//        public static ModelWithMap Allocate(PoleHeap heap)
//        {
//            PoleReference reference = heap.Allocate(ModelWithMapSchema.Size);
//            reference.WriteSchemaId(ModelWithMapSchema.IdL, ModelWithMapSchema.IdH);
//            return new ModelWithMap(reference);
//        }

//        public IDictionary<string, int> Map
//        {
//            get => _reference.ReadInt32(ModelWithMapSchema.MapOffset);
//            set => _reference.WriteObject(ModelWithMapSchema.MapOffset, value);
//        }
//    }
//}

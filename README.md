# Portable Object Layout Encoding (POLE)

POLE is a format for encoding object graphs similar to JSON, but: 

- serialization and deserialization are close to zero-alloc, just write/read bytes into/from byte buffers
- serialization is close to zero work, i.e. POLE objects have no data fields that need to be serialized; their "fields" *are* in the output buffer
- deserialization is close to zero work, i.e. POLE payload is exatly the memory representation of the POLE object graph
- deserializer can read from a sequence of buffers, as opposed to a single contiquous buffer. This enables efficient transimssion without buffer resize-copy.
- deserialized object graphs can be all structs, i.e. no heap allocations, but the structs have natural (easy to use) reference semantics
- serialized payloads are typically much smaller than JSON

## Hello World

```csharp
using var stream = new MemoryStream();

// write to stream
{
    using PoleHeap heap = new PoleHeap(); // the heap rents buffers from a pool
    HelloModel hello = HelloModel.Allocate(heap); // this does not actually allocate anthing on the GC heap.
    hello.Message = Utf8.Allocate(heap, "Hello World!"); // this does not actually allocate anthing on the GC heap.
    hello.RepeatCount = 5;
    SetIsEnabled(hello, true); // hello is a struct (no alloc), but has reference semantics, e.g. can passed to methods that mutate

    heap.WriteTo(stream);

    // local method just to illustrate that POLE objects (structs) can be passed just like reference types
    void SetIsEnabled(HelloModel hello, bool value) => hello.IsEnabled = value;
} // heap buffers are returned to the buffer pool here

Assert.AreEqual(29, stream.Length);

// read from stream
{
    stream.Position = 0;
    using var heap = PoleHeap.ReadFrom(stream); // the heap rents buffers from a pool and reads the stream into the buffers

    HelloModel hello = HelloModel.Deserialize(heap); // this does not actually "deserialize", just stores an address in the struct 

    Assert.IsTrue(hello.IsEnabled); // this just dereferences a bool stored in the heap
    Assert.AreEqual(5, hello.RepeatCount); // same but with an int

    if (hello.IsEnabled)
    {
        for(int i=0; i<hello.RepeatCount; i++)
        {
            Assert.AreEqual("Hello World!", hello.Message.ToString());
        }
    }
}
```

## Model Design
```csharp
[StructLayout(LayoutKind.Explicit)]
public struct HelloModel : IObject
{
    const int RepeatCountOffset = 0;                              // int
    const int IsEnabledOffset = RepeatCountOffset + sizeof(int);  // bool
    const int MessageOffset = IsEnabledOffset + sizeof(byte);     // string
    const int Size = MessageOffset + sizeof(int);

    [FieldOffset(0)]
    readonly PoleReference _reference;

    public static HelloModel Allocate(PoleHeap heap) => new (heap.Allocate(HelloModel.Size));
    public static HelloModel Deserialize(PoleHeap heap) => new (heap.GetAt(0));

    PoleReference IObject.Reference => _reference;

    private HelloModel(PoleReference reference) => _reference = reference;

    public int RepeatCount
    {
        get => _reference.ReadInt32(RepeatCountOffset);
        set => _reference.WriteInt32(RepeatCountOffset, value);
    }

    public bool IsEnabled
    {
        get => _reference.ReadBoolean(IsEnabledOffset);
        set => _reference.WriteBoolean(IsEnabledOffset, value);
    }

    public Utf8 Message
    {
        get => _reference.ReadString(MessageOffset);
        set => _reference.WriteString(MessageOffset, value);
    }
}
```

## PoleReference
```csharp
[StructLayout(LayoutKind.Explicit)]
public readonly struct PoleReference
{
    [FieldOffset(0)]
    readonly int _address;

    [FieldOffset(8)]
    readonly PoleHeap _heap;

    public PoleReference(PoleHeap heap, int address) : this()
    {
        _heap = heap;
        _address = address;
    }

    internal int Address => _address;
    internal PoleHeap Heap => _heap;
        
    public int ReadInt32(int offset) => BinaryPrimitives.ReadInt32LittleEndian(_heap.GetBytes(_address + offset));
    public void WriteInt32(int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(_heap.GetBytes(_address + offset), value);
    
    public bool ReadBoolean(int offset) => _heap[_address + offset] != 0;
    public void WriteBoolean(int offset, bool value) => _heap[_address + offset] = value ? (byte)1 : (byte)0;
    
    public PoleReference ReadReference(int offset) => new PoleReference(_heap, BinaryPrimitives.ReadInt32LittleEndian(_heap.GetBytes(_address + offset)));
    public void WriteReference(int offset, PoleReference reference) => BinaryPrimitives.WriteInt32LittleEndian(_heap.GetBytes(_address + offset), reference._address);
    
    public void WriteObject<T>(int offset, T value) where T : IObject => WriteReference(offset, value.Reference);
    
    public Utf8 ReadString(int offset) => new Utf8(this.ReadReference(offset));
    public void WriteString(int offset, Utf8 value) => WriteObject<Utf8>(offset, value);
    
    public ReadOnlySpan<byte> ReadBytes(int offset)
    {
        var len = BinaryPrimitives.ReadInt32LittleEndian(_heap.GetBytes(_address + offset));
        return _heap.GetBytes(_address + sizeof(int), len);
    }
}
```

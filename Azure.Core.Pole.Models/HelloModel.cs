using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Azure.Core.Pole.TestModels
{
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
}

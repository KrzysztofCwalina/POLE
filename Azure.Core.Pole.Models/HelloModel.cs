using Azure.Core.Pole;
using System;

namespace Azure.Core.Pole.TestModels
{
public struct HelloModel
{
    private readonly PoleReference __reference;
    private HelloModel(PoleReference reference) => __reference = reference;

    const int __RepeatCountOffset = 0;
    const int __IsEnabledOffset = 4;
    const int __MessageOffset = 5;

    internal static HelloModel Deserialize(PoleReference reference) => new(reference);

    public int RepeatCount
    {
        get => __reference.ReadInt32(__RepeatCountOffset);
    }
    public bool IsEnabled
    {
        get => __reference.ReadBoolean(__IsEnabledOffset);
    }
    public string Message
    {
        get => __reference.ReadString(__MessageOffset);
    }
}
}

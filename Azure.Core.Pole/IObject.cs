// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;

namespace Azure.Core.Pole
{
    public interface IObject
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        PoleReference Reference { get; }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Pole.Tooling;
using System;

namespace Azure.Core.Pole.TestModels.Definitions
{
    [Serializable, Deserializable]
    public class HelloModel
    {
        public HelloModel() { }

        public int RepeatCount { get; }
        public string Message { get; set; }

        [Version(2)]
        public bool IsEnabled { get; }
    }
}

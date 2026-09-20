using System;

namespace InsanityLib.Extended.Traits;

[Flags]
public enum ETraitSystem
{
    None = 0,

    Vanilla = 1,

    DynamicTraits = 2,

    XLib = 4,

    All = Vanilla | DynamicTraits | XLib
}

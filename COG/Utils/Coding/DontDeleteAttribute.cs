﻿using System;

namespace COG.Utils.Coding;

/// <summary>
/// The attribute to describe a target which shouldn't be deleted due to something
/// For example, there is a unused class, but reflection needs it
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
public sealed class DontDeleteAttribute : Attribute
{
}
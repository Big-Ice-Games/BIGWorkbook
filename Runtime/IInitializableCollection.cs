#region license

// Copyright (c) 2025, Big Ice Games
// All rights reserved.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace BIGWorkbook.Runtime
{
  public interface IInitializableCollection
  {
    void Initialize(KeyValuePair<Type, IList> data);
    string OutputPath { get; }
  }
}

﻿using System;
using System.Collections.Generic;
using FubuFastPack.Querying;

namespace FubuFastPack.JqGrid
{
    public interface ISmartGridHarness
    {
        Type GridType { get; }
        IEnumerable<FilteredProperty> FilteredProperties();
        GridViewModel BuildGridModel(IEnumerable<IGridPolicy> gridPolicies);

        string GetQuerystring();
        void RegisterArguments(params object[] arguments);
        string HeaderText();
        int Count();
        Type EntityType();
    }
}
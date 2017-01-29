﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace At.Utilities
{

    //(From Roslyn) Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

    /// <summary>
    /// Compares string based upon their ordinal equality.
    /// We use this comparer for string identifiers because it does exactly what we need and nothing more
    /// The StringComparer.Ordinal as implemented by StringComparer is more complex to support 
    /// case sensitive and insensitive compares depending on flags.
    /// It also defers to the default string hash function that might not be the best for our scenarios.
    /// </summary>
    internal sealed class StringOrdinalComparer : IEqualityComparer<string>
    {
        public static readonly StringOrdinalComparer Instance = new StringOrdinalComparer();

        private StringOrdinalComparer()
        {
        }

        bool IEqualityComparer<string>.Equals(string a, string b)
        {
            return Equals(a, b);
        }

        public static bool Equals(string a, string b)
        {
            // this is fast enough
            return string.Equals(a, b);
        }

        int IEqualityComparer<string>.GetHashCode(string s)
        {
            // PERF: the default string hashcode is not always good or fast and cannot be changed for compat reasons.
            // We, however, can use anything we want in our dictionaries. 
            // Our typical scenario is a relatively short string (identifier)
            // FNV performs pretty well in such cases
            return Hash.GetFNVHashCode(s);
        }
    }

}
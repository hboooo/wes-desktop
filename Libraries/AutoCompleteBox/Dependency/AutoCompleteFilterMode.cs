using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Controls
{
    public delegate bool AutoCompleteFilterPredicate<T>(string search, T item);

    public enum AutoCompleteFilterMode
    {
        None,
        StartsWith,
        StartsWithCaseSensitive,
        StartsWithOrdinal,
        StartsWithOrdinalCaseSensitive,
        Contains,
        ContainsCaseSensitive,
        ContainsOrdinal,
        ContainsOrdinalCaseSensitive,
        Equals,
        EqualsCaseSensitive,
        EqualsOrdinal,
        EqualsOrdinalCaseSensitive,
        Custom
    }
}
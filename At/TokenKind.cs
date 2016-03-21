using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace At
{
    public enum TokenKind
    {
        Any = 0,
        StartOfFile,
        EndOfFile,
        EndOfLine,
        Space,
        AtSymbol,
        TokenCluster,
        SemiColon,
        LessThan,
        GreaterThan,
        StringLiteral,
        Colon,
        LeftBrace,
        RightBrace,
        Dot,
        DotDot,
        Ellipsis,
        Comma
    }
}

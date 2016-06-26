using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace At
{
public enum TokenKind
{
    None = 0,
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
    LeftParenthesis,
    RightBrace,
    RightParenthesis,
    Dot,
    DotDot,
    Ellipsis,
    Comma
}
}

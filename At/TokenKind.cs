using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace At
{
public struct TokenKind : IEquatable<TokenKind>
{
    static HashSet<int> values = new HashSet<int>();
    internal int value;
    
    //see http://source.roslyn.io/#Microsoft.CodeAnalysis.CSharp/Syntax/SyntaxKind.cs
    public static readonly TokenKind Other = 100;
    public static readonly TokenKind None = 0;
    public static readonly TokenKind StartOfFile = 1;
    public static readonly TokenKind EndOfFile = (int) KnownTokenKind.EndOfFile;
    public static readonly TokenKind EndOfLine = 2;
    public static readonly TokenKind TokenCluster = (int) KnownTokenKind.TokenCluster;

    public static readonly TokenKind Space = (int) ' ';
    public static readonly TokenKind AtSymbol= (int) '@';

    public static readonly TokenKind SemiColon= 8212; 
    public static readonly TokenKind LessThan=8215;
    public static readonly TokenKind GreaterThan=8217;
    public static readonly TokenKind NumericLiteral=(int) KnownTokenKind.NumericLiteral;
    public static readonly TokenKind StringLiteral= 8511;
    public static readonly TokenKind Colon=8211;
    public static readonly TokenKind OpenBrace= 8205;
    public static readonly TokenKind OpenParenthesis= 8200;
    public static readonly TokenKind CloseBrace= 8206;
    public static readonly TokenKind CloseParenthesis= 8201;
    public static readonly TokenKind OpenBracket = 8207;
    public static readonly TokenKind CloseBracket = 8208;
    public static readonly TokenKind Dot= 8218;
    public static readonly TokenKind DotDot= 4;
    public static readonly TokenKind Ellipsis= 5;
    public static readonly TokenKind Comma= 8216;

    public TokenKind(int value)
    {
        if (values.Contains(value))
            throw new ArgumentException($"\"{value}\" is already a token kind value",nameof(value));
        
        values.Add(value);
        this.value = value;
    }

    public bool Equals(TokenKind tk) => (tk.value == value);
    public override bool Equals(object obj) => obj is TokenKind && Equals((TokenKind)obj);
    public override int GetHashCode() => value.GetHashCode();
    public static bool operator==(TokenKind a, TokenKind b) => a.Equals(b);
    public static bool operator!=(TokenKind a, TokenKind b) => !a.Equals(b);
    public static implicit operator TokenKind(int value) => new TokenKind(value);
    //public static implicit operator int(TokenKind tk) => tk.value;
}

public enum KnownTokenKind
{
    None = 0,
    StartOfFile = 1,
    EndOfFile = 8496,
    TokenCluster = 3,
    NumericLiteral = 8509,
}
}

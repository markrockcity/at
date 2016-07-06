using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace At
{
public struct TokenKind : IEquatable<TokenKind>
{
    internal int value;

    static HashSet<int> values = new HashSet<int>();
    static Lazy<Dictionary<int,string>> names = new Lazy<Dictionary<int,string>>(()=>
    {
         var d = new Dictionary<int, string>();
         var t = typeof(TokenKind);
         foreach(var f in t.GetFields().Where(_=>_.FieldType==t))
            d.Add(((TokenKind)f.GetValue(null)).value,f.Name);
         return d;
    });
    
    //see http://source.roslyn.io/#Microsoft.CodeAnalysis.CSharp/Syntax/SyntaxKind.cs
    public static readonly TokenKind Unspecified = -1;
    public static readonly TokenKind Other = 100;
    public static readonly TokenKind None = 0;
    public static readonly TokenKind StartOfFile = 1;
    public static readonly TokenKind EndOfFile = 8496;//(int) KnownTokenKind.EndOfFile;
    public static readonly TokenKind EndOfLine = 4;//(int) KnownTokenKind.EndOfLine;
    public static readonly TokenKind TokenCluster = 3;//(int) KnownTokenKind.TokenCluster;

    public static readonly TokenKind Space =8540;// (int) KnownTokenKind.Space;
    public static readonly TokenKind AtSymbol= 5;

    public static readonly TokenKind SemiColon= 8212; 
    public static readonly TokenKind LessThan=8215;
    public static readonly TokenKind GreaterThan=8217;
    public static readonly TokenKind NumericLiteral= 8509; //(int) KnownTokenKind.NumericLiteral;
    public static readonly TokenKind StringLiteral= 8511;
    public static readonly TokenKind Colon=8211;
    public static readonly TokenKind OpenBrace= 8205;
    public static readonly TokenKind OpenParenthesis= 8200;
    public static readonly TokenKind CloseBrace= 8206;
    public static readonly TokenKind CloseParenthesis= 8201;
    public static readonly TokenKind OpenBracket = 8207;
    public static readonly TokenKind CloseBracket = 8208;
    public static readonly TokenKind Dot= 8218;
    public static readonly TokenKind DotDot= 6;
    public static readonly TokenKind Ellipsis= 7;
    public static readonly TokenKind Comma= 8216;


    public TokenKind(int value, string name = null)
    {
        if (values.Contains(value))
            throw new ArgumentException($"\"{value}\" is already a token kind value",nameof(value));
        
        values.Add(value);
        this.value = value;

        if (!string.IsNullOrWhiteSpace(name))
            names.Value.Add(value,name);
    }

    public string Name => name();

    //TODO: implement
    public bool IsBuiltIn(TokenKind kind) => true; 
    public bool Equals(TokenKind tk) => (tk.value == value);
    public override bool Equals(object obj) => obj is TokenKind && Equals((TokenKind)obj);
    public override int GetHashCode() => value.GetHashCode();
    public override string ToString() => $"{{TokenKind: {name()}}}";
    public static bool operator==(TokenKind a, TokenKind b) => a.Equals(b);
    public static bool operator!=(TokenKind a, TokenKind b) => !a.Equals(b);
    public static implicit operator TokenKind(int value) => new TokenKind(value);

    string name()
    {
        return (names.Value.ContainsKey(value))
                    ? names.Value[value]
                    : $"#{value}";
            
    }
}

/*
public enum KnownTokenKind
{
    None = 0,
    StartOfFile = 1,
    EndOfFile = 8496,
    TokenCluster = 3,
    NumericLiteral = 8509,
    Space = 8540,
    EndOfLine = 4,
}*/
}

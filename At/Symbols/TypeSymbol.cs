using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace At.Symbols
{
public class TypeSymbol : ContextSymbol
{
    /// <summary>"object"</summary>
    public static readonly TypeSymbol Top;
    /// <summary>"void"/"Nil"</summary>
    public static readonly TypeSymbol Unit;
    /// <summary>When the type is not known.</summary>
    public static readonly TypeSymbol Unknown;
    /// <summary>"Type" type, the base type for all metatypes</summary>
    public static readonly TypeSymbol TypeType;
    public static readonly TypeSymbol Class;
    public static readonly TypeSymbol Interface;
    public static readonly TypeSymbol String;
    public static readonly TypeSymbol Number;
    public static readonly TypeSymbol TypeParameter;

    static TypeSymbol()
    {
        Top       = new TypeSymbol("Top",null,null);
        TypeType  = new TypeSymbol("Type",Top,null);
        Unit      = new TypeSymbol("()",Top,TypeType);
        Unknown   = new TypeSymbol("?",Top,TypeType);
        Class     = new TypeSymbol("Class",TypeType,TypeType);
        TypeType.Type = Class;
        Top.Type = Class;
        Interface = new TypeSymbol("Interface",TypeType,TypeType);
        String    = new TypeSymbol("String",Top,Class);
        Number = new TypeSymbol("Number",Top,Class);
        TypeParameter = new TypeSymbol("TypeParameter",TypeType,TypeType);

    }

    
    protected internal TypeSymbol(string name, TypeSymbol baseType, TypeSymbol metaType, AtSyntaxNode syntaxNode= null, ContextSymbol parentContext= null) : base(name,syntaxNode,parentContext)
    {
        BaseType = baseType;
        Type = metaType;
    }

    public TypeSymbol BaseType
    {
        get;
        private set;
    }

    public bool IsUnknownType => this == Unknown;
    public bool IsTypeParameter => this is TypeParameterSymbol;

    /// <summary>The type's metatype (Class, Interface, etc.)</summary>
    public override TypeSymbol Type {get; protected internal set;}
        
    public static TypeSymbol For(System.Type type)
    {
        return type==typeof(string) ? String 
            :  type==typeof(double) ? Number
            :  throw new NotImplementedException($"{type}");
    }

}
}

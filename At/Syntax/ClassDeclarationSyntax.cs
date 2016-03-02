namespace At.Syntax
{
public class ClassDeclarationSyntax: DeclarationSyntax
{

    internal ClassDeclarationSyntax(string name, string declText) : base(name,declText) 
    {
        TypeParameterList = new TypeParameterListSytnax();
    }


    public string BaseClass
    {
        get;
        internal set;
    }

    public TypeParameterListSytnax TypeParameterList {get;}
}
}
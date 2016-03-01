namespace At.Syntax
{
    public class ClassDeclarationSyntax: DeclarationSyntax
    {
        public string BaseClass
        {
            get;
            internal set;
        }

        public TypeParameterListSytnax TypeParameterList
        {
            get;
            internal set;
        }
    }
}
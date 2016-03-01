using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace At.Syntax
{
public class CompilationUnitSyntax
{
        internal CompilationUnitSyntax(IEnumerable<ExpressionSyntax> exprs) 
        {
            this.Nodes = new ReadOnlyCollection<ExpressionSyntax>(exprs.ToList());
        }

        public IReadOnlyList<ExpressionSyntax> Nodes
        {
            get;
        }
}
}
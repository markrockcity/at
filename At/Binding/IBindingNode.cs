using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace At.Binding
{
//IOperation
public interface IBindingNode
{
    AtSyntaxNode Syntax {get;}
    TResult Accept<TResult>(BindingTreeVisitor<TResult> visitor);
}
}

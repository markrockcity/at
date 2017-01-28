using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace At.Binding
{
public interface IBindingNode
{
    AtSyntaxNode Syntax {get;}
    void Accept(BindingTreeVisitor visitor);
}
}

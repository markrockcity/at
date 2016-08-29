using System;
using System.Collections.Generic;
using System.Linq;
using At.Syntax;

namespace At 
{
//"{ ... }"
public abstract class BlockSyntax : ExpressionSyntax 
{

    protected BlockSyntax
    (
        AtToken startDelimiter, 
        IEnumerable<ExpressionSyntax> contents, 
        AtToken endDelimiter, 
        IExpressionSource expDef, 
        IEnumerable<AtDiagnostic> diagnostics) 
        
        : base(nodes(startDelimiter,contents,endDelimiter), expDef,diagnostics) {

        StartDelimiter = startDelimiter;
        Contents       = contents?.ToList().AsReadOnly() ?? new List<ExpressionSyntax>().AsReadOnly();
        EndDelimiter   = endDelimiter;
    }

    public AtToken StartDelimiter {get;}
    public IReadOnlyList<ExpressionSyntax> Contents {get;}
    public AtToken EndDelimiter {get;}

    static IEnumerable<AtSyntaxNode> nodes(AtSyntaxNode startDelimiter, IEnumerable<ExpressionSyntax> contents, AtToken endDelimiter)
    {
        var r = new AtSyntaxNode[] {startDelimiter}.AsEnumerable();
        if (contents != null)
            r = r.Concat(contents);
        return r.Concat(new[] {endDelimiter});
    }
}

//"( ... )"
public class RoundBlockSyntax : BlockSyntax 
{
    public RoundBlockSyntax(AtToken startDelimiter, IEnumerable<ExpressionSyntax> contents, AtToken endDelimiter, IExpressionSource expDef, IEnumerable<AtDiagnostic> diagnostics) 
     : base(startDelimiter,contents,endDelimiter,expDef,diagnostics) {}

    
}

//"< ... >"
public class PointyBlockSyntax : BlockSyntax 
{
    public PointyBlockSyntax(AtToken startDelimiter, IEnumerable<ExpressionSyntax> contents, AtToken endDelimiter, IExpressionSource expDef, IEnumerable<AtDiagnostic> diagnostics) 
     : base(startDelimiter,contents,endDelimiter,expDef,diagnostics) {}

    
}

//"[ ... ]"
public class SquareBlockSyntax : BlockSyntax 
{
    public SquareBlockSyntax(AtToken startDelimiter, IEnumerable<ExpressionSyntax> contents, AtToken endDelimiter, IExpressionSource expDef, IEnumerable<AtDiagnostic> diagnostics) 
     : base(startDelimiter,contents,endDelimiter,expDef,diagnostics) {}

    
}

//"{ ... }"
public class CurlyBlockSyntax : BlockSyntax 
{
    public CurlyBlockSyntax(AtToken startDelimiter, IEnumerable<ExpressionSyntax> contents, AtToken endDelimiter, IExpressionSource expDef, IEnumerable<AtDiagnostic> diagnostics) 
     : base(startDelimiter,contents,endDelimiter,expDef,diagnostics) {}

    
}
}
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
        
        : base(_nodes(startDelimiter,contents,endDelimiter), expDef,diagnostics) {

        StartDelimiter = startDelimiter;
        Content       = contents?.ToList().AsReadOnly() ?? new List<ExpressionSyntax>().AsReadOnly();
        EndDelimiter   = endDelimiter;
    }

    public AtToken StartDelimiter {get;}
    public IReadOnlyList<ExpressionSyntax> Content {get;}
    public AtToken EndDelimiter {get;}

    static IEnumerable<AtSyntaxNode> _nodes(AtSyntaxNode startDelimiter, IEnumerable<ExpressionSyntax> contents, AtToken endDelimiter)
    {
        var r = new AtSyntaxNode[] {startDelimiter}.AsEnumerable();
        if (contents != null)
            r = r.Concat(contents);
        return r.Concat(new[] {endDelimiter});
    }

    public override bool MatchesPattern(SyntaxPattern p, IDictionary<string,AtSyntaxNode> d = null)
    {
        var t =    (p.Text == PatternName || p.Text=="Block")
                && (p.Token1==null && p.Token2==null || p.Token1==StartDelimiter.Kind.Name && p.Token2==EndDelimiter.Kind.Name)
                && (p.Content==null || MatchesPatterns(p.Content,Content,d))
                
                || base.MatchesPattern(p,d);

        
        if (t && d != null && p.Key != null)
            d[p.Key] = this;

        return t;
    }

    public override IEnumerable<string> PatternStrings()
    {
        var name = PatternName;
        var cs = PatternStrings(Content).ToList();

        if (cs.Any())
        {
            foreach(var c in cs)
            {
                yield return $"{name}[{StartDelimiter.PatternName},{EndDelimiter.PatternName}]({c})";
                yield return $"{name}({c})";
            }
        }
        else
        {
            yield return $"{name}[{StartDelimiter.PatternName},{EndDelimiter.PatternName}]()";
            yield return $"{name}()";
        }

        yield return name;

        if (cs.Any())
        {
            foreach(var c in cs)
            {
                yield return $"Block[{StartDelimiter.PatternName},{EndDelimiter.PatternName}]({c})";
                yield return $"Block({c})";
            }
        }
        else
        {
            yield return "Block()";
        }

        yield return "Block";

        foreach(var x in base.PatternStrings())
            yield return x;
    }    

    public override string PatternName
    {
        get 
        {
            var name = base.PatternName;
            return name.EndsWith("Block") ? name.Substring(0,name.Length-5) : name;
        }
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
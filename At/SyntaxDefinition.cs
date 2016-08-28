using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace At
{
//Curly blocks, etc?
public interface ISyntaxDefinition
{
    /// <summary>Returns true if the syntax definition matches the input up to 
    /// the given k-lookahead.</summary>
    bool MatchesUpTo(IScanner<AtToken> tokens, int k);
    AtSyntaxNode CreateSyntaxNode(Scanner<AtToken> tokens);
}
}

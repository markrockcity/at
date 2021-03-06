﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace At.Syntax
{
//non-expression list (e.g., parameters, arguments, etc.)
public class ListSyntax<T> : ContextSyntax where T : AtSyntaxNode
{
    private AtToken startDelimiter;
    private AtToken endDelimiter;

    internal ListSyntax(AtToken startDelimiter, SeparatedSyntaxList<T> list, AtToken endDelimiter, IEnumerable<AtDiagnostic> diagnostics) : 
    base(new AtSyntaxNode[] {startDelimiter}.Concat(list._list).Concat(new[] {endDelimiter}),diagnostics)
    {
        this.startDelimiter = startDelimiter;
        this.endDelimiter = endDelimiter;
        List = list ?? throw new ArgumentNullException(nameof(list));
    }

    protected ListSyntax(IEnumerable<AtSyntaxNode> nodes,IEnumerable<AtDiagnostic> diagnostics,bool isMissing = false) : base(nodes,diagnostics,isMissing)
    {
    }

    /// <summary>Includes list item delimiters</summary>
    public SeparatedSyntaxList<T> List {get;}

    public override string FullText
    {
        get
        {
            return startDelimiter?.FullText+List+endDelimiter?.FullText;
        }
    }

    public override TResult Accept<TResult>(AtSyntaxVisitor<TResult> visitor)
    {
        throw new NotImplementedException();
    }
}
}
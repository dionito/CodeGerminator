// <copyright company="Dionito">
//    Copyright (c) Dionito Software 2022.
// 
//    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
//    documentation files (the "Software"), to deal iin the Software without restriction, including without
//    limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
//    Software, and to permit persons to whom the Software is furnished to do so, subject to the following
//    conditions:
// 
//    The above copyright notice and this permission notice shall be included in all copies or substantial portions
//    of the Software.
// 
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
//    TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
//    THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
//    CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
//    DEALINGS IN THE SOFTWARE.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dionito.CodeGerminator;

public abstract class CodeGerminatorBase : ISourceGenerator
{
    public virtual void Execute(GeneratorExecutionContext context) => throw new System.NotImplementedException();

    /// <summary>
    /// Determines the namespace in which a <see cref="SyntaxNode"/> is declared.
    /// </summary>
    /// <param name="syntaxNode">A syntax node direct descendant of a namespace.</param>
    /// <returns>
    /// The namespace name as string or null if none.
    /// </returns>
    /// <remarks>
    /// Supports no namespace declaration, file scoped namespaces, as well as single and
    /// multiple namespaces in file.
    /// </remarks>
    protected virtual string GetNodeNamespaceName(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not (ClassDeclarationSyntax or EnumDeclarationSyntax))
        {
            throw new ArgumentException(
                $"{nameof(syntaxNode)} must be of type {nameof(ClassDeclarationSyntax)} or {nameof(ClassDeclarationSyntax)}",
                nameof(syntaxNode));
        }

        IList<SyntaxNode> children = syntaxNode.SyntaxTree.GetRoot().ChildNodes().ToList();
        SyntaxNode parentNode =
            children.SingleOrDefault(n => n.ChildNodes().Any(childNode => childNode.Equals(syntaxNode)));

        return parentNode switch
        {
            NamespaceDeclarationSyntax namespaceDeclaration => namespaceDeclaration.Name.ToString(),
            FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclaration => fileScopedNamespaceDeclaration
                .Name.ToString(),
            _ => null,
        };
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // uncomment below to debug on rebuild
//#if DEBUG
//        if (!Debugger.IsAttached)
//        {
//            Debugger.Launch();
//        }
//#endif

        this.ClassInitialize(context);
    }

    protected abstract void ClassInitialize(GeneratorInitializationContext context);
}
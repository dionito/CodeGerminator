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
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Dionito.CodeGerminator.FastEnumToString
{
    [Generator]
    public class FastEnumToStringGenerator : CodeGerminatorBase
    {
        const string attributeSourceCode = @"// <auto-generated />

using System;

[AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
public sealed class FastEnumToStringAttribute : Attribute
{
}";

        static void AppendEnumMember(string candidateEnumName, SyntaxNode syntaxNode, StringBuilder sourceBuilder)
        {
            if (syntaxNode is EnumMemberDeclarationSyntax fieldDeclarationSyntax)
            {
                string field = $"{candidateEnumName}.{fieldDeclarationSyntax.Identifier}";
                sourceBuilder.AppendLine($"            {field} => nameof({field}),");
            }
        }

        public override void Execute(GeneratorExecutionContext context)
        {
            context.AddSource(
                "FastEnumToStringAttribute",
                SourceText.From(attributeSourceCode, Encoding.UTF8));

            if (context.SyntaxReceiver is not FastEnumToStringSyntaxReceiver receiver)
            {
                return;
            }

            foreach (EnumDeclarationSyntax candidateEnum in receiver.CandidateEnums)
            {
                string enumNamespace = this.GetNodeNamespaceName(syntaxNode: candidateEnum);
                string enumName = candidateEnum.Identifier.ToString();
                string className = $"{enumName}Extensions";

                StringBuilder sourceBuilder = GenerateExtensionClass(
                    enumNamespace,
                    className,
                    enumName,
                    candidateEnum);

                context.AddSource(
                    className,
                    $"{Environment.NewLine}{sourceBuilder}");
            }
        }

        static StringBuilder GenerateExtensionClass(
            string nameSpace,
            string className,
            string enumName,
            EnumDeclarationSyntax enumDeclaration)
        {
            if (string.IsNullOrEmpty(className) || string.IsNullOrEmpty(enumName) || enumDeclaration == null)
            {
                return new StringBuilder($"// {enumName} not supported.");
            }

            var sourceBuilder = new StringBuilder();
            sourceBuilder.AppendLine("// <auto-generated />");
            sourceBuilder.AppendLine("using System;");

            // in case namespace is empty, to avoid adding empty using
            if (!string.IsNullOrEmpty(nameSpace))
            {
                sourceBuilder.AppendLine($"using {nameSpace};");
            }

            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine($"public static class {className}");
            sourceBuilder.AppendLine("{");
            sourceBuilder.AppendLine($"    public static string FastEnumToString(this {enumName} value)");
            sourceBuilder.AppendLine("    {");
            sourceBuilder.AppendLine("        return value switch");
            sourceBuilder.AppendLine("        {");

            foreach (SyntaxNode syntaxNode in enumDeclaration.ChildNodes())
            {
                AppendEnumMember(enumName, syntaxNode, sourceBuilder);
            }

            sourceBuilder.AppendLine(
                "            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),");
            sourceBuilder.AppendLine("        };");
            sourceBuilder.AppendLine("    }");
            sourceBuilder.AppendLine("}");
            return sourceBuilder;
        }

        protected override void ClassInitialize (GeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new FastEnumToStringSyntaxReceiver());
        }
    }
}
// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.DiagnosticAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class YieldStatementDiagnosticAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.ReplaceReturnStatementWithExpressionStatement,
                    DiagnosticDescriptors.ReplaceReturnStatementWithExpressionStatementFadeOut);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.RegisterSyntaxNodeAction(f => AnalyzeReturnStatement(f), SyntaxKind.YieldReturnStatement);
        }

        private void AnalyzeReturnStatement(SyntaxNodeAnalysisContext context)
        {
            if (GeneratedCodeAnalyzer?.IsGeneratedCode(context) == true)
                return;

            var yieldStatement = (YieldStatementSyntax)context.Node;

            if (ReplaceReturnStatementWithExpressionStatementRefactoring.CanRefactor(yieldStatement, context.SemanticModel, context.CancellationToken)
                && !yieldStatement.ContainsDirectives(TextSpan.FromBounds(yieldStatement.YieldKeyword.Span.End, yieldStatement.Expression.Span.Start)))
            {
                context.ReportDiagnostic(DiagnosticDescriptors.ReplaceReturnStatementWithExpressionStatement, yieldStatement.GetLocation(), "yield");

                context.FadeOutToken(DiagnosticDescriptors.ReplaceReturnStatementWithExpressionStatementFadeOut, yieldStatement.YieldKeyword);
                context.FadeOutToken(DiagnosticDescriptors.ReplaceReturnStatementWithExpressionStatementFadeOut, yieldStatement.ReturnOrBreakKeyword);
            }
        }
    }
}

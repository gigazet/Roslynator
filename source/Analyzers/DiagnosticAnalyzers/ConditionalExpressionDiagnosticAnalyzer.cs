﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.DiagnosticAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConditionalExpressionDiagnosticAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.WrapConditionalExpressionConditionInParentheses,
                    DiagnosticDescriptors.ReplaceConditionalExpressionWithCoalesceExpression,
                    DiagnosticDescriptors.SimplifyConditionalExpression);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.RegisterSyntaxNodeAction(f => AnalyzeConditionalExpression(f), SyntaxKind.ConditionalExpression);
        }

        private void AnalyzeConditionalExpression(SyntaxNodeAnalysisContext context)
        {
            if (GeneratedCodeAnalyzer?.IsGeneratedCode(context) == true)
                return;

            var conditionalExpression = (ConditionalExpressionSyntax)context.Node;

            if (conditionalExpression.Condition?.IsKind(SyntaxKind.ParenthesizedExpression) == false)
            {
                context.ReportDiagnostic(
                    DiagnosticDescriptors.WrapConditionalExpressionConditionInParentheses,
                    conditionalExpression.Condition.GetLocation());
            }

            if (conditionalExpression.Condition?.IsMissing == false
                && CanBeConvertedToCoalesceExpression(conditionalExpression)
                && conditionalExpression
                    .DescendantTrivia(conditionalExpression.Span)
                    .All(f => f.IsWhitespaceOrEndOfLineTrivia()))
            {
                context.ReportDiagnostic(
                    DiagnosticDescriptors.ReplaceConditionalExpressionWithCoalesceExpression,
                    conditionalExpression.GetLocation());
            }

            if (SimplifyConditionalExpressionRefactoring.CanRefactor(conditionalExpression, context.SemanticModel, context.CancellationToken)
                && !conditionalExpression.SpanContainsDirectives())
            {
                context.ReportDiagnostic(
                    DiagnosticDescriptors.SimplifyConditionalExpression,
                    conditionalExpression.GetLocation());
            }
        }

        private static bool CanBeConvertedToCoalesceExpression(ConditionalExpressionSyntax conditionalExpression)
        {
            ExpressionSyntax condition = conditionalExpression.Condition.UnwrapParentheses();

            if (condition.IsKind(SyntaxKind.EqualsExpression))
            {
                var binaryExpression = (BinaryExpressionSyntax)condition;

                if (binaryExpression.Left?.IsMissing == false
                    && binaryExpression.Right?.IsKind(SyntaxKind.NullLiteralExpression) == true)
                {
                    return binaryExpression.Left.IsEquivalentTo(
                        conditionalExpression.WhenFalse.UnwrapParentheses(),
                        topLevel: false);
                }
            }
            else if (condition.IsKind(SyntaxKind.NotEqualsExpression))
            {
                var binaryExpression = (BinaryExpressionSyntax)condition;

                if (binaryExpression.Left?.IsMissing == false
                    && binaryExpression.Right?.IsKind(SyntaxKind.NullLiteralExpression) == true)
                {
                    return binaryExpression.Left.IsEquivalentTo(
                        conditionalExpression.WhenTrue.UnwrapParentheses(),
                        topLevel: false);
                }
            }

            return false;
        }
    }
}

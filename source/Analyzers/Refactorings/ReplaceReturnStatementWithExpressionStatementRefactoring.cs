// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class ReplaceReturnStatementWithExpressionStatementRefactoring
    {
        public static bool CanRefactor(ReturnStatementSyntax returnStatement, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            ExpressionSyntax expression = returnStatement.Expression;

            return expression?.IsMissing == false
                && semanticModel
                    .GetTypeSymbol(expression, cancellationToken)?
                    .IsVoid() == true;
        }

        public static bool CanRefactor(YieldStatementSyntax yieldStatement, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (yieldStatement.IsYieldReturn())
            {
                ExpressionSyntax expression = yieldStatement.Expression;

                return expression?.IsMissing == false
                    && semanticModel
                        .GetTypeSymbol(expression, cancellationToken)?
                        .IsVoid() == true;
            }

            return false;
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            ReturnStatementSyntax returnStatement,
            CancellationToken cancellationToken)
        {
            ExpressionStatementSyntax newNode = ExpressionStatement(returnStatement.Expression)
                .WithTriviaFrom(returnStatement)
                .WithFormatterAnnotation();

            return await document.ReplaceNodeAsync(returnStatement, newNode, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            YieldStatementSyntax yieldStatement,
            CancellationToken cancellationToken)
        {
            ExpressionStatementSyntax newNode = ExpressionStatement(yieldStatement.Expression)
                .WithTriviaFrom(yieldStatement)
                .WithFormatterAnnotation();

            return await document.ReplaceNodeAsync(yieldStatement, newNode, cancellationToken).ConfigureAwait(false);
        }
    }
}
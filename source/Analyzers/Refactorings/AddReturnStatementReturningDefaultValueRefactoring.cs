// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class AddReturnStatementReturningDefaultValueRefactoring
    {
        public static bool CanRefactor(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (methodDeclaration.Body != null)
            {
                TypeSyntax returnType = methodDeclaration.ReturnType;

                if (returnType?.IsMissing == false)
                {
                    ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(returnType, cancellationToken);

                    if (typeSymbol?.IsErrorType() == false
                        && !typeSymbol.IsIEnumerableOrConstructedFromIEnumerableOfT()
                        && !typeSymbol.IsVoid())
                    {
                        ImmutableArray<Diagnostic> diagnostics = semanticModel.GetDiagnostics(methodDeclaration.Identifier.Span, cancellationToken);

                        foreach (Diagnostic diagnostic in diagnostics)
                        {
                            switch (diagnostic.Id)
                            {
                                case CSharpErrorCodes.NotAllCodePathsReturnValue:
                                    return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            MethodDeclarationSyntax methodDeclaration,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(methodDeclaration.ReturnType, cancellationToken);

            MethodDeclarationSyntax newNode = methodDeclaration
                .AddBodyStatements(ReturnStatement(SyntaxUtility.CreateDefaultValue(typeSymbol)));

            return await document.ReplaceNodeAsync(methodDeclaration, newNode, cancellationToken).ConfigureAwait(false);
        }
    }
}
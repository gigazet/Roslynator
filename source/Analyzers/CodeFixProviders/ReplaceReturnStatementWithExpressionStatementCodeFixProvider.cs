// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.CodeFixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReplaceReturnStatementWithExpressionStatementCodeFixProvider))]
    [Shared]
    public class ReplaceReturnStatementWithExpressionStatementCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.ReplaceReturnStatementWithExpressionStatement); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            StatementSyntax statement = root
                .FindNode(context.Span, getInnermostNodeForTie: true)?
                .FirstAncestorOrSelf<StatementSyntax>();

            Debug.Assert(statement != null, $"{nameof(statement)} is null");

            if (statement == null)
                return;

            switch (statement.Kind())
            {
                case SyntaxKind.ReturnStatement:
                    {
                        RegisterCodeFix(
                            context,
                            "Remove 'return'",
                            cancellationToken => ReplaceReturnStatementWithExpressionStatementRefactoring.RefactorAsync(context.Document, (ReturnStatementSyntax)statement, cancellationToken));

                        break;
                    }
                case SyntaxKind.YieldReturnStatement:
                    {
                        RegisterCodeFix(
                            context,
                            "Remove 'yield return'",
                            cancellationToken => ReplaceReturnStatementWithExpressionStatementRefactoring.RefactorAsync(context.Document, (YieldStatementSyntax)statement, cancellationToken));

                        break;
                    }
                default:
                    {
                        Debug.Assert(false, statement.Kind().ToString());
                        break;
                    }
            }
        }

        private static void RegisterCodeFix(CodeFixContext context, string title, Func<CancellationToken, Task<Document>> createChangedDocument)
        {
            CodeAction codeAction = CodeAction.Create(
                title,
                createChangedDocument,
                DiagnosticIdentifiers.ReplaceReturnStatementWithExpressionStatement + EquivalenceKeySuffix);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }
    }
}
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace PhoenixToolkits.CodeAnalysis
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PTCA001MakeTestMethodWithoutAsyncSuffixCodeFixProvider)), Shared]
	public class PTCA001MakeTestMethodWithoutAsyncSuffixCodeFixProvider : CodeFixProvider
	{
		public override sealed ImmutableArray<string> FixableDiagnosticIds
			=> ImmutableArray.Create(PTCA001MakeTestMethodWithoutAsyncSuffixAnalyzer.DiagnosticId);

		public override sealed FixAllProvider GetFixAllProvider()
			=> WellKnownFixAllProviders.BatchFixer;

		public override sealed async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document
				.GetSyntaxRootAsync(context.CancellationToken)
				.ConfigureAwait(false);

			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			// Find the type declaration identified by the diagnostic.
			var declaration = root.FindToken(diagnosticSpan.Start)
				.Parent
				.AncestorsAndSelf()
				.OfType<MethodDeclarationSyntax>()
				.First();

			// Register a code action that will invoke the fix.
			context.RegisterCodeFix(
				CodeAction.Create(
					title: CodeFixResources.PTCA001CodeFixTitle,
					createChangedSolution: c => MakeTrimEndAsyncSuffixAsync(context.Document, declaration, c),
					equivalenceKey: nameof(CodeFixResources.PTCA001CodeFixTitle)),
				diagnostic);
		}

		private async Task<Solution> MakeTrimEndAsyncSuffixAsync(
			Document document,
			MethodDeclarationSyntax methodDecl,
			CancellationToken cancellationToken)
		{
			var identifierToken = methodDecl.Identifier;
			var methodName = identifierToken.Text;
			var newName = methodName.AsSpan()
				.Slice(0, methodName.Length - "Async".Length)
				.ToString();

			var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
			var methodSymbol = semanticModel.GetDeclaredSymbol(methodDecl, cancellationToken);

			var renameOptions = new SymbolRenameOptions(true, false, true, false);
			var newSolution = await Renamer.RenameSymbolAsync(
				document.Project.Solution,
				methodSymbol,
				renameOptions,
				newName,
				cancellationToken).ConfigureAwait(false);

			return newSolution;
		}
	}
}

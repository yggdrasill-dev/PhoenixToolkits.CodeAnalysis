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
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Rename;

namespace PhoenixToolkits.CodeAnalysis
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CS4033MakeTestMethodWithoutAsyncSuffixCodeFixProvider)), Shared]
	public class CS4033MakeTestMethodWithoutAsyncSuffixCodeFixProvider : CodeFixProvider
	{
		public const string DiagnosticId = "CS4033";

		private static readonly ImmutableHashSet<string> _UnitTestAttributeNames = ImmutableHashSet.Create(
			"TestMethod",
			"Test",
			"Fact",
			"Theory");

		public override sealed ImmutableArray<string> FixableDiagnosticIds
			=> ImmutableArray.Create(DiagnosticId, "CS4032");

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
			var declaration = root.FindToken(diagnosticSpan.Start);

			var methodDecl = FindMethod(declaration.Parent);

			if (!methodDecl.AttributeLists
				.SelectMany(al => al.Attributes)
				.Any(attr => _UnitTestAttributeNames.Contains(
					attr.Name is QualifiedNameSyntax q
						? q.Right.ToString()
						: attr.Name is IdentifierNameSyntax id
							? id.ToString()
							: attr.ToString())))
				return;

			// Register a code action that will invoke the fix.
			context.RegisterCodeFix(
				CodeAction.Create(
					title: CodeFixResources.PTCA002CodeFixTitle,
					createChangedSolution: c => MakeTestMethodAsynchronousAsync(
						context.Document,
						methodDecl,
						c),
					equivalenceKey: nameof(CodeFixResources.PTCA001CodeFixTitle)),
				diagnostic);
		}

		private MethodDeclarationSyntax FindMethod(SyntaxNode node)
			=> node is MethodDeclarationSyntax syntax ? syntax : FindMethod(node.Parent);

		private async Task<Solution> MakeTestMethodAsynchronousAsync(
			Document document,
			MethodDeclarationSyntax methodDecl,
			CancellationToken cancellationToken)
		{
			var identifierToken = methodDecl.Identifier;
			var methodName = identifierToken.Text;

			var firstToken = methodDecl.GetFirstToken();
			var leadingTrivia = firstToken.LeadingTrivia;
			var trimmedMethod = methodDecl.ReplaceToken(firstToken, firstToken.WithLeadingTrivia(SyntaxTriviaList.Empty));

			var asyncToken = SyntaxFactory.Token(SyntaxKind.AsyncKeyword);

			var newModifier = trimmedMethod.Modifiers.Add(asyncToken);

			var newReturnType = SyntaxFactory.ParseTypeName("Task");

			var newMethod = trimmedMethod
				.WithLeadingTrivia(leadingTrivia)
				.WithModifiers(newModifier)
				.WithReturnType(newReturnType)
				.WithIdentifier(methodDecl.Identifier);

			// Add an annotation to format the new local declaration.
			var formattedMethod = newMethod.WithAdditionalAnnotations(Formatter.Annotation);

			// Replace the old local declaration with the new local declaration.
			var oldRoot = await document
				.GetSyntaxRootAsync(cancellationToken)
				.ConfigureAwait(false);
			var newRoot = oldRoot.ReplaceNode(methodDecl, formattedMethod);

			// Return document with transformed tree.
			var newDocument = document.WithSyntaxRoot(newRoot);

			var newName = methodName;

			if (methodName.EndsWith("Async", StringComparison.OrdinalIgnoreCase))
				newName = methodName.AsSpan()
					.Slice(0, methodName.Length - "Async".Length)
					.ToString();

			var semanticModel = await newDocument.GetSemanticModelAsync(cancellationToken);
			var findMethod = (await newDocument.GetSyntaxRootAsync(cancellationToken)
				.ConfigureAwait(false))
				.FindToken(methodDecl.SpanStart)
				.Parent
				.AncestorsAndSelf()
				.OfType<MethodDeclarationSyntax>()
				.First();
			var methodSymbol = semanticModel.GetDeclaredSymbol(findMethod, cancellationToken);

			var renameOptions = new SymbolRenameOptions(true, false, true, false);
			var newSolution = await Renamer.RenameSymbolAsync(
				newDocument.Project.Solution,
				methodSymbol,
				renameOptions,
				newName,
				cancellationToken).ConfigureAwait(false);

			return newSolution;
		}
	}
}

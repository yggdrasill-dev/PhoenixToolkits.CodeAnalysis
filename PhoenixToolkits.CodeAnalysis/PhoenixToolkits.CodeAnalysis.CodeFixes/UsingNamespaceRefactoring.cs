using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PhoenixToolkits.CodeAnalysis
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(UsingNamespaceRefactoring)), Shared]
	internal class UsingNamespaceRefactoring : CodeRefactoringProvider
	{
		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(
				context.CancellationToken).ConfigureAwait(false);

			var node = root.FindNode(context.Span);
			var semanticModel = await context.Document.GetSemanticModelAsync(
				context.CancellationToken).ConfigureAwait(false);

			if (semanticModel is null || !(root is CompilationUnitSyntax compilation))
				return;

			if (node.Parent is null)
				return;

			var symbolInfo = semanticModel.GetSymbolInfo(node);

			if (symbolInfo.Symbol is null || symbolInfo.Symbol.Kind != SymbolKind.NamedType)
				return;

			if (node.Parent is QualifiedNameSyntax qualifiedName)
			{
				var symbolNamespace = symbolInfo.Symbol.ContainingNamespace.ToDisplayString();

				if (!compilation.Usings.Any(s => s.Name.GetText().ToString() == symbolNamespace))
					context.RegisterRefactoring(CodeAction.Create(
						title: string.Format(CodeFixResources.UsingNamespaceRefactoringTitle, symbolNamespace),
						createChangedDocument: ct => AddUsingNamespaceAsync(
							context.Document,
							node,
							qualifiedName,
							SyntaxFactory.ParseName(symbolNamespace),
							ct)));
			}

			if (node.Parent is MemberAccessExpressionSyntax memberAccessExpression
				&& (memberAccessExpression.Expression is IdentifierNameSyntax
					|| memberAccessExpression.Expression is MemberAccessExpressionSyntax))
			{
				var expressionSymbolInfo = semanticModel.GetSymbolInfo(memberAccessExpression.Expression);

				if (expressionSymbolInfo.Symbol is null || expressionSymbolInfo.Symbol.Kind != SymbolKind.Namespace)
					return;

				var symbolNamespace = symbolInfo.Symbol.ContainingNamespace.ToDisplayString();

				if (!compilation.Usings.Any(s => s.Name.GetText().ToString() == symbolNamespace))
					context.RegisterRefactoring(CodeAction.Create(
						title: string.Format(CodeFixResources.UsingNamespaceRefactoringTitle, symbolNamespace),
						createChangedDocument: ct => AddUsingNamespaceAsync(
							context.Document,
							node,
							memberAccessExpression,
							SyntaxFactory.ParseName(symbolNamespace),
							ct)));
			}
		}

		private async Task<Document> AddUsingNamespaceAsync(
			Document document,
			SyntaxNode currentNode,
			SyntaxNode namespaceNode,
			NameSyntax namespaceSyntax,
			CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(
				cancellationToken).ConfigureAwait(false);
			var compilation = (CompilationUnitSyntax)root.ReplaceNode(
				namespaceNode,
				currentNode.WithLeadingTrivia(namespaceNode.GetLeadingTrivia()));

			var usingStatement = SyntaxFactory.UsingDirective(namespaceSyntax);

			return document.WithSyntaxRoot(compilation.AddUsings(usingStatement));
		}
	}
}

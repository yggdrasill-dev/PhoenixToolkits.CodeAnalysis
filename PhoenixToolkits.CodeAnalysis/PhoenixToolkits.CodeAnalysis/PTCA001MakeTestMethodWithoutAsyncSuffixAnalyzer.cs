using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace PhoenixToolkits.CodeAnalysis
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class PTCA001MakeTestMethodWithoutAsyncSuffixAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "PTCA001";

		private const string Category = "Naming";

		private static readonly ImmutableHashSet<string> _UnitTestAttributeNames = ImmutableHashSet.Create(
			"TestMethod",
			"Test",
			"Fact");

		// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
		// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
		private static readonly LocalizableString _Title = new LocalizableResourceString(
			nameof(Resources.PTCA001AnalyzerTitle),
			Resources.ResourceManager,
			typeof(Resources));

		private static readonly LocalizableString _MessageFormat = new LocalizableResourceString(
			nameof(Resources.PTCA001AnalyzerMessageFormat),
			Resources.ResourceManager,
			typeof(Resources));

		private static readonly LocalizableString _Description = new LocalizableResourceString(
			nameof(Resources.PTCA001AnalyzerDescription),
			Resources.ResourceManager,
			typeof(Resources));

		private static readonly DiagnosticDescriptor _Rule = new DiagnosticDescriptor(
			DiagnosticId,
			_Title,
			_MessageFormat,
			Category,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: _Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(_Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterSyntaxNodeAction(AnalyzeTestMethodName, SyntaxKind.MethodDeclaration);
		}

		private void AnalyzeTestMethodName(SyntaxNodeAnalysisContext context)
		{
			var methodDeclaration = (MethodDeclarationSyntax)context.Node;

			var methodName = methodDeclaration.Identifier.Text;

			if (!(methodDeclaration.ReturnType is QualifiedNameSyntax qualifiedName
				&& qualifiedName.Right.ToString() == "Task"
				|| methodDeclaration.ReturnType is IdentifierNameSyntax identifierName
				&& identifierName.ToString() == "Task"))
				return;

			if (!methodName.EndsWith("Async", System.StringComparison.OrdinalIgnoreCase))
				return;

			if (methodDeclaration.Modifiers.Any(token => token.Text == "static"))
				return;

			if (!methodDeclaration.AttributeLists
				.SelectMany(al => al.Attributes)
				.Any(attr => _UnitTestAttributeNames.Contains(
					attr.Name is QualifiedNameSyntax q
						? q.Right.ToString()
						: attr.Name is IdentifierNameSyntax id
							? id.ToString()
							: attr.ToString())))
				return;

			context.ReportDiagnostic(Diagnostic.Create(
				_Rule,
				Location.Create(
					methodDeclaration.Identifier.SyntaxTree,
					TextSpan.FromBounds(
						methodDeclaration.Identifier.Span.End - methodName.Length,
						methodDeclaration.Identifier.Span.End)),
				methodName));
		}
	}
}

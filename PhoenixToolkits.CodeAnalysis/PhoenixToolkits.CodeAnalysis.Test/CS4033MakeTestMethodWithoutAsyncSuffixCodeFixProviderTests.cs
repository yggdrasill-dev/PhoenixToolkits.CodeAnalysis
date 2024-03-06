using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = PhoenixToolkits.CodeAnalysis.Test.CSharpCodeFixVerifier<
	PhoenixToolkits.CodeAnalysis.PTCA001MakeTestMethodWithoutAsyncSuffixAnalyzer,
	PhoenixToolkits.CodeAnalysis.CS4033MakeTestMethodWithoutAsyncSuffixCodeFixProvider>;

namespace PhoenixToolkits.CodeAnalysis.Test;

[TestClass]
public class CS4033MakeTestMethodWithoutAsyncSuffixCodeFixProviderTests
{
	[TestMethod]
	public async Task Await_Diagnostic_NUnit()
	{
		var regex = new Regex(@"^\t{4}", RegexOptions.Multiline);

		var code = regex.Replace(
			"""
				using System;
				using System.Threading.Tasks;

				class Program
				{
					[Test]
					void TestMethod1()
					{
						[|await Task.CompletedTask|];
					}
				}
				""",
			string.Empty).Replace("\t", "    ");

		var expected = regex.Replace(
			"""
				using System;
				using System.Threading.Tasks;

				class Program
				{
					[Test]
					async Task TestMethod1()
					{
						await Task.CompletedTask;
					}
				}
				""", string.Empty).Replace("\t", "    ");

		await VerifyCS.VerifyCodeFixWithoutAnalyzerAsync(
			code,
			[
				DiagnosticResult.CompilerError("CS0246")
					.WithSpan(6, 6, 6, 10)
					.WithArguments("Test"),
				DiagnosticResult.CompilerError("CS0246")
					.WithSpan(6, 6, 6, 10)
					.WithArguments("TestAttribute")
			],
			expected);
	}

	[TestMethod]
	public async Task Await_Diagnostic_MsTest()
	{
		var regex = new Regex(@"^\t{4}", RegexOptions.Multiline);

		var code = regex.Replace(
			"""
				using System;
				using System.Threading.Tasks;

				class Program
				{
					[TestMethod]
					void TestMethod1()
					{
						[|await Task.CompletedTask|];
					}
				}
				""",
			string.Empty).Replace("\t", "    ");

		var expected = regex.Replace(
			"""
				using System;
				using System.Threading.Tasks;

				class Program
				{
					[TestMethod]
					async Task TestMethod1()
					{
						await Task.CompletedTask;
					}
				}
				""",
			string.Empty).Replace("\t", "    ");

		await VerifyCS.VerifyCodeFixWithoutAnalyzerAsync(
			code,
			[
				DiagnosticResult.CompilerError("CS0246")
					.WithSpan(6, 6, 6, 16)
					.WithArguments("TestMethod"),
				DiagnosticResult.CompilerError("CS0246")
					.WithSpan(6, 6, 6, 16)
					.WithArguments("TestMethodAttribute")
			],
			expected);
	}

	[TestMethod]
	public async Task Await_Diagnostic_XUnit()
	{
		var regex = new Regex(@"^\t{4}", RegexOptions.Multiline);

		var code = regex.Replace(
			"""
				using System;
				using System.Threading.Tasks;

				class Program
				{
					[Fact]
					void TestMethod1()
					{
						[|await Task.CompletedTask|];
					}
				}
				""",
			string.Empty).Replace("\t", "    ");

		var expected = regex.Replace(
			"""
				using System;
				using System.Threading.Tasks;

				class Program
				{
					[Fact]
					async Task TestMethod1()
					{
						await Task.CompletedTask;
					}
				}
				""",
			string.Empty).Replace("\t", "    ");

		await VerifyCS.VerifyCodeFixWithoutAnalyzerAsync(
			code,
			[
				DiagnosticResult.CompilerError("CS0246")
					.WithSpan(6, 6, 6, 10)
					.WithArguments("Fact"),
				DiagnosticResult.CompilerError("CS0246")
					.WithSpan(6, 6, 6, 10)
					.WithArguments("FactAttribute")
			],
			expected);
	}
}

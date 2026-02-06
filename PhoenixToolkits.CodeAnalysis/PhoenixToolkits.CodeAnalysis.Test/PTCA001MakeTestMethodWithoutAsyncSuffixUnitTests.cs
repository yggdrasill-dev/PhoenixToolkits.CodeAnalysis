using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = PhoenixToolkits.CodeAnalysis.Test.CSharpCodeFixVerifier<
    PhoenixToolkits.CodeAnalysis.PTCA001MakeTestMethodWithoutAsyncSuffixAnalyzer,
    PhoenixToolkits.CodeAnalysis.PTCA001MakeTestMethodWithoutAsyncSuffixCodeFixProvider>;

namespace PhoenixToolkits.CodeAnalysis.Test;

[TestClass]
public class MakeTestMethodWithoutAsyncSuffixUnitTest
{
    [TestMethod]
    public async Task Method_Not_AsyncFuntion()
    {
        var code = """
			using System;
			using System.Threading.Tasks;

			class Program
			{
				[TestMethod]
				void TestMethod1Async()
				{
				}
			}
			""";

        await VerifyCS.VerifyAnalyzerAsync(
            code,
            DiagnosticResult.CompilerError("CS0246")
                .WithSpan(6, 3, 6, 13)
                .WithArguments("TestMethod"),
            DiagnosticResult.CompilerError("CS0246")
                .WithSpan(6, 3, 6, 13)
                .WithArguments("TestMethodAttribute"));
    }

    [TestMethod]
    public async Task TestMethodWithoutAsyncSuffix_MsTest()
    {
        var code = """
			using System;
			using System.Threading.Tasks;

			class Program
			{
				[TestMethod]
				Task [|TestMethod1Async|]()
				{
					return Task.CompletedTask;
				}
			}
			""";

        await VerifyCS.VerifyAnalyzerAsync(
            code,
            DiagnosticResult.CompilerError("CS0246")
                .WithSpan(6, 3, 6, 13)
                .WithArguments("TestMethod"),
            DiagnosticResult.CompilerError("CS0246")
                .WithSpan(6, 3, 6, 13)
                .WithArguments("TestMethodAttribute"));
    }

    [TestMethod]
    public async Task TestMethodWithoutAsyncSuffix_MsTest_FullNamespace()
    {
        var code = """
			using System;
			using System.Threading.Tasks;

			class Program
			{
				[Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
				Task [|TestMethod1Async|]()
				{
					return Task.CompletedTask;
				}
			}
			""";

        await VerifyCS.VerifyAnalyzerAsync(
            code,
            DiagnosticResult.CompilerError("CS0234")
                .WithSpan(6, 13, 6, 25)
                .WithArguments("VisualStudio", "Microsoft"));
    }

    [TestMethod]
    public async Task TestMethodWithoutAsyncSuffix_NUnit()
    {
        var code = """
			using System;
			using System.Threading.Tasks;

			class Program
			{
				[Test]
				Task [|TestMethod1Async|]()
				{
					return Task.CompletedTask;
				}
			}
			""";

        await VerifyCS.VerifyAnalyzerAsync(
            code,
            DiagnosticResult.CompilerError("CS0246")
                .WithSpan(6, 3, 6, 7)
                .WithArguments("Test"),
            DiagnosticResult.CompilerError("CS0246")
                .WithSpan(6, 3, 6, 7)
                .WithArguments("TestAttribute"));
    }

    [TestMethod]
    public async Task TestMethodWithoutAsyncSuffix_NUnit_FullNamespace()
    {
        var code = """
			using System;
			using System.Threading.Tasks;

			class Program
			{
				[NUnit.Framework.Test]
				Task [|TestMethod1Async|]()
				{
					return Task.CompletedTask;
				}
			}
			""";

        await VerifyCS.VerifyAnalyzerAsync(
            code,
            DiagnosticResult.CompilerError("CS0246")
                .WithSpan(6, 3, 6, 8)
                .WithArguments("NUnit"));
    }

    [TestMethod]
    public async Task TestMethodWithoutAsyncSuffix_XUnit()
    {
        var code = """
			using System;
			using System.Threading.Tasks;

			class Program
			{
				[Fact]
				Task [|TestMethod1Async|]()
				{
					return Task.CompletedTask;
				}
			}
			""";

        await VerifyCS.VerifyAnalyzerAsync(
            code,
            DiagnosticResult.CompilerError("CS0246")
                .WithSpan(6, 3, 6, 7)
                .WithArguments("Fact"),
            DiagnosticResult.CompilerError("CS0246")
                .WithSpan(6, 3, 6, 7)
                .WithArguments("FactAttribute"));
    }

    [TestMethod]
    public async Task TestMethodWithoutAsyncSuffix_XUnit_FullNamespace()
    {
        var code = """
			using System;
			using System.Threading.Tasks;

			class Program
			{
				[Xunit.Fact]
				Task [|TestMethod1Async|]()
				{
					return Task.CompletedTask;
				}
			}
			""";

        await VerifyCS.VerifyAnalyzerAsync(
            code,
            DiagnosticResult.CompilerError("CS0246")
                .WithSpan(6, 3, 6, 8)
                .WithArguments("Xunit"));
    }

    [TestMethod]
    public async Task TestMethodWithoutAsyncSuffix_NoDiagnostic()
    {
        var code = """
			using System;
			using System.Threading.Tasks;

			class Program
			{
				[TestMethod]
				Task TestMethod1()
				{
					return Task.CompletedTask;
				}
			}
			""";

        await VerifyCS.VerifyAnalyzerAsync(
            code,
            DiagnosticResult.CompilerError("CS0246")
                .WithSpan(6, 3, 6, 13)
                .WithArguments("TestMethod"),
            DiagnosticResult.CompilerError("CS0246")
                .WithSpan(6, 3, 6, 13)
                .WithArguments("TestMethodAttribute"));
    }

    [TestMethod]
    public async Task TestMethodWithoutAsyncSuffix_Static_Method_NoDiagnostic()
    {
        var code = """
			using System;
			using System.Threading.Tasks;

			class Program
			{
				[TestMethod]
				static Task TestMethod1Async()
				{
					return Task.CompletedTask;
				}
			}
			""";

        await VerifyCS.VerifyAnalyzerAsync(
            code,
            DiagnosticResult.CompilerError("CS0246")
                .WithSpan(6, 3, 6, 13)
                .WithArguments("TestMethod"),
            DiagnosticResult.CompilerError("CS0246")
                .WithSpan(6, 3, 6, 13)
                .WithArguments("TestMethodAttribute"));
    }

    [TestMethod]
    public async Task MsTest_TestMethod_Diagnostic()
    {
        var code = """
			using System;
			using System.Threading.Tasks;

			class Program
			{
				[TestMethod]
				Task [|TestMethod1Async|]()
				{
					return Task.CompletedTask;
				}
			}
			""";

        var expected = """
			using System;
			using System.Threading.Tasks;

			class Program
			{
				[TestMethod]
				Task TestMethod1()
				{
					return Task.CompletedTask;
				}
			}
			""";

        await VerifyCS.VerifyCodeFixAsync(
            code,
            [
                DiagnosticResult.CompilerError("CS0246")
                    .WithSpan(6, 3, 6, 13)
                    .WithArguments("TestMethod"),
                DiagnosticResult.CompilerError("CS0246")
                    .WithSpan(6, 3, 6, 13)
                    .WithArguments("TestMethodAttribute")
            ],
            expected);
    }

    [TestMethod]
    public async Task NUnit_TestMethod_Diagnostic()
    {
        var code = """
			using System;
			using System.Threading.Tasks;

			class Program
			{
				[Test]
				Task [|TestMethod1Async|]()
				{
					return Task.CompletedTask;
				}
			}
			""";

        var expected = """
			using System;
			using System.Threading.Tasks;

			class Program
			{
				[Test]
				Task TestMethod1()
				{
					return Task.CompletedTask;
				}
			}
			""";

        await VerifyCS.VerifyCodeFixAsync(
            code,
            [
                DiagnosticResult.CompilerError("CS0246")
                    .WithSpan(6, 3, 6, 7)
                    .WithArguments("Test"),
                DiagnosticResult.CompilerError("CS0246")
                    .WithSpan(6, 3, 6, 7)
                    .WithArguments("TestAttribute")
            ],
            expected);
    }

    [TestMethod]
    public async Task XUnit_TestMethod_Diagnostic()
    {
        var code = """
			using System;
			using System.Threading.Tasks;

			class Program
			{
				[Fact]
				Task [|TestMethod1Async|]()
				{
					return Task.CompletedTask;
				}
			}
			""";

        var expected = """
			using System;
			using System.Threading.Tasks;

			class Program
			{
				[Fact]
				Task TestMethod1()
				{
					return Task.CompletedTask;
				}
			}
			""";

        await VerifyCS.VerifyCodeFixAsync(
            code,
            [
                DiagnosticResult.CompilerError("CS0246")
                    .WithSpan(6, 3, 6, 7)
                    .WithArguments("Fact"),
                DiagnosticResult.CompilerError("CS0246")
                    .WithSpan(6, 3, 6, 7)
                    .WithArguments("FactAttribute")
            ],
            expected);
    }

    [TestMethod]
    public async Task XUnit_TestMethod_Theory_Diagnostic()
    {
        var code = """
            using System;
            using System.Threading.Tasks;

            class Program
            {
                [Theory]
                [InlineData(1)]
                [InlineData(2)]
                Task [|TestMethod1Async|](int n)
                {
                    return Task.CompletedTask;
                }
            }
            """;

        var expected = """
            using System;
            using System.Threading.Tasks;

            class Program
            {
                [Theory]
                [InlineData(1)]
                [InlineData(2)]
                Task TestMethod1(int n)
                {
                    return Task.CompletedTask;
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            code,
            [
                DiagnosticResult.CompilerError("CS0246").WithSpan(6, 6, 6, 12).WithArguments("Theory"),
                DiagnosticResult.CompilerError("CS0246").WithSpan(6, 6, 6, 12).WithArguments("TheoryAttribute"),
                DiagnosticResult.CompilerError("CS0246").WithSpan(7, 6, 7, 16).WithArguments("InlineData"),
                DiagnosticResult.CompilerError("CS0246").WithSpan(7, 6, 7, 16).WithArguments("InlineDataAttribute"),
                DiagnosticResult.CompilerError("CS0246").WithSpan(8, 6, 8, 16).WithArguments("InlineData"),
                DiagnosticResult.CompilerError("CS0246").WithSpan(8, 6, 8, 16).WithArguments("InlineDataAttribute"),
            ],
            expected);
    }
}

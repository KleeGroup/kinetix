using System;
using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.DiagnosticsTest.DesignTest {

    [TestClass]
    public class ApiActionShouldBeDecoratedWithHttpVerbAnalyserTest : DiagnosticVerifier {

        // No diagnostics expected to show up
        [TestMethod]
        public void Check_EmptyCode_NoDiagnostic() {
            var test = string.Empty;

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Check_HttpGetAttribute_NoDiagnostic() {
            var test = @"
    using System;
    using System.Web.Http;

    namespace ConsoleApplication1 {        
        
        public class ReferentielReadController : ApiController {   
            
            [System.Web.Http.HttpGet()]
            public void LoadProduit(){
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        // Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_NoHttpVerbAttribute_Diagnostic() {
            var test = @"
    using System;
    using System.Web.Http;

    namespace ConsoleApplication1 {        
        
        public class ReferentielReadController : ApiController {   
            
            public void LoadProduit(){
            }
        }
    }";
            var expected = new DiagnosticResult {
                Id = FRC1111_ApiActionShouldBeDecoratedWithHttpVerbAnalyser.DiagnosticId,
                Message = string.Format(
                    "L'action {1} du controller {0} doit être décorée avec un verbe HTTP explicite.",
                    "ReferentielReadController",
                    "LoadProduit"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 25)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1111_ApiActionShouldBeDecoratedWithHttpVerbAnalyser();
        }
    }
}
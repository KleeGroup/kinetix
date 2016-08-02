using System;
using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.DiagnosticsTest.DesignTest {

    [TestClass]
    public class ApiActionShouldBeDecoratedWithRouteAnalyserTest : DiagnosticVerifier {

        // No diagnostics expected to show up
        [TestMethod]
        public void Check_EmptyCode_NoDiagnostic() {
            var test = string.Empty;

            VerifyCSharpDiagnostic(test);
        }

        // Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_NoRouteAttribute_Diagnostic() {
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
                Id = FRC1112_ApiActionShouldBeDecoratedWithRouteAnalyser.DiagnosticId,
                Message = string.Format(
                    "L'action {1} du controller {0} doit être décorée avec une route explicite.",
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

        [TestMethod]
        public void Check_RouteAttribute_NoDiagnostic() {
            var test = @"
    using System;
    using System.Web.Http;

    namespace ConsoleApplication1 {        
        
        public class ReferentielReadController : ApiController {   
            
            [System.Web.Http.RouteAttribute(""api/referentielRead/loadProduit"")]
            public void LoadProduit(){
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1112_ApiActionShouldBeDecoratedWithRouteAnalyser();
        }
    }
}
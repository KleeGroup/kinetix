using System;
using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.DiagnosticsTest.DesignTest {
    [TestClass]
    public class NoThreadSleepAnalyzerTest : DiagnosticVerifier {

        // Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_Diagnostic() {
            var test = @"
    using System;
    using System.ServiceModel;    
    using System.Threading;

    namespace ConsoleApplication1
    {
        class ServiceReferentielRead
        {   
            public void LoadProduit(){
                Thread.Sleep(1000);
            }
        }
    }";
            var expected = new DiagnosticResult {
                Id = FRC1102_NoThreadSleepAnalyzer.DiagnosticId,
                Message = string.Format("Ne pas utiliser Thread.Sleep."),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        // No diagnostics expected to show up
        [TestMethod]
        public void Check_NoDiagnostic() {
            var test = string.Empty;

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Check_NoService_NoDiagnostic() {
            var test = @"
    using System;
    using System.ServiceModel;    
    using System.Threading;

    namespace ConsoleApplication1
    {
        class ServiceReferentielRead
        {   
            public void LoadProduit(){
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1102_NoThreadSleepAnalyzer();
        }
    }
}
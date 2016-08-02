using System;
using Fmk.RoslynCop.Diagnostics.Maintainability;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.DiagnosticsTest.MaintainabilityTest {
    [TestClass]
    public class ClassShouldBeSmallAnalyzerTest : DiagnosticVerifier {

        // Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_ClassTooBig_Diagnostic() {
            var test = @"
    using System;
    using System.ServiceModel;    

    namespace ConsoleApplication1
    {
        public class ServiceReferentielRead
        {   
            public void Go(){
                var x = 1;
                var y = 1;
            }
        }
    }";
            var expected = new DiagnosticResult {
                Id = FRC1200_ClassShouldBeSmallAnalyzer.DiagnosticId,
                Message = string.Format("La classe {0} fait {1} lignes, ce qui dépasse la limite de {2}.", "ServiceReferentielRead", 7, 5),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 7, 22)
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
        public void Check_SmallClass_NoDiagnostic() {
            var test = @"
    using System;
    using System.ServiceModel;    

    namespace ConsoleApplication1
    {
        class ServiceReferentielRead
        {   
            private object _state;
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1200_ClassShouldBeSmallAnalyzer(5);
        }
    }
}
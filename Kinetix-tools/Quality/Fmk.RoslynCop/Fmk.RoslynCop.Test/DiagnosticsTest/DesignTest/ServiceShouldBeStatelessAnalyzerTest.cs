using System;
using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.DiagnosticsTest.DesignTest {
    [TestClass]
    public class ServiceShouldBeStatelessAnalyzerTest : DiagnosticVerifier {

        // Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_Diagnostic() {
            var test = @"
    using System;
    using System.ServiceModel;    

    namespace ConsoleApplication1
    {
        [ServiceBehavior]
        public class ServiceReferentielRead
        {   
            private readonly object _ok;
            private object _state;
        }
    }";
            var expected = new DiagnosticResult {
                Id = FRC1103_ServiceShouldBeStatelessAnalyzer.DiagnosticId,
                Message = string.Format("Le champ {1} du service {0} doit être const ou readonly.", "ServiceReferentielRead", "_state"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 13)
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
            return new FRC1103_ServiceShouldBeStatelessAnalyzer();
        }
    }
}
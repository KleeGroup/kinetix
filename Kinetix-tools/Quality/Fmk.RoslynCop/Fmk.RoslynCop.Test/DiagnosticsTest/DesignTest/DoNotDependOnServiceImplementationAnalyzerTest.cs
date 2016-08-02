using System;
using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.DiagnosticsTest.DesignTest {
    [TestClass]
    public class DoNotDependOnServiceImplementationAnalyzerTest : DiagnosticVerifier {

        // No diagnostics expected to show up
        [TestMethod]
        public void Check_EmptyCode_NoDiagnostic() {
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
        public class MoteurComponent {
        }

        public class MoteurCalcul
        {   
            public MoteurCalcul(MoteurComponent component){
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Check_ServiceContract_NoDiagnostic() {
            var test = @"
    using System;
    using System.ServiceModel;    

    namespace ConsoleApplication1
    {
        [ServiceContract]
        public interface IServiceReferentielRead {
        }

        [ServiceBehavior]
        public class ServiceReferentielRead : IServiceReferentielRead {
        }

        public class MoteurCalcul
        {   
            public MoteurCalcul(IServiceReferentielRead service){
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        // Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_ServiceImpl_Diagnostic() {
            var test = @"
    using System;
    using System.ServiceModel;    

    namespace ConsoleApplication1
    {
        [ServiceContract]
        public interface IServiceReferentielRead {
        }

        [ServiceBehavior]
        public class ServiceReferentielRead : IServiceReferentielRead {
        }

        public class MoteurCalcul
        {   
            public MoteurCalcul(ServiceReferentielRead service){
            }
        }
    }";
            var expected = new DiagnosticResult {
                Id = FRC1100_DoNotDependOnServiceImplementationAnalyzer.DiagnosticId,
                Message = string.Format("La classe {0} ne doit pas dépendre de l'implémentation de service {1}.", "MoteurCalcul", "ServiceReferentielRead"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 17, 33)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1100_DoNotDependOnServiceImplementationAnalyzer();
        }
    }
}
using System;
using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.DiagnosticsTest.DesignTest {

    [TestClass]
    public class ServiceImplementationClassDecorationAnalyserTest : DiagnosticVerifier {

        [TestMethod]
        public void Check_Attribute_NoDiagnostic() {
            var test = @"
    using System;
    using System.ServiceModel;    

    namespace Zorro.MachinImplementation {        
      
        [ServiceBehavior]
        public class ServiceReferentiel {   
            
            public void LoadProduit(){
            }

            public void SaveProduit(){
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        // No diagnostics expected to show up
        [TestMethod]
        public void Check_EmptyCode_NoDiagnostic() {
            var test = string.Empty;

            VerifyCSharpDiagnostic(test);
        }

        // Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        [Ignore]
        public void Check_NoAttribute_Diagnostic() {
            var test = @"
    using System;
    using System.ServiceModel;    

    namespace Zorro.MachinImplementation {        
      
        public class ServiceReferentiel {   
            
            public void LoadProduit(){
            }

            public void SaveProduit(){
            }
        }
    }";
            var expected = new DiagnosticResult {
                Id = FRC1107_ServiceImplementationClassDecorationAnalyser.DiagnosticId,
                Message = string.Format(
                    "Le service {0} doit être décoré avec l'attribut WCF ServiceBehavior.",
                    "ServiceReferentiel"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 7, 22)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1107_ServiceImplementationClassDecorationAnalyser();
        }
    }
}
using System;
using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.DiagnosticsTest.DesignTest {

    [TestClass]
    public class WcfServiceImplementationAnalyserTest : DiagnosticVerifier {

        /// <summary>
        /// Commentaire.
        /// </summary>
        [TestMethod]
        public void Check_AttributeOk_NoDiagnostic() {
            var test = @"
    using System;
    using System.ServiceModel;    

    namespace ConsoleApplication1 {        
      
        [ServiceBehavior]
        public class ServiceReferentiel {   
            
            [System.Diagnostics.CodeAnalysis.SuppressMessage(""FC0001"", ""Justification."")]
            public void LoadProduit(){
            }

            public void SaveProduit(){
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        // Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_Attribute_Diagnostic() {
            var test = @"
    using System;
    using System.ServiceModel;    

    namespace ConsoleApplication1 {        
      
        [ServiceBehavior]
        public class ServiceReferentiel {   
            
            [OperationContract]
            public void LoadProduit(){
            }

            public void SaveProduit(){
            }
        }
    }";
            var expected = new DiagnosticResult {
                Id = FRC1104_WcfServiceImplementationAnalyser.DiagnosticId,
                Message = string.Format(
                    "La méthode {1} du service {0} ne doit pas être décorée avec l'attribut {2}.",
                    "ServiceReferentiel",
                    "LoadProduit",
                    "OperationContractAttribute"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 14)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        // No diagnostics expected to show up
        [TestMethod]
        public void Check_EmptyCode_NoDiagnostic() {
            var test = string.Empty;

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Check_NoAttribute_NoDiagnostic() {
            var test = @"
    using System;
    using System.ServiceModel;    

    namespace ConsoleApplication1 {        
      
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

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1104_WcfServiceImplementationAnalyser();
        }
    }
}
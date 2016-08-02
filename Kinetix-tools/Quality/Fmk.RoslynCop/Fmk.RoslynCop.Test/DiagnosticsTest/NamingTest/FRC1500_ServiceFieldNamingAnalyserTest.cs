using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.DiagnosticsTest.DesignTest {
    [TestClass]
    public class FRC1500_ServiceFieldNamingAnalyserTest : DiagnosticVerifier {

        //No diagnostics expected to show up
        [TestMethod]
        public void Check_NoDiagnostic() {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Check_NamingOk_NoDiagnostic() {
            var test = @"
    using System;
    using System.ServiceModel;    

    namespace Zorro {        
      
        [System.ServiceModel.ServiceContract]
        public interface IServiceReferentiel {  
        }
              
        public class ServiceGarcia {  

            private readonly IServiceReferentiel _serviceReferentiel;
        }
    }
";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_NamingKo_Diagnostic() {
            var test = @"
    using System;
    using System.ServiceModel;    

    namespace Zorro {        
      
        [System.ServiceModel.ServiceContract]
        public interface IServiceReferentiel {  
        }
              
        public class ServiceGarcia {  

            private readonly IServiceReferentiel _serviceRoger;
        }
    }
";
            var expected = new DiagnosticResult {
                Id = FRC1500_ServiceFieldNamingAnalyser.DiagnosticId,
                Message = string.Format("Le champ {1} de la classe {0} doit être nommé {2}.",
                "ServiceGarcia",
                "_serviceRoger",
                "_serviceReferentiel"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 50)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1500_ServiceFieldNamingAnalyser();
        }
    }
}
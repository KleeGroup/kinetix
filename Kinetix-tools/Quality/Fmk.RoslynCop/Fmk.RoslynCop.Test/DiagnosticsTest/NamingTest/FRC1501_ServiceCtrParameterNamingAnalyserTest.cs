using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.DiagnosticsTest.DesignTest {
    [TestClass]
    public class FRC1501_ServiceCtrParameterNamingAnalyserTest : DiagnosticVerifier {

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

            public ServiceGarcia(IServiceReferentiel serviceReferentiel){
                _serviceReferentiel = serviceReferentiel;
            }
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

            private readonly IServiceReferentiel _serviceReferentiel;

            public ServiceGarcia(IServiceReferentiel serviceRoger){
                _serviceReferentiel = serviceRoger;
            }
        }
    }
";
            var expected = new DiagnosticResult {
                Id = FRC1501_ServiceCtrParameterNamingAnalyser.DiagnosticId,
                Message = string.Format("Le paramètre {1} du constructeur la classe {0} doit être nommé {2}.",
                "ServiceGarcia",
                "serviceRoger",
                "serviceReferentiel"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 15, 54)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1501_ServiceCtrParameterNamingAnalyser();
        }
    }
}
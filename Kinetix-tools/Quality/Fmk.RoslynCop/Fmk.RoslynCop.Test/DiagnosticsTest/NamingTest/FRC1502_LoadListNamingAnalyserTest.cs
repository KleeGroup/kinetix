using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.DiagnosticsTest.DesignTest {
    [TestClass]
    public class FRC1502_LoadListNamingAnalyserTest : DiagnosticVerifier {

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
    using System.Collections.Generic;  

    namespace Zorro.MachinContract {        

        public class MonBean {

        }
      
        [System.ServiceModel.ServiceContract]
        public interface IServiceReferentiel {  

                ICollection<MonBean> LoadMonBeanList();
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
    using System.Collections.Generic;  

    namespace Zorro.MachinContract {        

        public class MonBean {

        }
      
        [System.ServiceModel.ServiceContract]
        public interface IServiceReferentiel {  

                ICollection<MonBean> LoadMonBean();
        }
    }
";

            var expected = new DiagnosticResult {
                Id = FRC1502_LoadListNamingAnalyser.DiagnosticId,
                Message = string.Format("La méthode {1} du service {0} ne suit pas la règle de nommage Load[Nom métier]List[Complément].",
                "IServiceReferentiel",
                "LoadMonBean"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 15, 38)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1502_LoadListNamingAnalyser();
        }
    }
}
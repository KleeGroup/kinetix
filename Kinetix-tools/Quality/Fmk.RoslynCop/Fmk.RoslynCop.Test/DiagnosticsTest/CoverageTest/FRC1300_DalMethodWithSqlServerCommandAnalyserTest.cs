using System;
using Fmk.RoslynCop.Diagnostics.Coverage;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.DiagnosticsTest.CoverageTest {

    [TestClass]
    public class FRC1300_DalMethodWithSqlServerCommandAnalyserTest : DiagnosticVerifier {

        [TestMethod]
        public void Check_EmptyCode_NoDiagnostic() {
            var test = string.Empty;
            VerifyCSharpDiagnostic(test);
        }

        // Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_GerBroker_Diagnostic() {
            var test = @"
    using System;
    using System.ServiceModel;    

    namespace Zorro.MachinImplementation {        
      
        [ServiceBehavior]
        public class DalReferentiel : AbstractDal {   
            
            public Alerte LoadAlerteReception(int vrsId) {
                return GetBroker<Alerte>()[0];
            }
        }

        public abstract class AbstractDal {
            
               public T[] GetBroker<T>(){
                    return new T[0];
               }
        }
    }";
            var expected = new DiagnosticResult {
                Id = FRC1300_DalMethodWithSqlServerCommandAnalyser.DiagnosticId,
                Message = string.Format(
                    "La méthode utilise SqlServerCommand ou GetBroker."),
                Severity = DiagnosticSeverity.Hidden,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 27)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void Check_NoBroker_Diagnostic() {
            var test = @"
    using System;
    using System.ServiceModel;    

    namespace Zorro.MachinImplementation {        
      
        [ServiceBehavior]
        public class DalReferentiel {   
            
            public Alerte LoadAlerteReception(int vrsId) {                
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1300_DalMethodWithSqlServerCommandAnalyser();
        }
    }
}
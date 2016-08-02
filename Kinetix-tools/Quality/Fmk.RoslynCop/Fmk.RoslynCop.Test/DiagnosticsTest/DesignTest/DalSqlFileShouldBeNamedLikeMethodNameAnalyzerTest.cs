using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.DiagnosticsTest.DesignTest {

    [TestClass]
    public class DalSqlFileShouldBeNamedLikeMethodNameAnalyzerTest : DiagnosticVerifier {

        // No diagnostics expected to show up
        [TestMethod]
        public void Check_EmptyCode_NoDiagnostic() {
            var test = string.Empty;

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Check_NameConsistent_NoDiagnostic() {
            var test = @"
using System;
using System.ServiceModel;

namespace ConsoleApplication1 {        
      
    [ServiceBehavior]
    public class DalReferentiel {   

        public void LoadProduitList(){
            var cmd = GetSqlServerCommand(""LoadProduitList.sql"");
        }

        private bool GetSqlServerCommand(string s) {
            return false;
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        // Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_NameDifferent_Diagnostic() {
            var test = @"
using System;
using System.ServiceModel;

namespace ConsoleApplication1 {        
      
    [ServiceBehavior]
    public class DalReferentiel {   

        public void LoadProduitList(){
            var cmd = GetSqlServerCommand(""LoadProduit.sql"");
        }

        private bool GetSqlServerCommand(string s) {
            return false;
        }
    }
}";
            var expected = new DiagnosticResult {
                Id = FRC1106_DalSqlFileShouldBeNamedLikeMethodNameAnalyzer.DiagnosticId,
                Message = string.Format(
                    "Le nom du fichier SQL {0} n'est pas le même que le nom de la méthode de DAL {1}.",
                    "LoadProduit",
                    "LoadProduitList"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] { new DiagnosticResultLocation("Test0.cs", 11, 43) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void Check_NoLiteral_NoDiagnostic() {
            var test = @"
using System;
using System.ServiceModel;

namespace ConsoleApplication1 {        
      
    [ServiceBehavior]
    public class DalReferentiel {   

        public void LoadProduitList(){
            var query = ""LoadProduitList.sql"";
            var cmd = GetSqlServerCommand(query);
        }

        private bool GetSqlServerCommand(string s) {
            return false;
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1106_DalSqlFileShouldBeNamedLikeMethodNameAnalyzer();
        }
    }
}
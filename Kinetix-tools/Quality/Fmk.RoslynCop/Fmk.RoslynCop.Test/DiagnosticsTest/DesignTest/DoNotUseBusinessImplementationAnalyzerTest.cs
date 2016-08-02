using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.DiagnosticsTest.DesignTest {
    [TestClass]
    public class DoNotUseBusinessImplementationAnalyzerTest : DiagnosticVerifier {

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
                ServiceContractAttribute x;
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1101_DoNotUseBusinessImplementationAnalyzer();
        }
    }
}
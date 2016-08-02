using Fmk.RoslynCop.CodeFixes;
using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.CodeFixesTest {
    [TestClass]
    public class ReplaceImplementationWithContractFixTest : CodeFixVerifier {

        // Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_Fix() {
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
            var fixtest = @"
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
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() {
            return new ReplaceImplementationWithContractFix();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1100_DoNotDependOnServiceImplementationAnalyzer();
        }
    }
}
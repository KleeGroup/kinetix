using Fmk.RoslynCop.CodeFixes;
using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.CodeFixesTest {
    [TestClass]
    public class RenameServiceFieldFixTest : CodeFixVerifier {

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_Fix() {
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
            var fixtest = @"
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
            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() {
            return new RenameServiceFieldFix();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1500_ServiceFieldNamingAnalyser();
        }
    }
}
using Fmk.RoslynCop.CodeFixes;
using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.CodeFixesTest {

    [TestClass]
    public class AddHttpVerbAttributeFixTest : CodeFixVerifier {

        // Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_AlreadyWithAttribute() {
            var test = @"
using System;
using System.Web.Http;

namespace ConsoleApplication1 {        
        
    public class ReferentielReadController : ApiController {

        /// <summary>
        /// Commentaire.
        /// </summary>         
        [Route(""api/referentielRead/loadProduit"")]
        public void LoadProduit(){
        }
    }
}";

            var fixtest = @"
using System;
using System.Web.Http;

namespace ConsoleApplication1 {        
        
    public class ReferentielReadController : ApiController {

        /// <summary>
        /// Commentaire.
        /// </summary>         
        [HttpGet]
        [Route(""api/referentielRead/loadProduit"")]
        public void LoadProduit(){
        }
    }
}";
            VerifyCSharpFix(test, fixtest, 0);
        }

        // Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_NoAttribute() {
            var test = @"
using System;
using System.Web.Http;

namespace ConsoleApplication1 {        
        
    public class ReferentielReadController : ApiController {

        /// <summary>
        /// Commentaire.
        /// </summary>         
        public void LoadProduit(){
        }
    }
}";

            var fixtest = @"
using System;
using System.Web.Http;

namespace ConsoleApplication1 {        
        
    public class ReferentielReadController : ApiController {

        /// <summary>
        /// Commentaire.
        /// </summary>         
        [HttpGet]
        public void LoadProduit(){
        }
    }
}";
            VerifyCSharpFix(test, fixtest, 0);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() {
            return new AddHttpVerbAttributeFix();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1111_ApiActionShouldBeDecoratedWithHttpVerbAnalyser();
        }
    }
}
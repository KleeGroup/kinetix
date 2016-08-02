using Fmk.RoslynCop.CodeFixes;
using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.CodeFixesTest {
    [TestClass]
    public class MakeFieldStatelessFixTest : CodeFixVerifier {

        // Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_DiagnosticAndFix() {
            var test = @"
    using System;
    using System.ServiceModel;    

    namespace ConsoleApplication1
    {
        [ServiceBehavior]
        public class ServiceReferentielRead
        {   
            private readonly object _ok;
            private object _state;
        }
    }";

            var fixtest = @"
    using System;
    using System.ServiceModel;    

    namespace ConsoleApplication1
    {
        [ServiceBehavior]
        public class ServiceReferentielRead
        {   
            private readonly object _ok;
            private readonly object _state;
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() {
            return new MakeFieldStatelessFix();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1103_ServiceShouldBeStatelessAnalyzer();
        }
    }
}
using Fmk.RoslynCop.CodeFixes;
using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Fmk.RoslynCop.Test.CodeFixesTest {
    [TestClass]
    public class RemoveAttributeFixTest : CodeFixVerifier {

        // Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_AttributeList_Fix() {
            var test = @"
    using System;
    using System.ServiceModel;

    namespace ConsoleApplication1
    {
        
        [ServiceBehavior]
        public class ServiceReferentielRead {

            /// <summary>
            /// Commentaire.
            /// </summary>
            [OperationContract, System.Diagnostics.CodeAnalysis.SuppressMessage(""FC0001"", ""Justification."")]
            public void LoadProduit(){
            }
        }
    }";
            var fixtest = @"
    using System;
    using System.ServiceModel;

    namespace ConsoleApplication1
    {
        
        [ServiceBehavior]
        public class ServiceReferentielRead {

            /// <summary>
            /// Commentaire.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage(""FC0001"", ""Justification."")]
            public void LoadProduit(){
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        /// <summary>
        /// Commentaire.
        /// </summary>
        [TestMethod]
        public void Check_MultiAttributeListe1_Fix() {
            var test = @"
    using System;
    using System.ServiceModel;

    namespace ConsoleApplication1
    {
        
        [ServiceBehavior]
        public class ServiceReferentielRead {

            /// <summary>
            /// Commentaire.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage(""FC0001"", ""Justification."")]
            [OperationContract]
            public void LoadProduit(){
            }
        }
    }";
            var fixtest = @"
    using System;
    using System.ServiceModel;

    namespace ConsoleApplication1
    {
        
        [ServiceBehavior]
        public class ServiceReferentielRead {

            /// <summary>
            /// Commentaire.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage(""FC0001"", ""Justification."")]
            public void LoadProduit(){
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        // Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_MultiAttributeListe2_Fix() {
            var test = @"
    using System;
    using System.ServiceModel;

    namespace ConsoleApplication1
    {
        
        [ServiceBehavior]
        public class ServiceReferentielRead {

            /// <summary>
            /// Commentaire.
            /// </summary>
            [OperationContract]
            [System.Diagnostics.CodeAnalysis.SuppressMessage(""FC0001"", ""Justification."")]
            public void LoadProduit(){
            }
        }
    }";
            var fixtest = @"
    using System;
    using System.ServiceModel;

    namespace ConsoleApplication1
    {
        
        [ServiceBehavior]
        public class ServiceReferentielRead {

            /// <summary>
            /// Commentaire.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage(""FC0001"", ""Justification."")]
            public void LoadProduit(){
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        // Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Check_SingleAttribute_Fix() {
            var test = @"
    using System;
    using System.ServiceModel;    

    namespace ConsoleApplication1
    {
        
        [ServiceBehavior]
        public class ServiceReferentielRead {

            /// <summary>
            /// Commentaire.
            /// </summary>
            [OperationContract]
            public void LoadProduit(){
            }
        }
    }";
            var fixtest = @"
    using System;
    using System.ServiceModel;    

    namespace ConsoleApplication1
    {
        
        [ServiceBehavior]
        public class ServiceReferentielRead {

            /// <summary>
            /// Commentaire.
            /// </summary>
            public void LoadProduit(){
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() {
            return new RemoveAttributeFix();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FRC1104_WcfServiceImplementationAnalyser();
        }
    }
}
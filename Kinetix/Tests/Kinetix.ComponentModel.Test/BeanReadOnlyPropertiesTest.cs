using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kinetix.ComponentModel.Test {

    /// <summary>
    /// Test vérifiant le support de l'attribut ReadOnly.
    /// </summary>
    [TestClass]
    public class BeanReadOnlyPropertiesTest {

        /// <summary>
        /// Vérifie un support direct.
        /// </summary>
        [TestMethod]
        public void CheckReadOnlySupport() {
            BeanDefinition def = BeanDescriptor.GetDefinition(typeof(BeanPropertyROTest));
            Assert.IsTrue(def.Properties["Libelle"].IsReadOnly);
        }

        /// <summary>
        /// Vérifie un support indirect via MetadataTypeAttribute.
        /// </summary>
        [TestMethod]
        public void CheckReadOnlySupportThroughMetadataTypeAttribute() {
            BeanDefinition def = BeanDescriptor.GetDefinition(typeof(BeanPropertyROMetaTest));
            Assert.IsTrue(def.Properties["Libelle"].IsReadOnly);
        }
    }

    /// <summary>
    /// Classe permettant de vérifier le support de l'attribut ReadOnly.
    /// </summary>
    public class BeanPropertyROTest {

        /// <summary>
        /// Obtient le libellé.
        /// </summary>
        [ReadOnly(true)]
        public string Libelle {
            get;
            set;
        }
    }

    /// <summary>
    /// Classe permettant de vérifier le support de l'attribut ReadOnly via utilisation d'un décorateur externe.
    /// </summary>
    [MetadataType(typeof(BeanPropertyROTest))]
    public class BeanPropertyROMetaTest {

        /// <summary>
        /// Obtient ou définit le libellé non décoré.
        /// </summary>
        public object Libelle {
            get;
            set;
        }
    }
}

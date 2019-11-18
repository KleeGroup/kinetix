using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kinetix.ComponentModel;
using Kinetix.ServiceModel;

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Implémentation simple de IResourceWriter.
    /// S'appuie sur ReferenceBrokerTestHelper.
    /// </summary>
    public class ServiceResourceWriterMock : IResourceWriter {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceType"></param>
        /// <param name="primaryKey"></param>
        public void DeleteTraductionReferenceByReferenceAndPrimaryKey(Type referenceType, object primaryKey) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceType"></param>
        /// <param name="bean"></param>
        /// <param name="cultureUI"></param>
        public void SaveTraductionReference(Type referenceType, object bean, string cultureUI) {
            SaveTraductionReferenceList(referenceType, new List<object> { bean }, cultureUI);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceType"></param>
        /// <param name="beanList"></param>
        /// <param name="cultureUI"></param>
        public void SaveTraductionReferenceList(Type referenceType, ICollection beanList, string cultureUI) {
            BeanDefinition definition = BeanDescriptor.GetDefinition(referenceType);
            ICollection<BeanPropertyDescriptor> translatablePropList = definition.Properties.Where(x => x.IsTranslatable).ToList();

            foreach (object bean in beanList) {
                foreach (BeanPropertyDescriptor property in translatablePropList) {
                    object value = property.GetValue(bean);
                    if (value != null) {
                        string code = definition.PrimaryKey.GetValue(bean).ToString();
                        string languageCode = ReferenceBrokerTestHelper.LangueCode;
                        Tuple<string, string> key = new Tuple<string, string>(code, languageCode);
                        ReferenceBrokerTestHelper.Traduction[key] = value.ToString();
                    }
                }
            }
        }
    }

}

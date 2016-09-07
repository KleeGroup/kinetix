using Kinetix.ComponentModel;
using System.Collections.Generic;

namespace Kinetix.Rules
{
    public class RuleContext
    {
        private readonly IDictionary<string, string> context;

        public RuleContext(object obj, RuleConstants constants)
        {
            context = new Dictionary<string, string>();

            BeanDefinition definition = BeanDescriptor.GetDefinition(obj.GetType());
            BeanPropertyDescriptorCollection properties = definition.Properties;

            foreach (BeanPropertyDescriptor bean in properties)
            {
                context[bean.PropertyName] = bean.GetValue(obj).ToString();
            }

            if (constants != null)
            {
                foreach (KeyValuePair<string, string> keyValue in constants.GetValues())
                {
                    context.Add(keyValue);
                }
            }
        }

        public string this[string key]
        {
            get
            {
                return context[key];
            }
        }

    }
}

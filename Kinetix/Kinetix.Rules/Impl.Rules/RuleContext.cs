using Kinetix.ComponentModel;
using Kinetix.Rules.Impl.Rules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kinetix.Rules
{
    public class RuleContext
    {
        private readonly IDictionary<string, object> context;

        public RuleContext(object obj, RuleConstants constants)
        {
            context = new Dictionary<string, object>();

            BeanDefinition definition = BeanDescriptor.GetDefinition(obj.GetType());
            BeanPropertyDescriptorCollection properties = definition.Properties;

            foreach (BeanPropertyDescriptor bean in properties)
            {
                object val = bean.GetValue(obj);
                if (val != null)
                {
                    object value;
                    if (val is IList)
                    {
                        IList valList = (IList) val;
                        value = valList.Cast<object>().Select(v => v.ToString()).ToList();
                    }
                    else if (val is decimal)
                    {
                        value = val;
                    }
                    else
                    {
                        value = val.ToString();
                    }
                    context[bean.PropertyName] = value;
                }
            }

            if (constants != null)
            {
                foreach (KeyValuePair<string, string> keyValue in constants.GetValues())
                {
                    context.Add(keyValue.Key, keyValue.Value);
                }
            }
        }

        public object this[string key]
        {
            get
            {
                return context[key];
            }
        }

        public void TryGetValue(string key, out object obj)
        {
            context.TryGetValue(key, out obj);
        }

    }
}

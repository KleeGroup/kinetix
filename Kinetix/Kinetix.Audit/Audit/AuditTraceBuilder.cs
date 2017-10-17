using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Kinetix.Audit.Audit {
    public class AuditTraceBuilder
    {
        private int? Id;
        private readonly string Category;
	    private readonly string Username;
	    private DateTime BusinessDate;
        private readonly DateTime ExecutionDate;
	    private readonly int Item;
	    private readonly string Message;
	    private string Context;

        /// <summary>
        /// Builder for Audit trace.
        /// </summary>
        /// <param name="category">category.</param>
        /// <param name="username">username.</param>
        /// <param name="item">item.</param>
        /// <param name="message">message.</param>
        public AuditTraceBuilder(string category, string username, int item, string message)
        {
            Debug.Assert(!String.IsNullOrEmpty(category));
            Debug.Assert(!String.IsNullOrEmpty(username));
            //---
            this.Id = null;
            this.Category = category;
            this.Username = username;
            this.Message = message;
            this.Item = item;
            this.ExecutionDate = DateTime.UtcNow;
        }


        /// <summary>
        /// Optionnal business date.
        /// </summary>
        /// <param name="dateBusiness">dateBusiness.</param>
        /// <returns>the builder (for fluent style)</returns>
        public AuditTraceBuilder WithDateBusiness(DateTime dateBusiness)
        {
            Debug.Assert(dateBusiness != null);
            //---
            this.BusinessDate = dateBusiness;
            return this;
        }

        /// <summary>
        /// Optionnal context.
        /// </summary>
        /// <param name="context">context for metadata.</param>
        /// <returns>the builder (for fluent style)</returns>
        public AuditTraceBuilder WithContext(IList<String> context)
        {
            Debug.Assert(context != null);
            Debug.Assert(context.Count > 0, "The provided context is empty");
            //---
            StringBuilder sb = new StringBuilder();
            foreach (string contextString in context)
            {
                sb.Append(contextString).Append("|");
            }
            this.Context = sb.ToString();
            return this;
        }

        
        public AuditTrace Build()
        {
            return new AuditTrace(Id, Category, Username, BusinessDate, ExecutionDate, Item, Message, Context);
        }

    }
}

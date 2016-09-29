using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Audit.Audit
{
    public class AuditTraceCriteriaBuilder
    {
        private string myCategory;
        private string myUser;
        private DateTime? myStartBusinessDate;
        private DateTime? myEndBusinessDate;
        private DateTime? myStartExecutionDate;
        private DateTime? myEndExecutionDate;
        private int? myItem;

        /// <summary>
        /// Optionnal category.
        /// </summary>
        /// <param name="category">category.</param>
        /// <returns>the builder (for fluent style)</returns>
        public AuditTraceCriteriaBuilder WithCategory(string category)
        {
            Debug.Assert(category != null);
            //---
            this.myCategory = category;
            return this;
        }

        /// <summary>
        /// Optionnal user.
        /// </summary>
        /// <param name="user">user.</param>
        /// <returns>the builder (for fluent style)</returns>
        public AuditTraceCriteriaBuilder WithUser(string user)
        {
            Debug.Assert(user != null);
            //---
            this.myUser = user;
            return this;
        }

        /// <summary>
        /// Optionnal starting business date range.
        /// </summary>
        /// <param name="startBusinessDate">startBusinessDate.</param>
        /// <returns>the builder (for fluent style)</returns>
        public AuditTraceCriteriaBuilder WithDateBusinessStart(DateTime startBusinessDate)
        {
            Debug.Assert(startBusinessDate != null);
            //---
            this.myStartBusinessDate = startBusinessDate;
            return this;
        }

        /// <summary>
        /// Optionnal ending business date range.
        /// </summary>
        /// <param name="endBusinessDate">endBusinessDate.</param>
        /// <returns>the builder (for fluent style)</returns>
        public AuditTraceCriteriaBuilder WithDateBusinessEnd(DateTime endBusinessDate)
        {
            Debug.Assert(endBusinessDate != null);
            //---
            this.myEndBusinessDate = endBusinessDate;
            return this;
        }

        /// <summary>
        /// Optionnal starting execution date range.
        /// </summary>
        /// <param name="startExecutionDate">startExecutionDate.</param>
        /// <returns>the builder (for fluent style)</returns>
        public AuditTraceCriteriaBuilder WithDateExecutionStart(DateTime startExecutionDate)
        {
            Debug.Assert(startExecutionDate != null);
            //---
            this.myStartExecutionDate = startExecutionDate;
            return this;
        }

        /// <summary>
        /// Optionnal ending business date range.
        /// </summary>
        /// <param name="endExecutionDate">endExecutionDate.</param>
        /// <returns>the builder (for fluent style)</returns>
        public AuditTraceCriteriaBuilder WithDateExecutionEnd(DateTime endExecutionDate)
        {
            Debug.Assert(endExecutionDate != null);
            //---
            this.myEndExecutionDate = endExecutionDate;
            return this;
        }

        /// <summary>
        /// Optionnal item id.
        /// </summary>
        /// <param name="item">item.</param>
        /// <returns>the builder (for fluent style)</returns>
        public AuditTraceCriteriaBuilder WithItem(int? item)
        {
            Debug.Assert(item != null);
            //---
            this.myItem = item;
            return this;
        }


        public AuditTraceCriteria Build()
        {
            return new AuditTraceCriteria(this.myCategory, this.myUser, this.myStartBusinessDate,
                    this.myEndBusinessDate, this.myStartExecutionDate, this.myEndExecutionDate, this.myItem);
        }
    }
}

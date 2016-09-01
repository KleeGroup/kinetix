using System;

namespace Kinetix.Audit
{
    public class AuditTraceCriteria
    {

        public AuditTraceCriteria(
              string category,
              string user,
              DateTime? startBusinessDate,
              DateTime? endBusinessDate,
              DateTime? startExecutionDate,
              DateTime? endExecutionDate,
              long? item) {
            this.Category = category;
            this.User = user;
            this.StartBusinessDate = startBusinessDate;
            this.EndBusinessDate = endBusinessDate;
            this.StartExecutionDate = startExecutionDate;
            this.EndExecutionDate = endExecutionDate;
            this.Item = item;
        }

        public string Category {
            get;
            set;
        }

        public string User {
            get;
            set;
        }

        public DateTime? StartBusinessDate {
            get;
            set;
        }

        public DateTime? EndBusinessDate {
            get;
            set;
        }

        public DateTime? StartExecutionDate {
            get;
            set;
        }

        public DateTime? EndExecutionDate {
            get;
            set;
        }

        public long? Item {
            get;
            set;
        }
    }
}

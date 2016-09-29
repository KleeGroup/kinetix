using System;

namespace Kinetix.Audit
{
    public class AuditTraceCriteria
    {

        public AuditTraceCriteria(
              string category,
              string username,
              DateTime? startBusinessDate,
              DateTime? endBusinessDate,
              DateTime? startExecutionDate,
              DateTime? endExecutionDate,
              int? item) {
            this.Category = category;
            this.Username = username;
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

        public string Username {
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

        public int? Item {
            get;
            set;
        }
    }
}

using System;

namespace Kinetix.Audit
{
    public class AuditTraceCriteria
    {
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

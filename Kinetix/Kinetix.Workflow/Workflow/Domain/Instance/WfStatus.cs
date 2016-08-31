using Kinetix.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Workflow.instance
{

    /// <summary>
    /// This class defines the Status.
    /// </summary>
    /// 
    [Table("WF_STATUS")]
    public class WfStatus
    {

        [Column("WFS_CODE")]
        [Domain("DO_X_WORKFLOW_CODE")]
        [Key]
        public int? WfsCode
        {
            get;
            set;
        }

        [Column("WFS_LABEL")]
        [Domain("DO_X_WORKFLOW_LABEL")]
        public int? WfsLabel
        {
            get;
            set;
        }
        
    }
}

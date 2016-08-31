using Kinetix.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Workflow.model
{
    /// <summary>
    /// This class defines the Multiplicity Definition.
    /// </summary>
    /// 
    [Table("WF_MULTIPLICITY_DEFINITION")]
    public class WfMultiplicityDefinition
    {

        [Column("WFMD_ID")]
        [Domain("DO_X_WORKFLOW_CODE")]
        [Key]
        public string Code
        {
            get;
            set;
        }


        [Column("LABEL")]
        [Domain("DO_X_WORKFLOW_LABEL")]
        public string Label
        {
            get;
            set;
        }

    }
}

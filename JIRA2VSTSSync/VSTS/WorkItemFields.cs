using System;
using System.Collections.Generic;
using System.Text;

namespace JIRA2VSTSSync.VSTS
{
    public class WorkItemFields
    {
        public int count { get; set; }
        public WorkItemField[] value { get; set; }
    }
}

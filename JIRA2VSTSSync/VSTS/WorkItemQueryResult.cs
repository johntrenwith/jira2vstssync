using System;

namespace JIRA2VSTSSync.VSTS
{
    public class WorkItemQueryResult
    {
        public string queryType { get; set; }
        public string queryResultType { get; set; }
        public DateTime asOf { get; set; }
        public Column[] columns { get; set; }
        public Workitem[] workItems { get; set; }
    }
}

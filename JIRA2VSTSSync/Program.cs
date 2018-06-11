using JIRA2VSTSSync.JIRA;
using JIRA2VSTSSync.VSTS;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace JIRA2VSTSSync
{
    public class Program
    {
        public static IConfiguration Configuration { get; set; } 

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
            string vstsUrl = Configuration["VSTS:URL"];
            string vstsToken = Configuration["VSTS:Token"];
            string vstsProject = Configuration["VSTS:Project"];
            string vstsQuery = Configuration["VSTS:Query"];
            string vstsJiraKeyField = Configuration["VSTS:JIRAKeyField"];
            string jiraUrl = Configuration["JIRA:URL"];
            string jiraProject = Configuration["JIRA:Project"];
            string jiraUsername = Configuration["JIRA:Username"];
            string jiraPassword = Configuration["JIRA:Password"];

            Dictionary<string, int> keys = new Dictionary<string, int>();

            VSTSService vstsService = new VSTSService();

            string result = vstsService.GetWorkItemsByQuery(vstsUrl, vstsToken, vstsProject, vstsQuery);
            dynamic workItems = JObject.Parse(result);

            foreach (var workItem in workItems.value)
            {
                keys.Add(workItem.fields[vstsJiraKeyField].ToString(), 
                    Convert.ToInt32(workItem.fields["System.Id"].ToString()));
            }

            JIRAService jiraService = new JIRAService();
            string response = jiraService.GetUnresolvedIssuesByProject(jiraUrl,
                jiraProject, jiraUsername, jiraPassword);

            dynamic responseJson = JObject.Parse(response);

            foreach (var issue in responseJson.issues)
            {
                try
                {
                    string key = issue.key.ToString();
                    string title = issue.fields.summary.ToString();
                    // include the JIRA key in the workitem title if it's not already there
                    title = title.Contains(key) ? title : key + " " + title;

                    string description = issue.fields.description.ToString();
                    string issueType = issue.fields.issuetype.name.ToString();
                    string assignedTo = issue.fields.assignee.displayName.ToString();
                    string status = issue.fields.status.statusCategory.name.ToString();
                    int id = 0;

                    if (keys.ContainsKey(key))
                    {
                        id = keys.GetValueOrDefault(key);
                    }
                
                    Console.WriteLine("Creating/updating VSTS work item " + issueType + " for JIRA issue " + key);
                    VSTSService service = new VSTSService();
                    service.CreateUpdateWorkItem(id, vstsUrl, "ICT Solutions Delivery", vstsToken, issueType,
                        title, description, key, assignedTo, status);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message + " - " + ex.StackTrace);
                }

            }

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }        
    }
}

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace JIRA2VSTSSync.JIRA
{
    public class JIRAService
    {
        public string GetUnresolvedIssuesByProject(string url, string project, string username, string password)
        {
            string response = "";
            using (HttpClient request = new HttpClient())
            {
                var jiraUrl = url + "/rest/api/2/search?jql=project%20=%20" + project + 
                    "%20AND%20resolution%20=%20Unresolved";
                request.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Basic",
                            Convert.ToBase64String(
                                System.Text.ASCIIEncoding.ASCII.GetBytes(
                                    string.Format("{0}:{1}", username, password))));


                Task.Run(function: async () =>
                {
                    response = await request.GetStringAsync(jiraUrl);
                }).GetAwaiter().GetResult();
            }

            return response;
        }
    }
}

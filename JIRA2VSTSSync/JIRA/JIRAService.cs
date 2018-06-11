using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace JIRA2VSTSSync.JIRA
{
    public class JIRAService
    {
        public string GetIssuesForCurrentSprint(string url, int boardId, string username, string password)
        {
            return RunApiQuery(url + "/rest/agile/1.0/board/" + boardId.ToString() + 
                "/sprint?state=active", username, password);
        }

        public string GetIssuesForSprint(string url, int boardId, int sprintId, string username, string password)
        {
            return RunApiQuery(url + "/rest/agile/1.0/board/" + boardId.ToString() +
                "/sprint/" + sprintId.ToString() + "/issue", username, password);
        }

        public string GetUnresolvedIssuesByProject(string url, string project, string username, string password)
        {
            return RunApiQuery(url + "/rest/api/2/search?jql=project%20=%20" + project +
                    "%20AND%20resolution%20=%20Unresolved", username, password);
        }

        private string RunApiQuery(string url, string username, string password)
        {
            string credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password)));
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                var request = new HttpRequestMessage(new HttpMethod("GET"), url);
                var response = client.SendAsync(request).Result;
                var result = response.Content.ReadAsStringAsync().Result;
                return result;
            }            
        }
    }
}

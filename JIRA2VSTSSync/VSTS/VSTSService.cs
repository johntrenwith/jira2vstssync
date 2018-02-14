using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JIRA2VSTSSync.VSTS
{    
    public class VSTSService
    {
        public string GetWorkItemsByQuery(string url, string token, string project, string query)
        {
            string _credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", token)));
            var result = "";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                HttpResponseMessage queryHttpResponseMessage = client.GetAsync(project + "/_apis/wit/queries/" + query + "?api-version=2.2").Result;

                // Note - the extension method "ReadAsJsonAsync" is used here because the ReadAsAsync method
                // om HttpContent is not yet supported in .NET Core. See http://nodogmablog.bryanhogan.net/2017/10/httpcontent-readasasync-with-net-core-2/
                if (queryHttpResponseMessage.IsSuccessStatusCode)
                {
                    //bind the response content to the queryResult object
                    QueryResult queryResult = queryHttpResponseMessage.Content.ReadAsJsonAsync<QueryResult>().Result;
                    string queryId = queryResult.id;

                    //using the queryId in the url, we can execute the query
                    HttpResponseMessage httpResponseMessage = client.GetAsync(project + "/_apis/wit/wiql/" + queryId + "?api-version=2.2").Result;

                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        WorkItemQueryResult workItemQueryResult = httpResponseMessage.Content.ReadAsJsonAsync<WorkItemQueryResult>().Result;

                        //now that we have a bunch of work items, build a list of id's so we can get details
                        var builder = new System.Text.StringBuilder();
                        foreach (var item in workItemQueryResult.workItems)
                        {
                            builder.Append(item.id.ToString()).Append(",");
                        }

                        //clean up string of id's
                        string ids = builder.ToString().TrimEnd(new char[] { ',' });
                        string asOf = workItemQueryResult.asOf.ToString("s");
                        HttpResponseMessage getWorkItemsHttpResponse = 
                            client.GetAsync("_apis/wit/workitems?ids=" + ids + 
                            "&fields=System.Id,System.Title,System.State,StJohnAgile.JIRAKey&asOf=" +
                            asOf + "&api-version=2.2").Result;
                        result = getWorkItemsHttpResponse.Content.ReadAsStringAsync().Result;
                        
                    }
                }
            }

            return result;
        }

        public void CreateUpdateWorkItem(int id, string url, string project, string pat, string type, string title,
            string description, string jiraKey, string assignedTo, string status)
        {
            string personalAccessToken = pat;
            string credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalAccessToken)));
            string workItemType = "";
            string state = status;
            string reason = "Moved to state " + status;
            
            // Handle cases where we need to map JIRA issue type to VSTS work item type.
            switch (type)
            {
                case "Story":
                    workItemType = "User Story";
                    break;
                case "Defect":
                    workItemType = "Bug";
                    break;
                default:
                    workItemType = "Task";
                    break;
            }

            // Handle cases where we need to map JIRA status to VSTS state. Some modification
            // may be required here if the project uses custom states.
            switch (status)
            {
                case "In Progress":
                    state = "Active";
                    reason = "Implementation started";
                    break;
                case "Done":
                    state = "Closed";
                    reason = "Acceptance tests pass";
                    break;
                default:
                    state = "New";
                    break;
            }

            Object[] patchDocument = new Object[6];

            patchDocument[0] = new { op = "add", path = "/fields/System.WorkItemType", value = workItemType };
            patchDocument[1] = new { op = "add", path = "/fields/System.Title", value = title };
            patchDocument[2] = new { op = "add", path = "/fields/System.Description", value = description };
            patchDocument[3] = new { op = "add", path = "/fields/System.AssignedTo", value = assignedTo };
            patchDocument[4] = new { op = "add", path = "/fields/StJohnAgile.JIRAKey", value = jiraKey };
            patchDocument[5] = new { op = "add", path = "/fields/System.State", value = state };
            
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                var patchValue = new StringContent(JsonConvert.SerializeObject(patchDocument), Encoding.UTF8, "application/json-patch+json");

                var method = new HttpMethod("PATCH");
                url = url + (id == 0 ? "/" + project + "/_apis/wit/workitems/$Bug?api-version=2.2"
                    : "/_apis/wit/workitems/" + id + "?api-version=2.2");

                var request = new HttpRequestMessage(method, url) { Content = patchValue };
                var response = client.SendAsync(request).Result;
                var result = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Successfully " + (id > 0 ? "updated" : "added") + 
                        " work item for JIRA issue " + jiraKey);
                }
                else
                {
                    dynamic resultJson = JObject.Parse(result);
                    string message = resultJson.message.ToString();
                    Console.WriteLine("Failed to " + (id > 0 ? "update" : "add") +
                        " work item for JIRA issue " + jiraKey + ". Error was: " + message);
                }
            }
           
        }
    }
}

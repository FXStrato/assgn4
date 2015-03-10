using DawgSharp;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;

namespace WebRole1
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        private static TTrie<string> trie = new TTrie<string>();
        private PerformanceCounter cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private static PerformanceCounter ram = new PerformanceCounter("Memory", "Available MBytes");
        private Dictionary<string, List<string>> cache = new Dictionary<string, List<string>>();


        /*[WebMethod]
        public string getPlayer(string name)
        {
            if (cache.ContainsKey(name))
            {
                return cache[name];
            }
            else
            {
                name = name.Trim();
                string ins = name;
                name = name.Replace(" ", "+");
                WebClient client = new WebClient();
                string insert = "http://ec2-54-148-15-124.us-west-2.compute.amazonaws.com/updatedcall.php?txtSearch=" + name;
                var json = client.DownloadString(insert);
                cache.Add(ins, json);
                return json;
            }
        }

       [WebMethod]
        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
        public void getdatPlayer(string callback, string player)
        {
            player = player.Replace(" ", "+");
            string jsondata = new WebClient().DownloadString("http://ec2-54-148-15-124.us-west-2.compute.amazonaws.com/updatedcall.php?txtSearch=" + player);
            string results = callback + "(" + jsondata + ");";

            Context.Response.Clear();
            Context.Response.ContentType = "application/json";
            Context.Response.Write(results);
            Context.Response.End();
        } */


        [WebMethod]
        public string buildTrie()
        {
            
            string i = "";
            //C:\\Users\\Jeff\\Documents\\Visual Studio 2013\\Projects\\asgn4\\WebRole1\\
            StreamReader streamReader = new StreamReader(blob());
            float mem = ram.NextValue();
            int counter = 0;
            while (mem > 50 && streamReader.EndOfStream == false)
            {
                try
                {
                    string text = streamReader.ReadLine();
                    trie.Add(text.ToLower(), text.ToLower());
                    i = text;
                    counter++;
                    if (counter % 1000 == 0)
                    {
                        mem = ram.NextValue();
                    }
                }
                catch
                {
                    return "Trie is not accessible.";
                }
            }
            streamReader.Close();
            return "Success. Stopped at " + i + " with memory at " + mem;
        }

        [WebMethod]
        public string blob()
        {
            var filePath = "";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["connectionstring"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("blob");
            if (container.Exists())
            {
                CloudBlockBlob blob = container.GetBlockBlobReference("wiki.txt");
                /// Save blob contents to a file.
                filePath = Server.MapPath("") + "\\wiki.txt";
                blob.DownloadToFile(filePath, FileMode.Create);
            }
            return filePath;
        }


       

        //GET THIS WORKING. THIS WILL USE LINQ TO RETURN A LIST OF URLS WHICH MATCH THE KEYWORDS
        //PASSED IN. IF THE KEYWORD IS A PLAYER, 
        [WebMethod]
        public List<string> newsearch(string prefix)
        {
            if (cache.Count > 100)
            {
                cache.Clear();
            }
            if (cache.ContainsKey(prefix))
            {
                return cache[prefix];
            }
            //Get all the possible partitions by first splitting the passed in string by spaces.
            //Then after getting the partitions, loop through them and check them using LINQ to find
            //which one contains the most urls. Stick them into the List as they show up and then return
            //the top 15 most relevant.
            List<string> urls = new List<string>();
            List<string> titles = new List<string>();
            List<asgn3.Node> nodelist = new List<asgn3.Node>(); //This is the list to store all of the nodes that come back from the keyword searches
            string[] keywords = prefix.Split(new char[] { '.', '?', '!', ' ', ';', ':', ',', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            CloudTable table = getTable();
            for (int i = 0; i < keywords.Length; i++)
            {
                TableQuery<asgn3.Node> query = new TableQuery<asgn3.Node>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, keywords[i]));

                // Insert each node into the list. This could potentially be a lot... Should we have a limit?
                foreach (asgn3.Node entity in table.ExecuteQuery(query))
                {

                    urls.Add(entity.RowKey);
                    titles.Add(entity.title);
                }
            }
            List<string> returningurls = new List<string>();
            int[] keycount = new int[urls.Count];
            int max = 0;
            //After getting the list of nodes, need to now LINQ to find out which nodes have the most keywords.
            for (int i = 0; i < urls.Count; i++)
            {
                var count = keywords.Count(s => urls[i].ToLowerInvariant().Contains(s.ToLowerInvariant()));
                if (max < count)
                {
                    max = count;
                }
                keycount[i] = count;
            }
            int maxcount = 0;
            while (max != -1 && maxcount < urls.Count && maxcount < 20)
            {
                for (int i = 0; i < urls.Count; i++)
                {
                    if (keycount[i] == max)
                    {
                        if (maxcount > 20)
                        {
                            cache.Add(prefix, returningurls);
                            return returningurls;
                        }
                        var decoded = HttpUtility.UrlDecode(urls[i]);
                        returningurls.Add(decoded + "§" + titles[i]);
                        maxcount++;
                    }
                }
                
                max--;     
            }
            cache.Add(prefix, returningurls);
            return returningurls;
        }

        //EVERYTHING PAST THIS IS OLD PA3 METHODS. SOME REQUIRE CHANGING, LIKE TABLE RETRIEVAL
        //---------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------
        

        //Don't forget to make the QCheck first.
        [WebMethod]
        public string CrawlerStatus()
        {
            string temp = "";
            CloudTable table = getTable();
            TableOperation retrieveOperation = TableOperation.Retrieve<asgn3.QCheck>("end", "spider1");
            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                temp += ((((asgn3.QCheck)retrievedResult.Result).status));
            }
            else
            {
                temp += ("Uninitialized");
            }
            return temp;
        }

        [WebMethod]
        public List<string> initstart()
        {
            CloudQueue queue = getQueue();
            queue.Clear();
            asgn3.QCheck node = new asgn3.QCheck("spider1", "", "idle");
            CloudTable table = getTable();
            TableOperation insertOperation = TableOperation.InsertOrReplace(node);
            table.ExecuteAsync(insertOperation);
            return getStats();
        }

        [WebMethod]
        public string initializeSpider(string url)
        {
            asgn3.QCheck node = new asgn3.QCheck("spider1", url, "start");
            CloudTable table = getTable();
            TableOperation insertOperation = TableOperation.InsertOrReplace(node);
            table.ExecuteAsync(insertOperation);
            return "Initialized crawler with " + url;
        }

        [WebMethod]
        //Fix this to check via keywords. This is still running the old PA3 style of getting 
        //the related one from asgn partition.
        public List<string> GetFromTable(string url)
        {
            List<string> temp = new List<string>();
            string encodeurl = HttpUtility.UrlEncode(url);
            CloudTable table = getTable();
            TableOperation retrieveOperation = TableOperation.Retrieve<asgn3.Node>("asgn", encodeurl);
            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                temp.Add(url);
                temp.Add(((((asgn3.Node)retrievedResult.Result).title)));
                temp.Add(((((asgn3.Node)retrievedResult.Result).date)));
                return temp;
            }
            else
            {
                return temp;
            }

        }

        [WebMethod]
        public string endSpider()
        {
            asgn3.QCheck node = new asgn3.QCheck("spider1", "", "stop");
            CloudTable table = getTable();
            TableOperation insertOperation = TableOperation.InsertOrReplace(node);
            table.ExecuteAsync(insertOperation);
            return "idle";
        }

        [WebMethod]
        public string cpuram()
        {
            float temp = this.cpu.NextValue();
            Thread.Sleep(1000);
            temp = this.cpu.NextValue();
            return ram.NextValue() + "|" + temp;
        }

        [WebMethod]
        public List<string> getStats()
        {
            List<string> temp = new List<string>();
            CloudTable table = getTable();
            TableOperation retrieveOperation = TableOperation.Retrieve<asgn3.dashboard>("dash", "stats");
            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                asgn3.dashboard board = ((asgn3.dashboard)retrievedResult.Result);
                temp.Add(board.urlcount.ToString());
                temp.Add(board.urls);
                temp.Add(board.queuesize.ToString());
                temp.Add(board.tablesize.ToString());
                temp.Add(board.errors.ToString());
                temp.Add(board.titlecount.ToString());
                temp.Add(board.lasttitle);
                return temp;
            }
            else
                return temp;
        }

        [WebMethod]
        public List<string> initdash()
        {
            CloudTable table = getTable();
            asgn3.dashboard replace = new asgn3.dashboard(0, "", 0, 0, "", 0, "");
            TableOperation insertOperation = TableOperation.InsertOrReplace(replace);
            table.ExecuteAsync(insertOperation);
            return getStats();
        }

        public static CloudQueue getQueue()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["connectionstring"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("asgn4");
            queue.CreateIfNotExists();
            return queue;
        }
        public static CloudTable getTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["connectionstring"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("asgn4");
            table.CreateIfNotExists();
            return table;
        }
        
        //EVERYTHING PAST THIS IS PA2 STUFF.
        //---------------------------------------------------------------------
        //---------------------------------------------------------------------
        //---------------------------------------------------------------------

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<string> oldsearch(string str)
        {
            if (str != "")
            {
                List<string> to = new List<string>();
                str = str.ToLower();
                List<string> original = trie.PrefixMatch(str).ToList();
                if (original.Count < 1) return new List<string>();
                for (int i = 0; i < 15; i++)
                {
                    to.Add(original[i].ToLower());
                }
                return to;
            }
            else
            {
                return new List<string>();
            }
        }
    }
}

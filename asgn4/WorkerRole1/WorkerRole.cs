using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Queue;
using asgn3;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        public Spider crawler = new Spider();
        private bool isStarted = false;
        public CloudTable table = getTable();

        public override void Run()
        {


            try
            {
                //Basically, need to check to see if spiders should be running. Depending on what's in the table, it will take the url and then initialize itself.
                //If it says stop, then it should kill the spider. If it says idle, dont do anything; if it says crawling, dont do anything.
                //If it says start, then it should initialize the spider and set the condition to working. 
                while (true)
                {
                    Thread.Sleep(2000);
                    checkCrawler();
                    if (isStarted)
                    {
                        Thread.Sleep(2000);
                        getstatus();
                    }
                }
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            //ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            //Trace.TraceInformation("WorkerRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }

        private void checkCrawler()
        {
            CloudTable table = getTable();
            QCheck temp;
            TableOperation retrieveOperation = TableOperation.Retrieve<QCheck>("end", "spider1");
            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);
            temp = ((QCheck)retrievedResult.Result);

            if (temp != null && temp.status == "stop")
            {
                QCheck replace = new QCheck("spider1", "", "idle");
                TableOperation insertOperation = TableOperation.InsertOrReplace(replace);
                table.ExecuteAsync(insertOperation);
                crawler.End();
                isStarted = false;
            }
            else if (temp != null && temp.status == "start")
            {
                crawler.Go(temp.url);
                QCheck replace = new QCheck("spider1", temp.url, "crawling");
                TableOperation insertOperation = TableOperation.InsertOrReplace(replace);
                table.ExecuteAsync(insertOperation);
                isStarted = true;
            }
        }

        private void getstatus()
        {

            CloudTable table = getTable();
            TableQuery<dashboard> query = new TableQuery<dashboard>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "asgn"));
            int urlscount = crawler.urlscount;
            int queuesize = crawler.queuesize;
            int tablesize = 0;
            string temp = "";
            string secondary = "";
            tablesize = Spider.tablesize;
            string lasttitle = crawler.lasttitle;
            int titlecount = crawler.titles.Count;
            if (crawler.inserturls != null)
            {

                for (int i = 0; i < crawler.inserturls.Count; i++)
                {
                    temp += "<p>" + crawler.inserturls[i] + "</p>";
                }
            }
            if (Spider.errors != null)
            {

                for (int i = 0; i < Spider.errors.Count; i++)
                {
                    secondary += "<p>" + Spider.errors[i] + "</p>";
                }
            }

            asgn3.dashboard replace = new dashboard(urlscount, temp, queuesize, tablesize, secondary, titlecount, lasttitle);
            TableOperation insertOperation = TableOperation.InsertOrReplace(replace);
            table.ExecuteAsync(insertOperation);
        }

        private static CloudTable getTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["connectionstring"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("asgn4");
            table.CreateIfNotExists();
            return table;
        }

        private static CloudQueue getQueue()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["connectionstring"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("asgn4");
            queue.CreateIfNotExists();
            return queue;
        }
    }
}

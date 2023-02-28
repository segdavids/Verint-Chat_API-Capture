using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Configuration;
using System.IO;

namespace _15Mins_Service
{
    internal class Program
    {
       
       
        static void Main(string[] args)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["VerintWFM"].ConnectionString);
            string datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            
            string reportfolder;
            string Query = "select * from Report_Path";
            DataTable pathdt = GetWFM(Query);
            reportfolder = pathdt.Rows[0]["Path"].ToString();

            string FileNamePart = DateTime.Now.ToString("yyyyMMddHHmm"); ;//Datetime will be added to it
            string DestinationFolder = reportfolder; // @"K:\EF_RAW_DATA\";
            string FileDelimiter = "\t"; //You can provide comma or pipe or whatever you like
            string FileExtension = ".txt"; //Provide the extension you like such as .txt or .csv

            //DateTime final = temp+temptime;
            conn.Open();
            //DateTime datenow = DateTime.Now; //.ToString("yyy-MM-dd HH:mm");
            //string finaldate = string.Empty;
            //if((datenow.Hour.ToString()=="0") && (datenow.Minute.ToString()=="0"))
            //{
            //    datenow = datenow.AddDays(-1);                
            //}
            //finaldate = datenow.ToString("yyyy-MM-dd HH:mm");


            //DO NOT CHANGE THE DATETIME FOR GDATE - 10-8-2022 REMEMBER THE RROR THAT GENERATED FILES FOR ONE STATIC DATE ND CREATED PROBLEMS
            string q = $"BEGIN DECLARE @gdate DATETIME, @datetime DATETIME, @reportstartime datetime, @service_level_threshold int BEGIN  SELECT @gdate = convert(varchar(16),getdate(), 21) select @reportstartime = CAST('00:15' AS datetime) SELECT @datetime = DATEADD(mi, -15, @gdate) Select @service_level_threshold = 90 SELECT ReportDate, TimeInterval, Queue, totatchats as Chats, Replied as Replied,cast(cast((ISNULL((slaanswered * 1.0 / NULLIF(totatchats, 0)) * 100, 0)) as decimal(5, 2)) as float) AS SL, ISNULL((WaitTime / NULLIF(Replied, 0)), 0) as ASA, ISNULL((ChatDuration / NULLIF(Replied, 0)), 0) AS AHT, staff, Abd as Abd FROM (Select(CONVERT(VARCHAR(5), (CAST(@gdate as datetime) - @reportstartime), 108)) + '-' + (CONVERT(VARCHAR(5), (CAST(@gdate as datetime)), 108)) as TimeInterval, qd.queue_id as Queue,q.service_level_type as sltype,Convert(Varchar, CAst(@datetime as Date), 101) as ReportDate,CONVERT(VARCHAR(5), (CAST(@gdate as datetime)), 108) as Report_Time,CONVERT(VARCHAR(5), (CAST(@gdate as datetime)), 108) + '-' + CONVERT(VARCHAR(5), (CAST(@gdate as datetime) - @reportstartime), 108) as Time_Interval,count(distinct(qd.conversation_id)) as totatchats,count(distinct(qd.agent_id)) as staff,sum(case when((qd.enqueue_time is not null) and(qd.start_time is null) and(qd.end_time is not null) and qd.ended_by in ('customer', 'network', 'bot')) then 1 else 0 end) as Abd, sum(case when qd.enqueue_time is not null and qd.start_time is not null and qd.agent_id is not null then 1 else 0 end) as Replied,   sum(DATEDIFF(second, CASE WHEN qd.enqueue_time IS NOT NULL then qd.enqueue_time end, qd.start_time)) AS WaitTime, SUM(DATEDIFF(SECOND, CASE WHEN qd.start_time IS NOT NULL then qd.start_time end, qd.end_time)) AS ChatDuration, ISNULL(sum(case when qd.enqueue_time is not null and qd.start_time is null and qd.end_time is not null and qd.ended_by in ('customer', 'network', 'bot') and DATEDIFF(second, qd.enqueue_time, qd.end_time) <= @service_level_threshold then 1 else 0 end),0) as slaabandoned, ISNULL(sum(case when qd.enqueue_time is not null and qd.agent_id is not null and DATEDIFF(SECOND, qd.enqueue_time, qd.start_time) <= @service_level_threshold then 1 else 0 end),0) as slaanswered from[EFHybridchat].[dbo].[Queue_Chat_Details] as qd INNER JOIN[EFHybridchat].[dbo].[Queues] as q ON qd.queue_id = q.queue_id where qd.session_start_time between @datetime and @gdate and conversation_id not in (select subq.conversation_id from[EFHybridchat].[dbo].[Queue_Chat_Details] as subq where subq.session_start_time between @datetime and @gdate and subq.ended_by in ('RONA')) GROUP BY  qd.queue_id,q.service_level_type) t END END update[Verint_Connector].[dbo].[LastUpdate] set LastReportdate = getdate(), Type = 'System'";
          
            SqlCommand scmd = new SqlCommand(q, conn);

            SqlDataAdapter da = new SqlDataAdapter(scmd);
            DataTable d_table = new DataTable();
            d_table.Load(scmd.ExecuteReader());
            //Prepare the file path 
            string FileFullPath = DestinationFolder + "\\" + FileNamePart  + FileExtension;

            StreamWriter sw = null;
            sw = new StreamWriter(FileFullPath, false);

            // Write the Header Row to File
            int ColumnCount = d_table.Columns.Count;
            for (int ic = 0; ic < ColumnCount; ic++)
            {
                sw.Write(d_table.Columns[ic]);
                if (ic < ColumnCount - 1)
                {
                    sw.Write(FileDelimiter);
                }
            }
            sw.Write(sw.NewLine);

            // Write All Rows to the File
            if (d_table.Rows.Count > 0)
            {
                // Write All Rows to the File
                foreach (DataRow dr in d_table.Rows)
                {
                    for (int ir = 0; ir < ColumnCount; ir++)
                    {
                        if (!Convert.IsDBNull(dr[ir]))
                        {
                            sw.Write(dr[ir].ToString());
                        }
                        if (ir < ColumnCount - 1)
                        {
                            sw.Write(FileDelimiter);
                        }
                    }
                    sw.Write(sw.NewLine);
                }
            }
            else
            {
                string reportdate = DateTime.Now.ToString("MM/dd/yyyy");
                string totime =  DateTime.Now.ToString("HH:mm"); ;// DateTime.ParseExact(fromtime, "HH:mm",
                string fromtime = DateTime.Now.AddMinutes(-15).ToString("HH:mm");
             
                string timeinterval = fromtime + "-" + totime;
                string queue = "3008";

                DataTable dt = new DataTable();
                dt.Columns.AddRange(new DataColumn[10]
                {
                        new DataColumn("ReportDate", typeof(string)),
                        new DataColumn("TimeInterval", typeof(string)),
                        new DataColumn("Queue", typeof(string)),
                        new DataColumn("Chats", typeof(string)),
                        new DataColumn("Replied", typeof(string)),
                        new DataColumn("SL", typeof(string)),
                        new DataColumn("ASA", typeof(string)),
                        new DataColumn("AHT", typeof(string)),
                        new DataColumn("staff", typeof(string)),
                        new DataColumn("Abd", typeof(string))

                });
                dt.Rows.Add(reportdate, timeinterval, queue, "0", "0", "0", "0", "0", "0", "0");

                // Write All Rows to the File
                foreach (DataRow dr in dt.Rows)
                {
                    for (int ir = 0; ir < ColumnCount; ir++)
                    {
                        if (!Convert.IsDBNull(dr[ir]))
                        {
                            sw.Write(dr[ir].ToString());
                        }
                        if (ir < ColumnCount - 1)
                        {
                            sw.Write(FileDelimiter);
                        }
                    }
                    sw.Write(sw.NewLine);
                }
            }
            sw.Close();
        }
        public static DataTable GetWFM(string Query)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["VerintWFM"].ConnectionString))
                {

                    using (SqlDataAdapter sda = new SqlDataAdapter(Query, conn))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        return dt;

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //public void GetReportPath()
        //{
        //    try
        //    {
        //        //  DateTime lastreport = DateTime.Now.AddDays(-1);
        //        string Query = "select * from Report_Path";
        //        DataTable pathdt = GetWFM(Query);
        //        reportfolder = pathdt.Rows[0]["Path"].ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
    }
}

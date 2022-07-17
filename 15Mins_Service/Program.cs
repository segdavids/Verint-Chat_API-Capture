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
            string q = @"BEGIN DECLARE @gdate DATETIME, @datetime DATETIME, @reportstartime datetime

BEGIN
        SELECT @gdate = convert(varchar(16), getdate(), 21)

    select @reportstartime = CAST('00:15' AS datetime)

    SELECT @datetime = DATEADD(mi, -15, @gdate)


SELECT ReportDate, TimeInterval, Queue, totatchats as Chats, Replied as Replied,
 CASE
 WHEN sltype = 1

    THEN ISNULL((slaanswered*1.0 / NULLIF((slatotal - slaabandoned), 0)) *100,0) 
 WHEN sltype = 3

    THEN ISNULL(((slaanswered +slaabandoned) *1.0 / NULLIF((slatotal), 0)) *100,0)
 WHEN sltype = 2

    THEN cast(cast((ISNULL((slaanswered* 1.0 / NULLIF(slatotal, 0)) *100,0))as decimal(5, 2))as float)
 else
                0
 END AS SL, ISNULL((WaitTime / Replied), 0) as ASA, ISNULL((ChatDuration / Replied), 0) AS AHT, staff, Abd as Abd
 FROM
 (

 Select(CONVERT(VARCHAR(5), (CAST(@gdate as datetime) - @reportstartime), 108)) + '-' + (CONVERT(VARCHAR(5), (CAST(@gdate as datetime)), 108)) as TimeInterval,
qd.queue_id as Queue,
 q.service_level_type as sltype,
CONVERT([varchar], @gdate, 101) as ReportDate, 
 CONVERT(VARCHAR(5), (CAST(@gdate as datetime)), 108) as Report_Time, 
 CONVERT(VARCHAR(5), (CAST(@gdate as datetime)), 108) + '-' + CONVERT(VARCHAR(5), (CAST(@gdate as datetime) - @reportstartime), 108) as Time_Interval,
 count(distinct(qd.conversation_id)) as totatchats, 
 count(distinct(qd.agent_id)) as staff, 
 sum(case when((qd.enqueue_time is not null) and(qd.start_time is null) and(agent_id is null) and(bot_id is null)) then 1 else 0 end) as Abd, 
 sum(case when((qd.start_time is not null) and(qd.agent_id is not null)) then 1 else 0 end) as Replied, 
  sum(DATEDIFF(second, CASE WHEN qd.session_start_time IS NOT NULL then qd.session_start_time end, qd.start_time)) AS WaitTime,
    SUM(DATEDIFF(SECOND, CASE WHEN qd.start_time IS NOT NULL then qd.start_time end, qd.end_time)) AS ChatDuration,
      ISNULL(sum(case when qd.start_time is null and (CONVERT(datetime, (CAST(qd.end_time as datetime))-CAST(qd.session_start_time as datetime), 108)) <= (CONVERT(datetime, DATEADD(second, q.service_level_threshold, q.service_level_threshold), 108)) then 1 else 0 end) +sum(case when qd.start_time is not null and (CONVERT(datetime, (CAST(qd.start_time as datetime))-CAST(qd.session_start_time as datetime), 108)) <= (CONVERT(datetime, DATEADD(second, q.service_level_threshold, q.service_level_threshold), 108)) then 1 else 0 end),0) as slatotal, 
 ISNULL(sum(case when qd.start_time is null and (CONVERT(datetime, (CAST(qd.end_time as datetime))-CAST(qd.session_start_time as datetime), 108)) <= (CONVERT(datetime, DATEADD(second, q.service_level_threshold, q.service_level_threshold), 108)) then 1 else 0 end),0) as slaabandoned, 
 ISNULL(sum(case when qd.start_time is not null and (CONVERT(datetime, (CAST(qd.start_time as datetime))-CAST(qd.session_start_time as datetime), 108)) <= (CONVERT(datetime, DATEADD(second, q.service_level_threshold, q.service_level_threshold), 108)) then 1 else 0 end),0) as slaanswered

from[EFHybridchat].[dbo].[Queue_Chat_Details] as qd
 INNER JOIN[EFHybridchat].[dbo].[Queues] as q
 ON qd.queue_id = q.queue_id
where qd.session_start_time >= (CONVERT(datetime, (CAST(dateadd(mi, datediff(mi, 0, @gdate), 0) as datetime) - @reportstartime), 120)) and qd.session_start_time <= (CONVERT(datetime, (CAST(dateadd(mi, datediff(mi, 0, @gdate), 0) as datetime)), 120))
GROUP BY  qd.queue_id,q.service_level_type
) t
END
 END
 update[Verint_Connector].[dbo].[LastUpdate] set LastReportdate = getdate(), Type = 'System'";
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

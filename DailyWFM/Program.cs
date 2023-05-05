using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace DailyWFM
{
    internal class Program
    {
        static void Main(string[] args)
        {

            dailysinglefiles();
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

  

        public static void dailysinglefiles()
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["VerintWFM"].ConnectionString);
            DateTime startdate_raw = DateTime.Today.AddDays(-1);
            DateTime end_raw = DateTime.Today.AddMinutes(-15);

            //string startdate = startdate_raw.ToString("yyyy-MM-dd HH:mm");
            //string endtime = end_raw.ToString("yyyy-MM-dd")+" 23:59";


            //Declare Variables and provide values
            string FileNamePart = String.Empty;// datetime_picker.SelectedDate.Value.ToString("yyyyMMdd"); //Datetime will be added to it
            string DestinationFolder= "C:\\EF_RAW_DATA"; // @"C:\EF_RAW_DATA"; // @"K:\EF_RAW_DATA\";
            string FileDelimiter = "\t"; //You can provide comma or pipe or whatever you like
            string FileExtension = ".txt"; //Provide the extension you like such as .txt or .csv

            conn.Open();
            try
            {
                // DateTime starttimecatcher = starttime;
                for (DateTime i = startdate_raw; i <= end_raw; i = i.AddMinutes(15)) //(startdate <= endtime)
                {
                   

                    string q = $"BEGIN \r\nDECLARE @gdate DATETIME, @datetime DATETIME, @reportstartime datetime, @service_level_threshold int \r\nBEGIN \r\nSELECT @gdate = convert(varchar(16), '" + i.ToString("yyyy-MM-dd HH:mm") + "', 21) \r\nselect @reportstartime = CAST('00:15' AS datetime) \r\nSELECT @datetime = DATEADD(mi, -15, @gdate) Select @service_level_threshold = 90 \r\nSELECT ReportDate, TimeInterval, Queue, totatchats as Chats, Replied as Replied,cast(cast((ISNULL((slaanswered * 1.0 / NULLIF(totatchats, 0)) * 100, 0)) as decimal(5, 2)) as float) AS SL, \r\nISNULL((WaitTime / NULLIF(Replied, 0)), 0) as ASA, \r\nISNULL((ChatDuration / NULLIF(Replied, 0)), 0) AS AHT, \r\nstaff, Abd as Abd \r\nFROM \r\n(\r\nSelect(CONVERT(VARCHAR(5), (CAST(@gdate as datetime) - @reportstartime), 108)) + '-' + (CONVERT(VARCHAR(5), (CAST(@gdate as datetime)), 108)) as TimeInterval,\r\nqd.queue_id as Queue, q.service_level_type as sltype,\r\nCONVERT([varchar], @datetime, 101) as ReportDate, \r\nCONVERT(VARCHAR(5), (CAST(@gdate as datetime)), 108) as Report_Time, \r\nCONVERT(VARCHAR(5), (CAST(@gdate as datetime)), 108) + '-' + CONVERT(VARCHAR(5), (CAST(@gdate as datetime) - @reportstartime), 108) as Time_Interval,\r\ncount(distinct(qd.conversation_id)) as totatchats, \r\ncount(distinct(qd.agent_id)) as staff,  \r\nsum(case when((qd.enqueue_time is not null) and (qd.start_time is null) and (qd.end_time is not null) and qd.ended_by in ('customer' , 'network' , 'bot') ) then 1 else 0 end) as Abd,  \r\nsum(case when((qd.enqueue_time is not null) and (qd.start_time is not null) and (qd.agent_id is not null)) then 1 else 0 end) as Replied,   \r\nsum(DATEDIFF(second, CASE WHEN qd.enqueue_time IS NOT NULL then qd.enqueue_time end, qd.start_time)) AS WaitTime,\r\nSUM(DATEDIFF(SECOND, CASE WHEN qd.start_time IS NOT NULL then qd.start_time end, qd.end_time)) AS ChatDuration, \r\nISNULL(sum(case when qd.enqueue_time is not null and qd.start_time is null and qd.end_time is not null and qd.ended_by in ('customer' , 'network' , 'bot') and DATEDIFF(second,qd.enqueue_time,qd.end_time) <=@service_level_threshold then 1 else 0 end),0) as slaabandoned,  \r\nISNULL(sum(case when (qd.enqueue_time is not null) and (qd.agent_id is not null) and DATEDIFF(SECOND,qd.enqueue_time,qd.start_time)  <=  @service_level_threshold then 1 else 0 end),0) as slaanswered \r\nfrom [EFHybridchat].[dbo].[Queue_Chat_Details] as qd \r\nINNER JOIN [EFHybridchat].[dbo].[Queues] as q ON qd.queue_id = q.queue_id \r\nwhere qd.session_start_time between @datetime and @gdate and conversation_id not in (select subq.conversation_id from[EFHybridchat].[dbo].[Queue_Chat_Details] as subq where subq.session_start_time between @datetime and @gdate and subq.ended_by in ('RONA'))\r\nGROUP BY  qd.queue_id,q.service_level_type) t END END update [Verint_Connector].[dbo].[LastUpdate] set LastReportdate = getdate(), Type = 'Manual', Status = 1";
                    SqlCommand scmd = new SqlCommand(q, conn);
                    FileNamePart = i.ToString("yyyyMMddHHmm");

                    SqlDataAdapter da = new SqlDataAdapter(scmd);
                    DataTable d_table = new DataTable();
                    d_table.Load(scmd.ExecuteReader());
                    //Prepare the file path 
                    string FileFullPath = DestinationFolder + "\\" + FileNamePart + FileExtension;

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
                        string reportdate = i.ToString("MM/dd/yyyy");
                        string totime = i.ToString("HH:mm"); ;// DateTime.ParseExact(fromtime, "HH:mm",
                        string fromtime = i.AddMinutes(-15).ToString("HH:mm");

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


                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }
    }

}
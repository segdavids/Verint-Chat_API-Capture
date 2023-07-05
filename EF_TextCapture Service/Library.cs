using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Data;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Configuration;
using EF_TextCapture_Service;

namespace EF_TextCapture_Service
{

    public static class Library
    {
      //  DateTime timenow = DateTime.Now();

        public static void logger(Exception e)
        {
            StreamWriter loggerman = null;
            try {
               // loggerman = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Log.txt", true);
                loggerman = new StreamWriter(@"C:\EF\Text Capture\Log.txt", true);
                loggerman.WriteLine(DateTime.Now.ToString() + ":" + e.Source.ToString().Trim() + ";" + e.Message.ToString().Trim());
                loggerman.Flush();
                loggerman.Close();
            }
            catch
            { }
        }
        public static DataTable GetRequest(string Query)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["VerintDB"].ConnectionString))
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

        /// <summary>
        /// DEACTIVATE ADVERTISEMENT
        /// </summary>
        /// <param name="Query"></param>
        /// <returns></returns>
        public static string NonQeryRequest(string Query)
        {
            string Statcode = string.Empty;
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["VerintDB"].ConnectionString);
            conn.Open();
            SqlCommand cmd = new SqlCommand(Query, conn);
            int result = cmd.ExecuteNonQuery();
            if (result > 0)
                Statcode = "200";
            else
                Statcode = "400";
            conn.Close();

            return Statcode;
        }
        public static string encryptpass(string password)
        {
            string msg = "";
            byte[] encode = new byte[password.Length];
            encode = Encoding.UTF8.GetBytes(password);
            msg = Convert.ToBase64String(encode);
            return msg;
        }
        static public string decryptpass(string encodedData)

        {
            //string from = Convert.FromBase64String(encodedData);
            byte[] raw = Convert.FromBase64String(encodedData); // unpack the base-64 to a blob
            string s = Encoding.UTF8.GetString(raw);
            return s;

           

        }
        public static void logerror(string messageex)
        {
            StreamWriter custommessage = null;
            string logpath = @"C:\EF\Text Capture\Logs\" + DateTime.Now.ToString("yyyy_MM_dd") + "_Log.txt";
            custommessage = new StreamWriter(logpath, true);
            custommessage.WriteLine(DateTime.Now.ToString() + ":" + messageex);
            custommessage.Flush();
            custommessage.Close();
            Console.WriteLine(messageex);
        }

        public static LLA_Model.verint_interface Post(HC_Model.HC_Interface hcapicall)
        {
            string hcurl;
            LLA_Model.verint_interface retturnmeeesgae = new LLA_Model.verint_interface { };
            try
            {
                DateTime currtime =  DateTime.Now;
                DateTime yesttime = currtime.AddDays(-1);
                //string urlpart = "/conversations/getAllConversations?start_time= " + yesttime.ToString("yyyy-mm-dd") + "%2000:00:01&end_time=" + currtime.ToString("yyyy-mm-dd") + "%2023:11:59";
                string urlpart = "/conversations/getAllConversations?start_time=2021-05-21%2000:00:01&end_time=2021-06-06%2023:11:59";
                var deserializer = new RestSharp.Serialization.Json.JsonDeserializer();
                logerror("Going to get Hybrid Chat URL from DB");
                string query = @"select hc_url from endpoints";// where url_name = 'getconversations'";
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["VerintAPICapture"].ConnectionString))
                using (var cmd = new SqlCommand(query, conn))
                using (SqlDataReader da = (cmd.ExecuteReader()))
                {
                    while (da.Read())
                    {
                        cmd.CommandType = CommandType.Text;
                        hcurl = da["hc_url"].ToString().Trim();
                        string getconvourl =  hcurl+ urlpart;

                        //GET CONVERSTATIONS
                        logerror("Fetching AllConversations from Hybrid Chat MongoDB");
                        var gethcconversations = new RestSharp.RestClient(getconvourl);
                        var getconvorequest = new RestSharp.RestRequest(getconvourl, RestSharp.Method.GET);
                        var hcconversationresponse = gethcconversations.Execute(getconvorequest);
                        int statcode = Convert.ToInt32(hcconversationresponse.StatusCode);
                        if (statcode == 200)
                        {
                            string action = "Conversations fetched successfully";
                            logerror("{action}");
                            var deserializer1 = new RestSharp.Serialization.Json.JsonDeserializer();
                            HC_Model.MainRoot2 cooc = deserializer1.Deserialize<HC_Model.MainRoot2>(hcconversationresponse);
                            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
                            logerror("looping through the conversation IDs");
                            foreach (var looper in cooc.response)
                            {
                                string convid = looper.id;
                                //GET MESSAGES
                                string getmessageurl_part = "/getMessages?conversationId=" + convid + "";
                                string getmessageurl = hcurl+getmessageurl_part;
                                logerror("Going to fetch AllMessages for Conversation_Id: " +convid+" from Hybrid Chat MongoDB");
                                var gethcmessages = new RestSharp.RestClient(getmessageurl);
                                var getmessagesrequest = new RestSharp.RestRequest(getmessageurl, RestSharp.Method.GET);
                                var hcmessagesresponse = gethcconversations.Execute(getmessagesrequest);
                                int GetMessageStatcode = Convert.ToInt32(hcmessagesresponse.StatusCode);
                                if (GetMessageStatcode == 200)
                                {
                                    string info = "Messages fetsched successfully";
                                    logerror("{info}");
                                    var MessageDeserializer = new RestSharp.Serialization.Json.JsonDeserializer();
                                    HC_Model.MainRoot messageObj = MessageDeserializer.Deserialize<HC_Model.MainRoot>(hcconversationresponse);
                                    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
                                    logerror("Fetched the Messages Successfully through the conversation IDs");
                                }
                                else
                                {

                                }
                            }
                        }

                        
                        if (statcode == 201)
                        {


                        }
                        else
                        {
                            retturnmeeesgae = new LLA_Model.verint_interface { };
                        }
                       
                    }
                }
            }
            catch (Exception e)
            {
                retturnmeeesgae.id = e.Message.ToString();
                LLA_Model.errorobj errorguy = new LLA_Model.errorobj();
                errorguy.errorname = e.ToString();
                return retturnmeeesgae;
            }
            return retturnmeeesgae;
        }
    



    }
}

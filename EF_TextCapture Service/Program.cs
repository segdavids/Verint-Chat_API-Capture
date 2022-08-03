using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Data;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Configuration;
using EF_TextCapture_Service;
using System.Net.Http.Headers;
using RestSharp.Authenticators;
using RestSharp;
using Verint.Platform.Security;
using Renci.SshNet;
using Newtonsoft.Json;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

namespace EF_TextCapture_Service
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["VerintDB"].ConnectionString);
             string host ;
             string username ;
            string password;
            string workingdirectory;

            string hcurl;
            //string lla_id = "chat594842024220242@conference-2-standaloneclustera3caf.lab.local";
            //string language = "en-us";
            ////string type = "Conversation";
            //string sourceType = "ExpertFlow HybriChat";
            //string project = "1503806";
            //string channel = "1504806";
            //DateTime startTime;
            //DateTime endTime;
            //string subject = "";
            //int direction = 2;
            //string threadId = "";
            //string datasource = "ACDJABDS";
            //string parentId = string.Empty;
            //// string sourceType = "ExpertFlow HybriChat";
            // string sourceSubType = "Chats";
            //string conversationtype = "InstantMessage";

    LLA_Model.verint_interface retturnmeeesgae = new LLA_Model.verint_interface { };

            try
            {

                string currtime = DateTime.Now.ToString("yyyy-MM-dd");
                string yesttime = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                //string urlpart = "/conversations/getAllConversations?start_time= " + yesttime.ToString("yyyy-mm-dd") + "%2000:00:01&end_time=" + currtime.ToString("yyyy-mm-dd") + "%2023:11:59";
                string urlpart = "/conversations/getAllConversations?start_time=" + yesttime + "%2000:00:01&end_time=" + yesttime + "%2023:23:59";
                var deserializer = new RestSharp.Serialization.Json.JsonDeserializer();
                logerror("Going to get Hybrid Chat URL from DB");
                string query = @"select hc_url from endpoints";// where url_name = 'getconversations'";


                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                using (
                    SqlDataReader da = (cmd.ExecuteReader()))
                {
                    // conn.Open();
                    while (da.Read())
                    {
                        
                        cmd.CommandType = CommandType.Text;
                        hcurl = da["hc_url"].ToString().Trim();
                        string getconvourl = hcurl + urlpart;

                        //GET CONVERSTATIONS
                        logerror("Fetching AllConversations from Hybrid Chat MongoDB");
                        var gethcconversations = new RestSharp.RestClient(getconvourl);
                        var getconvorequest = new RestSharp.RestRequest(getconvourl, RestSharp.Method.GET);
                        var hcconversationresponse = gethcconversations.Execute(getconvorequest);
                        //string statcode = hcconversationresponse.StatusCode.ToString();

                        var statcode = hcconversationresponse;
                        if (statcode.StatusCode == HttpStatusCode.OK)
                        {
                            if (statcode.Content.Length > 5)
                            {
                                string action = "Conversations Fetched successfully";
                                logerror(action);
                                var deserializer1 = new RestSharp.Serialization.Json.JsonDeserializer();
                                var cooc = deserializer1.Deserialize<List<HC_Model.Root2>>(hcconversationresponse);
                                // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
                                logerror("looping through the conversation IDs");
                                int success_count = 0;
                                int fail_count = 0;
                                foreach (var looper in cooc)
                                {
                                    string convid = looper.id;
                                    //GET MESSAGES
                                    string getmessageurl_part = "/getMessages?conversationId=" + convid + "";
                                    string getmessageurl = hcurl + getmessageurl_part;
                                    logerror("Going to fetch All Chats for Conversation_Id: " + convid + " from EF_HybridChat");
                                    var gethcmessages = new RestSharp.RestClient(getmessageurl);
                                    var getmessagesrequest = new RestSharp.RestRequest(getmessageurl, RestSharp.Method.GET);
                                    var hcmessagesresponse = gethcmessages.Execute(getmessagesrequest);
                                    int GetMessageStatcode = Convert.ToInt32(hcmessagesresponse.StatusCode);
                                    if (GetMessageStatcode == 200)
                                    {
                                        string info = "Chats for Conversations_Id: " + looper.id + " fetched successfully";
                                        logerror(info);
                                        var MessageDeserializer = new RestSharp.Serialization.Json.JsonDeserializer();
                                        var messageObj = MessageDeserializer.Deserialize<List<HC_Model.Root>>(hcmessagesresponse);
                                        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
                                        //logerror("Fetched the Messages Successfully through the conversation IDs");
                                        logerror("Creating JSon object for Conversation_Id: " + convid + "");

                                        //CREAT LIST OF ACTOR CLASSES
                                        List<LLA_Model.Actor> ActorList = new List<LLA_Model.Actor>();
                                        //CREAT LIST OF ACTOR UTTERANCES
                                        List<LLA_Model.Utterance> UtteranceList = new List<LLA_Model.Utterance>();
                                        List<string> To = new List<string>();
                                        List<string> forempty = new List<string>();
                                        string receiver;

                                        //CREATE NEW ATTRIBUTE SUBCALSS INSTANCE
                                        LLA_Model.Attributes attributeinst = new LLA_Model.Attributes();
                                        attributeinst.sourceType = "Chat";
                                        attributeinst.sourceSubType = "Chat";

                                        //CALL NEW ROOT OBJECT FOR LLA INSTANCE
                                        LLA_Model.verint_interface ObjClass = new LLA_Model.verint_interface();
                                        ObjClass.id = convid;
                                        ObjClass.language = "en-us";
                                        ObjClass.sourceType = "Chat";//sourceType;
                                        ObjClass.project = "LifeLine Customer Contact Solution";
                                        //ObjClass.channel = looper.channel;
                                        ObjClass.startTime = looper.startTime;
                                        ObjClass.endTime = looper.updatedAt;
                                        ObjClass.subject = "";
                                        ObjClass.direction = 2;
                                        ObjClass.threadId = looper.taskId;
                                        ObjClass.datasource = "Text";
                                        ObjClass.parentId = convid;

                                        foreach (var actor in messageObj)
                                        {
                                            LLA_Model.Actor actorsids = new LLA_Model.Actor();
                                            //SETTING THE DYNAMIC PART OF ACTORS CLASS INSTANCE
                                            {
                                                actorsids.id = actor.from.id.ToLower();
                                                actorsids.email = "";
                                                actorsids.accountId = actor.from.id.ToLower();
                                                string finalrole;
                                                switch (actor.from.type.ToLower())
                                                {
                                                    case "bot":
                                                        finalrole = "info";
                                                        break;
                                                    case "customer":
                                                        finalrole = "visitor";
                                                        break;
                                                    case "agent":
                                                        finalrole = "agent";
                                                        break; 
                                                    case "supervisor":
                                                        finalrole = "agent";
                                                        break;
                                                    default:
                                                        finalrole = "";
                                                        break;
                                                }
                                                actorsids.role = finalrole;// actor.from.type.ToLower() == "bot" ? "info" : actor.from.type.ToLower();
                                                actorsids.displayName = HttpUtility.UrlDecode(actor.from.name);
                                                actorsids.timezone = "";
                                                actorsids.enterTime = actor.timestamp;
                                                actorsids.leaveTime = actor.updatedAt;
                                            }
                                            //CONFIRM IF THE ACTOR DOES NOT ALREADY EXIST IN THE LIST OF ACTORS AND PUSH NEW ITEM INTO LIST
                                            bool containsItem = ActorList.Any(item => item.id == actorsids.id);
                                            if (containsItem == false)
                                            {
                                                ActorList.Add(actorsids);
                                            }
                                        }

                                        foreach (var ChatItem in messageObj)
                                        {
                                            //CREATE NEW ACTOR SUBCLASS


                                            //SETTING THE DYNAMIC PART OF UTTERANCE CLASS INSTANCE
                                            //CREATE NEW UTTERANCE SUBCLASS
                                            LLA_Model.Utterance utteranceinst = new LLA_Model.Utterance();

                                            //foreach (var recevierid in ChatItem.to)
                                            //{
                                            //    string id = recevierid.id.ToLower();
                                            //    To.Add(id);
                                            //}
                                            if (ChatItem.to.Count == 0)
                                            {

                                                //if (To.Count == 0)
                                                //{

                                                forempty = ActorList.Where(f => f.role.ToLower() == "agent").Select(n => n.id).ToList();

                                            }
                                            //else
                                            //{
                                            //    receiver = ChatItem.to[0].id.ToLower().ToString();
                                            //    To.Add(receiver);
                                            //}
                                            else
                                            {

                                                receiver = ChatItem.to[0].id.ToLower().ToString();
                                            //    To.Add(receiver);
                                                bool containsItem2 = To.Any(item => item.ToString() == receiver.ToString());

                                                if (containsItem2 == false)
                                                {
                                                    To.Add(receiver);
                                                }

                                            }
                                            utteranceinst.language = "en-us";
                                            utteranceinst.actor = ChatItem.from.id.ToLower().ToLower();

                                            utteranceinst.to = To.Count == 0 ? forempty : To; // ChatItem.to[0].id.ToLower();// To;
                                            utteranceinst.startTime = ChatItem.timestamp;
                                            utteranceinst.startTime = ChatItem.timestamp;
                                            utteranceinst.type = ChatItem.messageType;
                                            utteranceinst.value = ChatItem.text == null ? "This is an activity message" : System.Web.HttpUtility.UrlDecode(ChatItem.text);
                                            utteranceinst.raw_value = ChatItem.text == null ? "This is an activity message" : System.Web.HttpUtility.UrlDecode(ChatItem.text);
                                            // PUSH NEW ITEM INTO LIST
                                            UtteranceList.Add(utteranceinst);

                                            //SETTING THE DYNAMIC PART OF ROOT CLASS INSTANCE
                                            ObjClass.type = "EF-HybridChat" + ChatItem.messageType;
                                            ObjClass.actors = ActorList;
                                            ObjClass.attributes = attributeinst;
                                            ObjClass.utterances = UtteranceList;

                                            //NOW ADDING THE DATA TO RESPECTIVE OBJECT ARRAYS

                                        }
                                        //CREATE A FILE OUT OF THE OBJECT BEFORE SENDING TO SFTP
                                        logerror("Creating JSon file - SFTP Transfer for Conversation_Id: " + convid + "");
                                        using (StreamWriter file = File.CreateText(@"C:\EF\Text Capture\JSONObject\" + convid+"_"+ DateTime.Now.ToString("ddMMyyyy")+".json"))
                                        {
                                            JsonSerializer serializer = new JsonSerializer();
                                            //serialize object directly into file stream
                                            serializer.Serialize(file, ObjClass);
                                        }

                                        //PUSHING TO SFTP FOLDER
                                        logerror("JSon Created successfully, now sending file to SFTP client for Conversation_Id: " + convid + "");
                                        //using (var sftpclient = new SftpClient(host, port, username, password))
                                        //{
                                        //    sftpclient.Connect();
                                        //    if (sftpclient.IsConnected)
                                        //    {
                                        //        logerror("Connected to SFTP: " + convid + "");
                                        //        using (var fileStream = new FileStream(uploadFile, FileMode.Open))
                                        //        {

                                        //            sftpclient.BufferSize = 4 * 1024; // bypass Payload error large files
                                        //            sftpclient.UploadFile(fileStream, Path.GetFileName(uploadFile));
                                        //        }
                                        //        logerror("file sent to SFTP: " + convid + "");
                                        //    }
                                        //    else
                                        //    {
                                        //        logerror("Connection to SFTP server failed, trying again..: " + convid + "");
                                        //    }
                                        //}



                                        //NPW CALLING NTT API TO PUSH THE CHAT TRANSCRIPTS


                                        string LLA_url = "https://sydpvertxr01.iptel.lifeline.org.au/api/recording/textcapture/v1/ingestion";
                                        string test = "ed067050bbc1a63b285e970cf551dce5";
                                        // geo_json geoprop = new geo_json { type = "Feature", properties = "" };
                                        var keyId = "hmm11D1C";
                                        var keyStr = "nCYUvKyXqoc6dEboQCdmO8B94jVY8ySZrVBJWZLRS1s";

                                        var client = new RestSharp.RestClient(LLA_url);
                                        var request = new RestSharp.RestRequest("" + LLA_url + "", RestSharp.Method.POST);
                                        client.UseVwtAuthentication(keyId, keyStr);
                                        // request.AddHeader("UKtLFljf", "B6CEF06DE8BA59FA57ED4F76AC24F56DD6194DB589E414CB5B7E3812FF46944FFEA663CFCB79141DC5A2387A50B045D24360EC7F973D2E9D802B45B1161C21BF");
                                        request.RequestFormat = RestSharp.DataFormat.None;
                                        request.AddJsonBody(ObjClass);

                                        var response = client.Execute(request);
                                        int V_StatCode = Convert.ToInt32(response.StatusCode);
                                        if (V_StatCode == 201)
                                        {
                                            string V_info = "Successfully pushed Chat Transcript for " + convid + "";
                                            logerror(V_info);
                                            string insertquery = "insert into Report_Stat (ConversationId,ChatStartTime,ChatEndTime,ThreadId,status,Error_message,Date_reported) values('" + ObjClass.id + "','" + ObjClass.startTime + "','" + ObjClass.endTime + "','" + ObjClass.threadId + "'," + 1 + ",'N/A','" + DateTime.Now + "')";
                                            SqlCommand activated = new SqlCommand(insertquery, conn);
                                            activated.ExecuteNonQuery();
                                            success_count = success_count + 1;
                                            string s = Newtonsoft.Json.JsonConvert.SerializeObject(ObjClass);
                                            logerror(s);
                                        }
                                        else
                                        {
                                            string errorinstring = response.Content.ToString() == "" ? response.ErrorException.Message.ToString() : response.Content.ToString();
                                            errorinstring = errorinstring.Replace("'", "");
                                            errorinstring= errorinstring.Replace("\"", "");
                                            //string errormessagatrimmed = errormessagatrimmedx.Replace("com.verint.textcapture.model.exception.", "Verint Text Capture Model Exception:");
                                            // string checkquery = "select * from Report_Stat where ConversationId='" + ObjClass.id + "'";
                                            string checkquery = "IF EXISTS (select * from Report_Stat where ConversationId='" + ObjClass.id + "') BEGIN update Report_Stat set status=" + 0 + ",Error_message='" + errorinstring + "',Date_Updated='" + DateTime.Now + "' where ConversationId='" + ObjClass.id + "' END ELSE BEGIN insert into Report_Stat (ConversationId,ChatStartTime,ChatEndTime,ThreadId,status,Error_message,Date_reported) values('" + ObjClass.id + "','" + ObjClass.startTime + "','" + ObjClass.endTime + "','" + ObjClass.threadId + "'," + 0 + ",'" + errorinstring + "','" + DateTime.Now + "') END";

                                            SqlCommand activated = new SqlCommand(checkquery, conn);
                                            activated.ExecuteNonQuery();

                                            fail_count = fail_count + 1;
                                            var s = response.StatusCode.ToString() =="0"? response.ErrorException.Message.ToString(): errorinstring;
                                            logerror(s);

                                            
                                        }

                                    }
                                    else //STAT CODE FOR GETTING MESSAGES FOR EACH CONVERSATION ID IS NOT 200 AKA PASS
                                    {
                                        retturnmeeesgae.id = "System could not get the Messages for ID: " + convid + "";
                                        LLA_Model.errorobj errorguy = new LLA_Model.errorobj();
                                        errorguy.errorname = "System could not get the Messages for ID: " + convid + "";
                                        logerror(errorguy.errorname);
                                    }
                                }
                            }

                        } 
                        else if (statcode.StatusCode != HttpStatusCode.OK)
                        {
                           string errormessage = "Conversations could not be Fetched from EF Hybrid Chat";                        
                            logerror(errormessage);
                        }

                    }
                    conn.Close();
                }
            }
            catch (Exception e)
            {

                string errormessage = e.Message.ToString();
                logerror(errormessage);

                // return retturnmeeesgae;
            }
            // return retturnmeeesgae;

            Environment.Exit(0);
        }
        public static void logger(Exception e)
        {
            StreamWriter loggerman = null;
            try
            {
                // loggerman = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Log.txt", true);
                loggerman = new StreamWriter(@"C:\Text Capture\Log.txt", true);
                loggerman.WriteLine(DateTime.Now.ToString() + ":" + e.Source.ToString().Trim() + ";" + e.Message.ToString().Trim());
                loggerman.Flush();
                loggerman.Close();
            }
            catch
            { }
        }

        public static void logerror(string messageex)
        {
            StreamWriter custommessage = null;
            //custommessage = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\CustomLog.txt", true);
            custommessage = new StreamWriter(@"C:\EF\Text Capture\CustomLog.txt", true);
            custommessage.WriteLine(DateTime.Now.ToString() + ":" + messageex);
            custommessage.Flush();
            custommessage.Close();
        }

        private RestClient InitializeAndGetClient()
        {
            var cookieJar = new CookieContainer();
            var client = new RestClient("http://sydvertxr01/api/recording/textcapture/v1/ingestion")
            {
                Authenticator = new HttpBasicAuthenticator("UKtLFljf", "B6CEF06DE8BA59FA57ED4F76AC24F56DD6194DB589E414CB5B7E3812FF46944FFEA663CFCB79141DC5A2387A50B045D24360EC7F973D2E9D802B45B1161C21BF"),
                CookieContainer = cookieJar
            };

            return client;
        }

    }
}


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
using MongoDB.Driver;
using static EF_TextCapture_Service.HC_Model;
using MongoDB.Bson;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

namespace EF_TextCapture_Service
{
    class Program
    {
        
        public static async Task Main(string[] args)
        {
            string host = String.Empty;
            string username = String.Empty;
            string password = String.Empty;
            string workingdirectory = String.Empty;
            int port = 0;
            //GET PASSWORD
            string get = $"select * from sftp";
            DataTable dt = Library.GetRequest(get);
            if (dt.Rows.Count > 0)
            {
                string temppw = String.IsNullOrEmpty(dt.Rows[0]["password"].ToString()) ? "" : dt.Rows[0]["password"].ToString();
                password = temppw;// Library.decryptpass(temppw);
                username = String.IsNullOrEmpty(dt.Rows[0]["username"].ToString()) ? "" : dt.Rows[0]["username"].ToString();
                host = String.IsNullOrEmpty(dt.Rows[0]["host"].ToString()) ? "" : dt.Rows[0]["host"].ToString();
                port = Convert.ToInt32(String.IsNullOrEmpty(dt.Rows[0]["port"].ToString()) ? "0" : dt.Rows[0]["port"].ToString());
            }


            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["VerintDB"].ConnectionString);
           

            string hcurl;
           

    LLA_Model.verint_interface retturnmeeesgae = new LLA_Model.verint_interface { };

            try
            {
                //string currtime = DateTime.Now.ToString("yyyy-MM-dd");
                string yeststarttime = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") + "T00:00:00.000+10:00";
                string yestendtime = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") + "T23:59:59.999+10:00";
                DateTime finalstarttime = Convert.ToDateTime(yeststarttime);
                DateTime finalendtime = Convert.ToDateTime(yestendtime);
                var starttime = DateTime.SpecifyKind(finalstarttime, DateTimeKind.Utc);// DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                var endtime = DateTime.SpecifyKind(finalendtime, DateTimeKind.Utc);
                //var deserializer = new RestSharp.Serialization.Json.JsonDeserializer();
                string action = "Going to get Hybrid Chat URL from DB";
                Library.logerror(action);
                string query = @"select hc_url from endpoints";
                DataTable querydt = Library.GetRequest(query);
                if (querydt.Rows.Count > 0)
                {
                    hcurl = querydt.Rows[0]["hc_url"].ToString().Trim();
                    action = "Fetching All Conversations from Hybrid Chat MongoDB";
                    //GET CONVERSTATIONS
                    Library.logerror(action);
                    var connect = ConfigurationManager.AppSettings["conmongo"];
                    MongoClient dbClient = new MongoClient("mongodb://10.40.5.30:27017/?directConnection=true");   //("mongodb://10.40.5.30:27017/chatsolution?tlsAllowInvalidCertificates=true&tlsAllowInvalidHostnames=true&directConnection=true");
                    var database = dbClient.GetDatabase("chatsolution");
                    var conversatrioncollection = database.GetCollection<Root2>("conversations");
                    var filterbuilder = Builders<Root2>.Filter;
                    var filter = filterbuilder.Eq(x => x.state, "CLOSED") & filterbuilder.Gte(y => y.createdAt, starttime) & filterbuilder.Lte(z => z.updatedAt, endtime);
                    var post = conversatrioncollection.Find(filter);
                    List<Root2> poster = post.ToList();
                    //var test = post.Find(filter).                   
                    //var gethcconversations = new RestSharp.RestClient(getconvourl);
                    //var getconvorequest = new RestSharp.RestRequest(getconvourl, RestSharp.Method.GET);
                    //var hcconversationresponse = gethcconversations.Execute(getconvorequest);
                    //var statcode = hcconversationresponse;               
                    if (poster.Count > 0)
                        {                      
                         action = "Conversations Fetched successfully";
                            logerror(action);
                        Library.logerror(action);
                        //var deserializer1 = new RestSharp.Serialization.Json.JsonDeserializer();
                        //var cooc = deserializer1.Deserialize<List<HC_Model.Root2>>(hcconversationresponse);
                        action = "looping through the conversation IDs";
                        logerror(action);
                        Library.logerror(action);
                        int success_count = 0;
                            int fail_count = 0;
                        int totalconvcount = 0;
                        int totalchatssenttoftp = 0;
                        Library.logerror(poster.Count+ " Conversatiosn found");
                        foreach (var looper in poster)
                            {
                                string convid = looper.id;
                                //GET MESSAGES
                                totalconvcount++;
                                string getmessageurl_part = $"/getMessages?conversationId={convid}";
                                string getmessageurl = hcurl + getmessageurl_part;
                                Library.logerror("Going to fetch All Chats for Conversation_Id: " + convid + " from EF_HybridChat");
                                var gethcmessages = new RestSharp.RestClient(getmessageurl);
                                var getmessagesrequest = new RestSharp.RestRequest(getmessageurl, RestSharp.Method.GET);
                                var hcmessagesresponse = gethcmessages.Execute(getmessagesrequest);
                                int GetMessageStatcode = Convert.ToInt32(hcmessagesresponse.StatusCode);
                                if (GetMessageStatcode == 200)
                                {
                                    string info = "Chats for Conversations_Id: " + looper.id + " fetched successfully";
                                    Library.logerror(info);
                                    var MessageDeserializer = new RestSharp.Serialization.Json.JsonDeserializer();
                                    var messageObj = MessageDeserializer.Deserialize<List<HC_Model.Root>>(hcmessagesresponse);
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
                                    ObjClass.sourceType = "Chat";
                                    ObjClass.project = "LifeLine Customer Contact Solution";
                                    ObjClass.startTime = looper.startTime;
                                    ObjClass.endTime = looper.updatedAt;
                                    ObjClass.subject = "";
                                    ObjClass.direction = 2;
                                    ObjClass.threadId = looper.taskId;
                                    ObjClass.datasource = "Text";
                                    ObjClass.parentId = convid;
                                foreach (var messageitem in messageObj)
                                {
                                    LLA_Model.Actor actorsids = new LLA_Model.Actor();
                                    actorsids.id = messageitem.from.id.ToLower();
                                    bool containsItem = ActorList.Any(item => item.id == actorsids.id);
                                    if (containsItem == false)
                                    {
                                        actorsids.id = messageitem.from.id.ToLower();
                                        actorsids.email = "";
                                        actorsids.accountId = messageitem.from.id.ToLower();
                                        string finalrole;
                                        switch (messageitem.from.type.ToLower())
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
                                        actorsids.role = finalrole;
                                        actorsids.displayName = HttpUtility.UrlDecode(messageitem.from.name);
                                        actorsids.timezone = "";
                                        actorsids.enterTime = messageitem.timestamp;
                                        actorsids.leaveTime = ObjClass.endTime;
                                        //CONFIRM IF THE ACTOR DOES NOT ALREADY EXIST IN THE LIST OF ACTORS AND PUSH NEW ITEM INTO LIST
                                        ActorList.Add(actorsids);
                                    }
                                }
                                foreach (var message in messageObj)
                                    {
                                    
                                    //SETTING THE DYNAMIC PART OF UTTERANCE CLASS INSTANCE
                                    //CREATE NEW UTTERANCE SUBCLASS
                                    if (message.messageType != "ActivityMessage")
                                    {
                                        LLA_Model.Utterance utteranceinst = new LLA_Model.Utterance();
                                        if (message.to.Count == 0)
                                        {
                                            forempty = ActorList.Where(f => f.id!= message.from.id).Select(n => n.id).ToList();
                                        }
                                        else
                                        {
                                            receiver = message.to[0].id.ToLower().ToString();
                                            bool containsItem2 = To.Any(item => item.ToString() == receiver.ToString());
                                            if (containsItem2 == false)
                                            {
                                                To.Add(receiver);
                                            }
                                        }
                                        utteranceinst.language = "en-us";
                                        utteranceinst.actor = message.from.id.ToLower();
                                        utteranceinst.to = To.Count == 0 ? forempty : To;
                                        utteranceinst.startTime = message.timestamp;
                                        utteranceinst.type = message.messageType;
                                        utteranceinst.value = message.text == null ? "" : System.Web.HttpUtility.UrlDecode(message.text);
                                        utteranceinst.raw_value = message.text == null ? "" : System.Web.HttpUtility.UrlDecode(message.text);
                                        // PUSH NEW ITEM INTO LIST
                                        UtteranceList.Add(utteranceinst);
                                    }
                                        //SETTING THE DYNAMIC PART OF ROOT CLASS INSTANCE
                                        ObjClass.type = "EF-HybridChat" + message.messageType;
                                        ObjClass.actors = ActorList;
                                        ObjClass.attributes = attributeinst;
                                        ObjClass.utterances = UtteranceList;
                                    }

                                //=====================================
                                //SAVE SFTP FILE
                                //=====================================
                                //CREATE A FILE OUT OF THE OBJECT BEFORE SENDING TO SFTP
                                    Library.logerror("Creating JSon file - SFTP Transfer for Conversation_Id: " + convid + "");
                                    using (StreamWriter file = File.CreateText(@"C:\inetpub\wwwroot\temp\" + convid + ".json"))
                                {
                                    JsonSerializer writefile = new JsonSerializer();
                                    writefile.Serialize(file,ObjClass);
                                    Library.logerror("json created successfully, now sending file to sftp client for conversation_id: " + convid + "");
                                }
                                    
                                    //===============================================================================================================
                                    //VERINT INGESTION PART
                                    //===============================================================================================================

                                    //CHECK IF AGENT EXISTS IN THE ACTORS AND SEND TO VERINT ELSE IGNORE
                                    bool agenstexist = ActorList.Any(item => item.role.ToLower() == "agent");
                                    //NOW CALLING NTT API TO PUSH THE CHAT TRANSCRIPTS
                                    if (agenstexist == true)
                                    {
                                        //AGENT EXISTS, NOW CALLING VERINT API TO PUSH THE CHAT TRANSCRIPTS
                                       string LLA_url = "https://sydpvertxr01.iptel.lifeline.org.au/api/recording/textcapture/v1/ingestion";
                                        string test = "ed067050bbc1a63b285e970cf551dce5";
                                        var keyId = "hmm11D1C";
                                        var keyStr = "nCYUvKyXqoc6dEboQCdmO8B94jVY8ySZrVBJWZLRS1s";

                                        var client = new RestSharp.RestClient(LLA_url);
                                        var request = new RestSharp.RestRequest("" + LLA_url + "", RestSharp.Method.POST);
                                        client.UseVwtAuthentication(keyId, keyStr);
                                        request.RequestFormat = RestSharp.DataFormat.Json;
                                        request.AddJsonBody(ObjClass);
                                        var response = client.Execute(request);
                                        int V_StatCode = Convert.ToInt32(response.StatusCode);
                                        if (V_StatCode == 201)
                                        {
                                            string checkquery = "IF EXISTS (select * from Report_Stat where ConversationId='" + ObjClass.id + "') BEGIN update Report_Stat set status=" + 1 + ",Error_message='N/A',Date_Updated='" + DateTime.Now + "' where ConversationId='" + ObjClass.id + "' END ELSE BEGIN insert into Report_Stat (ConversationId,ChatStartTime,ChatEndTime,ThreadId,status,Error_message,Date_reported) values('" + ObjClass.id + "','" + ObjClass.startTime + "','" + ObjClass.endTime + "','" + ObjClass.threadId + "'," + 1 + ",'N/A','" + DateTime.Now + "') END";
                                            string resp = Library.NonQeryRequest(checkquery);
                                            switch (resp)
                                            {
                                                case "200":
                                                    success_count = success_count + 1;
                                                    string s = "Successfully pushed Chat Transcript for ChatID: " + convid; Library.logerror(s);
                                                    break;
                                                case "400":
                                                    success_count = success_count + 1;
                                                    string err = "Successfully pushed Chat Transcript to Verint but could not insert audit into DB for ChatID: " + convid;
                                                    Library.logerror(err);
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            string errorinstring = response.Content.ToString() == "" ? response.ErrorException.Message.ToString() : response.Content.ToString();
                                            errorinstring = errorinstring.Replace("'", "");
                                            errorinstring = errorinstring.Replace("\"", "");
                                            errorinstring = errorinstring.Replace("data:{documentId:a6rFMFkp8n0bBoBw-lFP7},meta:{request_server:sydpvertxr01.iptel.lifeline.org.au,_error:,request_authHeaderValue:Bearer eyJhbGciOiJIUzI1NiIsImtpZCI6IkFsbktMR1dCIiwidHlwIjoiSldUIn0.eyJ2X2F1dGgiOiJTV1QiLCJ2X2tleSI6ImhtbTExRDFDIiwidW5pcXVlX25hbWUiOiJUZW5hbnQiLCJzdWIiOiJUZW5hbnQiLCJyb2xlIjoiVGVuYW50Iiwidl90aWQiOiIwIiwibmJmIjoxNjU1Mzg4MjU5LCJleHAiOjE2NTUzODg1NTksImlhdCI6MTY1NTM4ODI1OSwiaXNzIjoiVmVyaW50In0.tPAZj-j5UKhgPbyGwtolixrTdPfMZ7uo2ttizhJMTXU,request_port:29519,request_uri:/api/recording/textcapture/v1/ingestion},api_request_id", "");
                                            string checkquery = "IF EXISTS (select * from Report_Stat where ConversationId='" + ObjClass.id + "') BEGIN update Report_Stat set status=" + 0 + ",Error_message='" + errorinstring + "',Date_Updated='" + DateTime.Now + "' where ConversationId='" + ObjClass.id + "' END ELSE BEGIN insert into Report_Stat (ConversationId,ChatStartTime,ChatEndTime,ThreadId,status,Error_message,Date_reported) values('" + ObjClass.id + "','" + ObjClass.startTime + "','" + ObjClass.endTime + "','" + ObjClass.threadId + "'," + 0 + ",'" + errorinstring + "','" + DateTime.Now + "') END";
                                            string resp1 = Library.NonQeryRequest(checkquery);
                                            switch (resp1)
                                            {
                                                case "200":
                                                    fail_count = fail_count + 1;
                                                    var ss = response.StatusCode.ToString() == "0" ? response.ErrorException.Message.ToString() : errorinstring;
                                                    Library.logerror($"error:{ss}");
                                                    break;
                                                case "400":
                                                    fail_count = fail_count + 1;
                                                    string err = "Error pushing Transcript to Verint, could not insert audit into DB for ChatID: " + convid;
                                                    Library.logerror(err + response.StatusCode.ToString() == "0" ? response.ErrorException.Message.ToString() : errorinstring);
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //NO AGENT IN THE ACTOR LIST SO DO convid
                                        string noagenterror = "No agent exist in the conversation: " + convid + "";
                                        Library.logerror(noagenterror);
                                    }

                                }
                                else
                                //,CONVERSATION ID DID NOT RETURN 200 OK
                                {
                                    Library.logerror("Error fetcghing messages for Conversation_Id: " + convid + " " + GetMessageStatcode + "");
                                }
                            }

                        //======================================
                        //SENDING BULK JSON FILES TO SFTP PART
                        //======================================

                        //PUSHING TO SFTP FOLDER
                        System.IO.DirectoryInfo di = new DirectoryInfo(@"C:\inetpub\wwwroot\Temp\");
                        var privateKey = new PrivateKeyFile(@"C:\EF\sFTP\expertflow_key.ppk");// USING PUBLIC KEY
                        
                        using (var sftpclient = new SftpClient(host, port, username, new[] { privateKey }))
                        {
                            sftpclient.Connect();
                            if (sftpclient.IsConnected)
                            {
                                Library.logerror("connected to sftp to transfer batch files");
                                foreach (FileInfo file in di.EnumerateFiles())
                                {
                                    string uploadfile = file.FullName;//@"c:\inetpub\wwwroot\temp\" + convid + ".json";

                                    using (var filestream = new FileStream(uploadfile, FileMode.Open))
                                    {
                                        sftpclient.BufferSize = 4 * 1024; // bypass payload error large files
                                        sftpclient.UploadFile(filestream, Path.GetFileName(uploadfile));
                                    }
                                    Library.logerror("file sent to sftp: " + Path.GetFileName(uploadfile) + "");
                                    totalchatssenttoftp = totalchatssenttoftp + 1;
                                    Library.logerror(totalchatssenttoftp + " conversation files sent to SFTP");
                                    if (File.Exists(uploadfile))
                                    {
                                        // if file found, delete it    
                                        File.Delete(uploadfile);
                                    }
                                }
                            }
                            else
                            {
                                Library.logerror("connection to sftp server failed");
                            }
                        }
                        //DELETING ANY REMNANT FILE IN TEMP FOLDER FOR JSON UPLOAD
                        foreach (FileInfo file in di.EnumerateFiles())
                        {
                            file.Delete();
                        }
                        Library.logerror("SFTP transfer complete");
                        Library.logerror("Total Conversations Found: "+ totalconvcount + "");
                        Library.logerror("Total Conversations files sent to SFTP: " + totalchatssenttoftp + "");
                    }
                    else
                    {
                        string errormessage = "Conversations could not be Fetched from EF Hybrid Chat";
                        logerror(errormessage);
                    }
                }
                else
                {
                     action = "System could not get the Hybrid Chat Reporting URL";
                    Library.logerror(action);
                }
            }
            catch (Exception e)
            {
                string errormessage = e.Message.ToString();
                logerror(errormessage);
            }
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
            Console.WriteLine(messageex);
        }

        public void getpassword()
        {
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


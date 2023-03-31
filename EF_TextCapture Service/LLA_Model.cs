using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_TextCapture_Service
{
    public class LLA_Model
    {
        public class verint_interface
        {
            public string id { get; set; }
            public string language { get; set; }
            public string type { get; set; }
            public string sourceType { get; set; }
            public string project { get; set; }
           // public string channel { get; set; }
            public DateTime startTime { get; set; }
            public DateTime endTime { get; set; }
            public string subject { get; set; }
            public int direction { get; set; }
            public string threadId { get; set; }
            public string datasource { get; set; }
            public string parentId { get; set; }
            public List<Actor> actors { get; set; }
            public Attributes attributes { get; set; }
            public List<Utterance> utterances { get; set; }

        }

        public class Actor
        {
            public string id { get; set; }
            public string email { get; set; }
            public string accountId { get; set; }
            public string role { get; set; }
            public string displayName { get; set; }
            public string timezone { get; set; }
            public DateTime enterTime { get; set; }
            public DateTime leaveTime { get; set; }
        }

        public class Attributes
        {
            public string sourceType { get; set; }
            public string sourceSubType { get; set; }
        }

        public class Utterance
        {
            public string language { get; set; }
            public string actor { get; set; }
            public List<string> to { get; set; }
            public DateTime startTime { get; set; }
            public string type { get; set; }
            public string value { get; set; }
            public string raw_value { get; set; }
        }



        public class errorobj
        {
            public string errorname = "There was an error";

        }

        public class error
        {
            public api_error api_error { get; set; }
            public data data { get; set; }
            public meta meta { get; set; }
            public string api_request_id { get; set; }
        }
        public class api_error
        {
            public string message { get; set; }

        }
        public class data
        {
            public string documentId { get; set; }
        }
        public class meta
        {
            public string request_server { get; set; }
            public string _error { get; set; }
            public string request_authHeaderValue { get; set; }
            public string request_port { get; set; }
            public string request_uri { get; set; }
        }

        ////THIS MODEL IS FOR THE INTERBAL BASED API FOR ALL CHAT CONVERSATIONS 
        //public class conversationloop
        //{
        //    public string _id { get; set; }
        //    public string participant { get; set; }
        //    public List<roles> roles { get; set; }
        //    public List<handraise> handraises { get; set; }
        //    public string id { get; set; }
        //    public string instanceId { get; set; }
        //    public string startTime { get; set; }
        //    public string channel { get; set; }
        //    public string state { get; set; }
        //    public string type { get; set; }
        //}

        //public class roles
        //{

        //}public class handraise
        //{

        //}


    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Verint_Chat_API_Capture
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
            public string channel { get; set; }
            public string startTime { get; set; }
            public string endTime { get; set; }
            public string subject { get; set; }
            public string direction { get; set; }
            public string threadId { get; set; }
            public string datasource { get; set; }
            public string parentId { get; set; }
            public List<actorobj> actors { get; set; }
            public attributesobj attributes { get; set; }
            public List<utteranceobj> utterances { get; set; }

        }

        public class actorobj
        {
            public string id { get; set; }
            public string email { get; set; }
            public string accountId { get; set; }
            public string role { get; set; }
            public string displayName { get; set; }
            public string timezone { get; set; }
            public string enterTime { get; set; }
            public string leaveTime { get; set; }
        }

        public class attributesobj
        {
            public string sourceType { get; set; }
            public string sourceSubType { get; set; }

        }

        public class utteranceobj
        {
            public string language { get; set; }
            public string actor { get; set; }
            public List<string> to { get; set; }
            public string role { get; set; }
            public string displayName { get; set; }
            public string startTime { get; set; }
            public string type { get; set; }
            public string value { get; set; }
            public string raw_value { get; set; }
        }

        public class errorobj
        {
            public string errorname = "There was an error";

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


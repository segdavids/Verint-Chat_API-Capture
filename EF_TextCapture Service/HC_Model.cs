using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_TextCapture_Service
{
    public class HC_Model
    {
        public class HC_Interface
        {

        }
        //BEGINNING OF CLASS TO FETCHE ALL CONVERSATIONS 
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
        public class To
        {
            public string id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
        }

        public class From
        {
            public string id { get; set; }
            public string name { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string alias { get; set; }
            public string type { get; set; }
        }

        public class Participant
        {
            public string id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
        }

        public class Info
        {
            public Participant participant { get; set; }
        }

        public class Button
        {
            public int index { get; set; }
            public string payload { get; set; }
            public string title { get; set; }
            public string type { get; set; }
        }

        public class StructuredData
        {
            public object elements { get; set; }
            public object quick_replies { get; set; }
            public List<Button> buttons { get; set; }
            public object attachment { get; set; }
            public object image { get; set; }
            public object custom { get; set; }
        }

        public class Root
        {
            public string _id { get; set; }
            public List<To> to { get; set; }
            public List<object> intents { get; set; }
            public List<object> output { get; set; }
            public List<object> entities { get; set; }
            public string conversationId { get; set; }
            public string messageId { get; set; }
            public string messageType { get; set; }
            public From from { get; set; }
            public DateTime timestamp { get; set; }
            public string tag { get; set; }
            public string refId { get; set; }
            public bool processByBot { get; set; }
            public string language { get; set; }
            public string channel { get; set; }
            public string activityType { get; set; }
            public Info info { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public int __v { get; set; }
            public string text { get; set; }
            public List<object> attachments { get; set; }
            public List<object> buttons { get; set; }
            public string botResponseType { get; set; }
            public StructuredData structuredData { get; set; }
        }
        public class MainRoot
        {
            public List<Root> response { get; set; }
        }
        //END OF CLASS TO FETCH ALL CONVERSATIONS

        //BEGINNING OF CLASS TO FETCH MESSAGES REAPONSE
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
        public class Root2
        {
            public string _id { get; set; }
            public List<object> participant { get; set; }
            public List<object> roles { get; set; }
            public List<object> handRaise { get; set; }
            public string id { get; set; }
            public string instanceId { get; set; }
            public DateTime startTime { get; set; }
            public string channel { get; set; }
            public string state { get; set; }
            public string type { get; set; }
            public string taskId { get; set; }
            public string refId { get; set; }
            public string sessionType { get; set; }
            public string skillgroup { get; set; }
            public string lastOwnerAgent { get; set; }
            public string lastOutboundAgent { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public int __v { get; set; }
            public List<object> participantJoinTime { get; set; }
            public DateTime endTime { get; set; }
        }

      public class  MainRoot2
            {
            public List<Root2> response { get; set; }
            }
        //END OF CLASS TO FETCH MESSAGES REAPONSE




    }
}

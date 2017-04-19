using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Wrapper.Services
{
    public class HttpClientService<T>
    {
        private readonly string _authentication;
        public HttpClientService()
        {
           
        }

        public HttpClientService(string consumerKey, string consumerSecret)
        {
            _authentication = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(consumerKey + ":" + consumerSecret));
        } 
        public string GetContent(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();
            var streamText = "";

            if (responseStream == null) return streamText;

            var reader = new StreamReader(responseStream);
            streamText = reader.ReadToEnd();
            return streamText;
        }

        public T GetWithBearerAuthentication(string url, string bearerToken)
        {

            var request = (HttpWebRequest) WebRequest.Create(url);
            request.Headers.Add("Authorization", "Bearer " + bearerToken);

            var response = (HttpWebResponse) request.GetResponse();
            var responseStream = response.GetResponseStream();
            var streamText = "";

            if (responseStream != null)
            {
                var reader = new StreamReader(responseStream);
                streamText = reader.ReadToEnd();
            }

            var elements = JsonConvert.DeserializeObject<T>(streamText);
            return elements;
        }

        public string PostInsights(T model, string url, string consumerKey, string consumerSecret, bool isJson = false)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(consumerKey + ":" + consumerSecret));
            request.Headers.Add("Authorization", "Basic " + encoded);
            request.Method = "POST";
            request.ContentType = isJson?"application/json":"text/html";

            var writer = new StreamWriter(request.GetRequestStream());

            writer.Write(model);
            writer.Close();

            var response = (HttpWebResponse)request.GetResponse();

            var responseStream = response.GetResponseStream();
            var streamText = "";

            if (responseStream == null) return streamText;

            var reader = new StreamReader(responseStream);
            streamText = reader.ReadToEnd();

            return streamText;
        }

        public T Get(string url, string consumerKey, string consumerSecret)
        {
            var streamText = GetStringResponse(url, consumerKey, consumerSecret);
            var elements = JsonConvert.DeserializeObject<T>(streamText);

            return elements;
        }

        public string GetStringResponse(string url, string consumerKey, string consumerSecret)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(consumerKey + ":" + consumerSecret));
            request.Headers.Add("Authorization", "Basic " + encoded);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var response = (HttpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();
            var streamText = "";

            if (responseStream != null)
            {
                var reader = new StreamReader(responseStream);
                streamText = reader.ReadToEnd();
            }
            return streamText;
        }

        public T PostUrlEncoded(string body, string url, string consumerKey, string consumerSecret)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(consumerKey + ":" + consumerSecret));
            request.Headers.Add("Authorization", "Basic " + encoded);
            request.Method = "POST";
            request.ContentType = body.Equals("") ?"application/json": "application/x-www-form-urlencoded";

            var writer = new StreamWriter(request.GetRequestStream());

            writer.Write(body);
            writer.Close();

            var response = (HttpWebResponse)request.GetResponse();

            var responseStream = response.GetResponseStream();
            var streamText = "";

            if (responseStream != null)
            {
                var reader = new StreamReader(responseStream);
                streamText = reader.ReadToEnd();
            }

            var elements = JsonConvert.DeserializeObject<T>(streamText);
            return elements;
        }

        public T Post(string body, string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Authorization", "Basic " + _authentication);
            request.Method = "POST";
            request.ContentType = "application/json";

            var writer = new StreamWriter(request.GetRequestStream());

            writer.Write(body);
            writer.Close();

            var response = (HttpWebResponse)request.GetResponse();

            var responseStream = response.GetResponseStream();
            var streamText = "";

            if (responseStream != null)
            {
                var reader = new StreamReader(responseStream);
                streamText = reader.ReadToEnd();
            }

            var elements = JsonConvert.DeserializeObject<T>(streamText);
            return elements;
        }
    }
}

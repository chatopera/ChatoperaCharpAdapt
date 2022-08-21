using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChatoperaCSharpAdapt
{
    public class ChatoperaClient
    {
        private string baseUrl; // 服务地址
        private string clientId; // 机器人 ClientId
        private string clientSecret; // 机器人 Secret

        private class Authorize_token
        {
            public string appId { get; set; }
            public long timestamp { get; set; }
            public string signature { get; set; }
            public long random { get; set; }
        }

        private Object hash_hmac(string signatureString, string secretKey, bool raw_output = false)
        {
            var enc = Encoding.UTF8;
            HMACSHA1 hmac = new HMACSHA1(enc.GetBytes(secretKey));
            hmac.Initialize();

            byte[] buffer = enc.GetBytes(signatureString);
            if (raw_output)
            {
                return hmac.ComputeHash(buffer);
            }
            else
            {
                return BitConverter.ToString(hmac.ComputeHash(buffer)).Replace("-", "").ToLower();
            }
        }

        private String Md5(string s)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
            bytes = md5.ComputeHash(bytes);
            md5.Clear();
            string ret = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                ret += Convert.ToString(bytes[i], 16).PadLeft(2, '0');
            }
            return ret.PadLeft(32, '0');
        }

        private String generate_authorize_token(string clientId, string secret, string method, string path)
        {
            long timestamp;
            timestamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;

            Random r1 = new Random();
            long random = r1.Next(1000000000);

            string signature = (string)hash_hmac(clientId + timestamp.ToString() + random.ToString() + method + path, secret);

            Authorize_token parm = new Authorize_token();
            parm.appId = clientId;
            parm.random = random;
            parm.timestamp = timestamp;
            parm.signature = signature;
            string jsonstr = JsonConvert.SerializeObject(parm);
            //System.Console.WriteLine("jsonstr: {0}", jsonstr);

            Encoding encode = Encoding.UTF8;
            byte[] bytedata = encode.GetBytes(jsonstr);
            string x_param = Convert.ToBase64String(bytedata);

            return x_param;
        }

        private string httptrequest(string UrlPath)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPath);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            return retString;
        }

        public string RandString(int size, bool lowerCase = false)
        {
            Random random = new Random();
            StringBuilder _builder = new StringBuilder(size);
            int _startChar = lowerCase ? 97 : 65;//65 = A / 97 = a
            for (int i = 0; i < size; i++)
                _builder.Append((char)(26 * random.NextDouble() + _startChar));
            return _builder.ToString();
        }


        public string command(string method, string path, string content = "")
        {
            string rspResult = String.Empty;
            string service_method = method.ToUpper();
            string service_path = "/api/v1/chatbot/" + clientId + path;

            if (service_path.IndexOf('?') >= 0)
            {
                service_path += "&sdklang=CSharp";
            }
            else
            {
                service_path += "?sdklang=CSharp";
            }
            string service_url = baseUrl + service_path;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(service_url);
            string token = generate_authorize_token(clientId, clientSecret, service_method, service_path);
            req.Method = service_method;
            req.ContentType = "application/json";
            req.Headers["Authorization"] = token;
            req.Accept = "application/json";

            // req.ContentType = "text/html;charset=UTF-8";
            // req.ContentType = "application/x-www-form-urlencoded;charset=utf8";
            // req.ContentLength = bytesToPost.Length;

            if (service_method == "POST")
            {
                byte[] bytesToPost;//= System.Text.Encoding.Default.GetBytes(content); //转换为bytes数据
                Encoding encode = Encoding.GetEncoding("UTF-8");
                bytesToPost = encode.GetBytes(content);
                req.ContentLength = bytesToPost.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(bytesToPost, 0, bytesToPost.Length);
                    reqStream.Close();
                }
            }
            else
            {
                req.ContentLength = 0;
            }

            HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();
            if (rsp != null && rsp.StatusCode == HttpStatusCode.OK)
            {
                string encoding = rsp.ContentEncoding;
                if (encoding == null || encoding.Length < 1)
                {
                    encoding = "UTF-8"; //默认编码 
                }

                using (StreamReader sr = new StreamReader(rsp.GetResponseStream(), Encoding.GetEncoding(encoding)))
                {
                    rspResult = sr.ReadToEnd();
                    object jsonstr = JsonConvert.DeserializeObject(rspResult);
                    //System.Console.WriteLine("{0}", jsonstr);
                    string outtent = jsonstr.ToString();
                    rspResult = outtent;
                    string path_out = @"web_content.txt";
                    byte[] rspytes = Encoding.UTF8.GetBytes(outtent);
                    File.WriteAllBytes(path_out, rspytes);

                    sr.Close();
                }
                rsp.Close();
            }
            return rspResult;
        }

        /*
         * 机器人初始化 
         * @return null
         * @throws Exception
         * @deprecated DeprecationWarning: use `Chatbot#command` API instead, removed in 2020-10
         */

        public void init(string baseurl, string clientid, string secretkey)
        {
            baseUrl = baseurl;
            clientId = clientid;
            clientSecret = secretkey;
        }

        /*
         * 查看机器人详情
         * @return mixed
         * @throws Exception
         * @deprecated DeprecationWarning: use `Chatbot#command` API instead, removed in 2020-10
         */
        public string detail()
        {
            return command("GET", "/");
        }


        /*
        * 检索多轮对话
        * @param $userId 用户唯一标识
        * @param $textMessage 问题
        * @return mixed
        * @throws Exception
        * @deprecated DeprecationWarning: use `Chatbot#command` API instead, removed in 2020-10
        */
        public string conversation(string userId, string textMessage, double faqBestReplyThreshold = 0.9)
        {
            string content = string.Format("{{\"fromUserId\": \"{0}\" , \"textMessage\" : \"{1}\", \"isDebug\" : false }}", userId, textMessage);
            if (faqBestReplyThreshold == 0.9)
                content = string.Format("{{\"fromUserId\": \"{0}\" , \"textMessage\" : \"{1}\", \"isDebug\" : false }}", userId, textMessage);
            else
                content = string.Format("{{\"fromUserId\": \"{0}\" , \"textMessage\" : \"{1}\", \"faqBestReplyThreshold\":{2}, \"isDebug\" : false }}", userId, textMessage, faqBestReplyThreshold);
            return command("POST", "/conversation/query", content);
        }

        /*
           * 检索机器人知识库
           * @param $userId 用户唯一标识
           * @param $query  问题
           * @return mixed
           * @throws Exception
           * @deprecated DeprecationWarning: use `Chatbot#command` API instead, removed in 2020-10
           */
        public string faq(string userId, string query)
        {

            string content = string.Format("{{\"fromUserId\" : \"{0}\" , \"query\" : \"{1}\" }}", userId, query);
            return command("POST", "/faq/query", content);
        }

        /*
        * 查询用户列表
        * @param int $limit 每页数据条数
        * @param int $page 页面索引
        * @param string $sortby 排序规则
        * @return mixed
        * @throws Exception
        * @deprecated DeprecationWarning: use `Chatbot#command` API instead, removed in 2020-10
        */
        public string users(int limit = 50, int page = 1, string sortby = "-lasttime")
        {
            string url = string.Format("/users?page={0}&limit={1}&sortby={2}", page, limit, sortby);
            return command("GET", url);
        }

        /*
         * 查看一个用户的聊天历史
         * @param string userId 用户唯一标识
         * @param int  limit 每页数据条数
         * @param int $page 页面索引
         * @param string $sortby 排序规则[-lasttime: 最后对话时间降序]
         * @return mixed
         * @throws Exception
         * @deprecated DeprecationWarning: use `Chatbot#command` API instead, removed in 2020-10
         */
        public string chats(string userId, int limit = 50, int page = 1, string sortby = "-lasttime")
        {
            string url = string.Format("/users/{0}/chats?page={1}&limit={2}&sortby={3}", userId, page, limit, sortby);
            return command("GET", url);
        }

        /*
         * 屏蔽用户
         * @param $userId 用户唯一标识
         * @return bool 执行是否成功
         * @throws Exception
         * @deprecated DeprecationWarning: use `Chatbot#command` API instead, removed in 2020-10
         */
        public string mute(string userId)
        {
            return command("POST", "/users/" + userId + "/mute");
        }

        /*
         * 取消屏蔽用户
         * @param $userId 用户唯一标识
         * @return bool 执行是否成功
         * @throws Exception
         * @deprecated DeprecationWarning: use `Chatbot#command` API instead, removed in 2020-10
         */
        public string unmute(string userId)
        {
            return command("POST", "/users/" + userId + "/unmute");
        }

        /*
         * 检测用户是否被屏蔽
         * @param $userId 用户唯一标识
         * @return bool 用户是否被屏蔽
         * @throws Exception
         * @deprecated DeprecationWarning: use `Chatbot#command` API instead, removed in 2020-10
         */
        public string ismute(string userId)
        {
            return command("POST", "/users/" + userId + "/ismute");
        }

        /*
         * 读取用户画像
         * @param $userId 用户唯一标识
         * @return mixed
         * @throws Exception
         * @deprecated DeprecationWarning: use `Chatbot#command` API instead, removed in 2020-10
         */
        public string user(string userId)
        {
            return command("POST", "/users/" + userId + "/profile");
        }

        /*
         * 创建意图session
         * @deprecated DeprecationWarning: use `Chatbot#command` API instead, removed in 2020-10
         */
        public string intentSession(string uid, string channel)
        {
            string content = string.Format("{\"uid\" : \"{0}\" , \"channel\" : \"{1}\"}", uid, channel);
            return command("POST", "/clause/prover/session", content);

        }

        /*
         * 获取意图session详情
         * @deprecated DeprecationWarning: use `Chatbot#command` API instead, removed in 2020-10
         */
        public string intentSessionDetail(string sessionId)
        {
            string url = string.Format("/clause/prover/session/{0}", sessionId);
            return command("GET", url);
        }

        /*
         * 意图对话
         * @deprecated DeprecationWarning: use `Chatbot#command` API instead, removed in 2020-10
         */
        public string intentChat(string sessionId, string uid, string textMessage)
        {
            string content = string.Format("{\"fromUserId\" : \"{0}\" , \"session\" : {\"id\": \"{1}\"}, \"message\" : {\"textMessage\":\"{2}\" } }", uid, sessionId, textMessage);

            return command("POST", "/clause/prover/chat", content);
        }

        /*
         * 心理咨询聊天
         * @deprecated DeprecationWarning: use `Chatbot#command` API instead, removed in 2020-10
         */
        public string psychChat(string channel, string channelId, string userId, string textMessage)
        {
            string content = string.Format("{\"channel\" : \"{0}\", \"channelId\" : \"{1}\", \"userId\" : \"{2}\", \"textMessage\" : \"{3}\" }", channel, channelId, userId, textMessage);
            return command("POST", "/skills/psych/chat", content);
        }

        /*
         * 心理咨询查询
         * @deprecated DeprecationWarning: use `Chatbot#command` API instead, removed in 2020-10
         */
        public string psychSearch(string query, double threshold = 0.2)
        {
            string content = string.Format("{\"query\" : \"{0}\" , \"threshold\" : \"{1}\"}", query, threshold);

            return command("POST", "/skills/psych/search", content);
        }

        /*
         * 删除内部ID
         * @param $resp
         * @return mixed
         */
        private string purge(string resp)
        {
            return resp;
        }



    }
}

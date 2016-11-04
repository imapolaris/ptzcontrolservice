using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;

namespace CCTVSnapshotServer.Util
{
    public class WebUtil
    {
        /// <summary>
        /// 通过HTTP POST方式调用Web服务
        /// </summary>
        /// <param name="postUrl"></param>
        /// <param name="paramData"></param>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        public static string PostDataByWebClient(String postUrl, String paramData, String mediaType = @"application/x-www-form-urlencoded")
        {
            String result = String.Empty;
            try
            {
                byte[] postData = Encoding.UTF8.GetBytes(paramData);
                WebClient webClient = new WebClient();
                webClient.Headers.Add("Content-Type", mediaType);
                byte[] responseData = webClient.UploadData(new Uri(postUrl), "POST", postData);
                result = Encoding.UTF8.GetString(responseData);
            }
            catch (Exception e)
            {
                throw e;
            }
            return result;
        }

        /// <summary>
        /// 通过HTTP POST方式调用Web服务
        /// </summary>
        /// <param name="postUrl">url</param>
        /// <param name="paramData">data</param>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        public static string PostDataByWebRequest(string postUrl, string paramData, String mediaType = @"application/x-www-form-urlencoded")
        {
            string result = string.Empty;
            Stream newStream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(paramData);
                var conuri = new Uri(postUrl);
                var sp = ServicePointManager.FindServicePoint(conuri);
                sp.Expect100Continue = false;
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(conuri);
                webReq.Method = "POST";
                webReq.ContentType = mediaType;
                webReq.ContentLength = byteArray.Length;
                newStream = webReq.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);
                response = (HttpWebResponse)webReq.GetResponse();
                sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                result = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
                if (newStream != null)
                {
                    newStream.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// 通过TCP/IP POST方式调用Web服务
        /// </summary>
        /// <param name="postUrl"></param>
        /// <param name="paramData"></param>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        public static string PostDataByTcpClient(string postUrl, string paramData, String mediaType = @"application/x-www-form-urlencoded")
        {
            String result = String.Empty;
            TcpClient clientSocket = null;
            Stream readStream = null;
            try
            {
                clientSocket = new TcpClient();
                Uri URI = new Uri(postUrl);
                clientSocket.Connect(URI.Host, URI.Port);
                StringBuilder RequestHeaders = new StringBuilder();//用来保存HTML协议头部信息
                RequestHeaders.AppendFormat("{0} {1} HTTP/1.1\r\n", "POST", URI.PathAndQuery);
                RequestHeaders.AppendFormat("Connection:close\r\n");
                RequestHeaders.AppendFormat("Host:{0}:{1}\r\n", URI.Host, URI.Port);
                RequestHeaders.AppendFormat("Content-Type:{0}\r\n", mediaType);
                RequestHeaders.AppendFormat("\r\n");
                RequestHeaders.Append(paramData + "\r\n");
                Encoding encoding = Encoding.UTF8;
                byte[] request = encoding.GetBytes(RequestHeaders.ToString());
                clientSocket.Client.Send(request);
                readStream = clientSocket.GetStream();
                StreamReader sr = new StreamReader(readStream, Encoding.UTF8);
                result = sr.ReadToEnd();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (readStream != null)
                {
                    readStream.Close();
                }
                if (clientSocket != null)
                {
                    clientSocket.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// 通过HTTP方式下载文件
        /// </summary>
        /// <param name="downloadUrl"></param>
        /// <param name="saveFullName"></param>
        public static bool DownloadFileBy(string downloadUrl, string saveFullName)
        {
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)(HttpWebRequest.Create(downloadUrl));
                HttpWebResponse response = (HttpWebResponse)(request.GetResponse());
                Stream sr = response.GetResponseStream();

                Stream sw = new FileStream(saveFullName, FileMode.Create);
                long total = 0;
                byte[] bytes = new byte[1024];
                int s = sr.Read(bytes, 0, (int)bytes.Length);
                while (s > 0)
                {
                    total = s + total;
                    sw.Write(bytes, 0, s);
                    s = sr.Read(bytes, 0, (int)bytes.Length);
                }

                sw.Close();
                sr.Close();
                response.Close();

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

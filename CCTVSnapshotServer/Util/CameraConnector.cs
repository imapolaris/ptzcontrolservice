using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace CCTVSnapshotServer.Util
{
    public class CameraConnector : IDisposable
    {
        //Endpoint URL:http://192.168.122.1:8080/sony/camera|avContent|system
        string _endpointUrlOfCamera;
        string _endpointUrlOfavContent;
        string _endpointUrlOfSystem;

        public CameraConnector()
            : this(@"http://192.168.122.1:8080/sony/camera",
                  @"http://192.168.122.1:8080/sony/avContent",
                  @"http://192.168.122.1:8080/sony/system")
        {
            
        }

        public CameraConnector(string endpointUrlOfCamera, string endpointUrlOfavContent, string endpointUrlOfSystem)
        {
            _endpointUrlOfCamera = endpointUrlOfCamera;
            _endpointUrlOfavContent = endpointUrlOfavContent;
            _endpointUrlOfSystem = endpointUrlOfSystem;
        }

        public void SetFlashMode()
        {
            RequestInfo _setFlashMode = new RequestInfo() { Method = "setFlashMode", Params = "\"on\"", Id = "1", Version = "1.0" };
            var result = dooPut(_setFlashMode);

            Debug.WriteLine(result);
        }

        public void Zoom()
        {
            RequestInfo _actZoomIn = new RequestInfo() { Method = "actZoom", Params = "\"in\",\"start\"", Id = "1", Version = "1.0" };
            var result = dooPut(_actZoomIn);
            Debug.WriteLine(result);

            RequestInfo _actZoomOut = new RequestInfo() { Method = "actZoom", Params = "\"out\",\"start\"", Id = "1", Version = "1.0" };
            result = dooPut(_actZoomOut);
            Debug.WriteLine(result);
        }

        public void DownloadFile()
        {
            for (int i = 0; i < 50; ++i)
            {
                DateTime dt = DateTime.Now;

                var bRes = false;

                // 将相机切换到拍摄状态
                bRes = ChangeCameraFunction("Contents Transfer");

                string url = @"http://192.168.122.1:8080/contentstransfer/orgjpeg/index%3A%2F%2F1000%2F00000001-default%2F0000015B-00000987_347_1_1000";

                WebUtil.DownloadFileBy(url, Path.Combine(@"D:\TestD", DateTime.Now.ToString("yyyyMMddHHmmsss")+".jpg"));

                //Debug.WriteLine($"拍照耗时：{(DateTime.Now-dt).Milliseconds}ms");
                LogHelperEx.WriteLog($"下载图片耗时：{(DateTime.Now - dt).Milliseconds}ms");

                Thread.Sleep(3000);
            }
        }

        public void TestExample()
        {
            string result = "";

            //result = dooPut(_setCameraTransferInfo);
            //result = dooPut(_setCameraShootingInfo);

            //Debug.WriteLine(result);

            //// 存储信息
            //RequestInfo _getStorageInformation = new RequestInfo()
            //{
            //    Method = "getStorageInformation",
            //    Params = "",
            //    Id = "1",
            //    Version = "1.0",
            //};
            //result = dooPut(_getContentCountInfo);
            //Debug.WriteLine(result);

            //result = dooPut(_getEventPolling);

            // 判断相机当前状态，切换到内容传输状态
            var bRes = ChangeCameraFunction("Contents Transfer");
            if (!bRes)
                return;

            // 
            //RequestInfo _getSchemeList = new RequestInfo() { Method = "getSchemeList", Params = "", Id = "1", Version = "1.0" };
            //result = dooPut(_getSchemeList, _endpointUrlOfavContent);
            //Debug.WriteLine(result);

            //
            //RequestInfo _getSourceList = new RequestInfo() { Method = "getSourceList", Params = "{\"scheme\":\"storage\"}", Id = "1", Version = "1.0" };
            //result = dooPut(_getSourceList, _endpointUrlOfavContent);
            //Debug.WriteLine(result);

            //
            //RequestInfo _getContentCount = new RequestInfo() { Method = "getContentCount", Params = "{\"uri\":\"storage:memoryCard1\",\"target\":\"all\",\"view\":\"date\"}", Id = "1", Version = "1.2" };
            //result = dooPut(_getContentCount, _endpointUrlOfavContent);
            //Debug.WriteLine(result);

            //
            RequestInfo _getContentList = new RequestInfo() { Method = "getContentList", Params = "{\"uri\":\"storage:memoryCard1\",\"stIdx\":0,\"cnt\":100,\"view\":\"flat\",\"sort\":\"ascending\"}", Id = "1", Version = "1.3" };
            result = dooPut(_getContentList, _endpointUrlOfavContent);
            Debug.WriteLine(result);

            Debug.WriteLine(result);
        }

        public void SetStillSize()
        {
            dooPut(_setStillSize);
        }

        public void GetStillSize()
        {
            dooPut(_getStillSize);
        }

        public void SetStillQuality()
        {

        }

        public void GetStillQuality()
        {

        }

        public void TaskPic()
        {
            for (int i = 0; i < 10; ++i)
            {
                DateTime dt = DateTime.Now;

                var bRes = false;

                // 将相机切换到拍摄状态
                bRes = ChangeCameraFunction("Remote Shooting");


                // 拍照
                var result = dooPut(_actTakePictureInfo);

                //Debug.WriteLine($"拍照耗时：{(DateTime.Now-dt).Milliseconds}ms");
                LogHelperEx.WriteLog($"拍照耗时：{(DateTime.Now - dt).Milliseconds}ms");

                Thread.Sleep(3000);
            }
        }

        public void SetCameraShoot()
        {
            dooPut(_setCameraShootingInfo);
        }

        public void SetCameraTransfer()
        {
            dooPut(_setCameraTransferInfo);
        }

        /// <summary>
        /// 连接相机
        /// </summary>
        /// <returns></returns>
        public bool ConnectCamera()
        {
            string result = dooPut(_echo);
            if (string.IsNullOrEmpty(result))
                return false;

            return true;
        }

        /// <summary>
        /// 拍摄
        /// {"result":[["http:\/\/192.168.122.1:8080\/postview\/memory\/DCIM\/100MSDCF\/DSC00010.JPG?size=Scn"]],"id":1}
        /// </summary>
        /// <returns></returns>
        public bool Shoot(string saveFileName = @"d:\TaskPics")
        {
            for (int i = 0; i < 100; ++i)
            {
                // 测试程序运行时长
                DateTime sw1 = DateTime.Now;

                var bRes = false;

                // 将相机切换到拍摄状态
                bRes = ChangeCameraFunction("Remote Shooting");
                if (!bRes)
                    return false;
                DateTime sw2 = DateTime.Now;

                // 拍照
                var result = dooPut(_actTakePictureInfo);
                if (string.IsNullOrEmpty(result))
                    return false;


                // for test.
                //var result = "{\"result\":[[\"http:// 192.168.122.1:8080/ postview/ memory/ DCIM/ 100MSDCF/ DSC00085.JPG ? size = Scn\"]],\"id\":1}";

                DateTime sw3 = DateTime.Now;
                // 解析响应数据，获取图片名称
                var jo = JObject.Parse(result);
                var jp = jo.Property("result");
                if (jp == null)
                    return false;
                var results = jp.Value.ToString();
                var jt = JArray.Parse(results)[0];
                var imgname = JArray.Parse(jt.ToString())[0].Value<string>();

                // 过滤掉空格和\
                imgname = imgname.Replace(" ", "");
                imgname = imgname.Replace("\\", "/");

                //http://192.168.122.1:8080/postview/memory/DCIM/100MSDCF/DSC00074.JPG?size=Scn

                imgname = imgname.Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries)[0];
                imgname = imgname.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last();


                // 循环遍历全部内容
                int stIdx = 0;
                int cnt = 100;
                var uri = "";
                var url = "";
                var flag = false;


                DateTime sw4 = DateTime.Now;
                // 切换相机状态到内容传输，下载图片
                bRes = ChangeCameraFunction("Contents Transfer");
                if (!bRes)
                    return false;

                DateTime sw5 = DateTime.Now;
                do
                {
                    // 获取图片（这里方法可以改进为获取当前日期的内容，然后进行筛选）
                    RequestInfo _getContentList = new RequestInfo() { Method = "getContentList", Params = "{\"uri\":\"storage:memoryCard1\",\"stIdx\":" + $"{stIdx}" + ",\"cnt\":" + $"{cnt}" + ",\"view\":\"flat\",\"sort\":\"ascending\"}", Id = "1", Version = "1.3" };
                    result = dooPut(_getContentList, _endpointUrlOfavContent);
                    Debug.WriteLine(result);

                    // 判断是否没有获取到数据
                    if (result == null || result == "")
                        break;
                    // {"id":1,"error":[1,"Cannot obtain content list."]}
                    if (ActionFailed(result))
                        break;

                    // 解析内容，查找当前拍摄图片的URI和URL
                    var joo = JObject.Parse(result);
                    var jpp = joo.Property("result");
                    var jaa = JArray.Parse(jpp.Value.ToString());
                    jaa = JArray.Parse(jaa[0].ToString());
                    //var jtt = jaa.Where(x => IsMatch(x, images));

                    JToken jtc = null;
                    foreach (var jtt in jaa)
                    {
                        var s = jtt.Value<JObject>();
                        var jpc = s.Property("content");
                        var oo = jpc.Value;
                        var jao = oo.Value<JArray>("original");
                        var fname = jao[0].Value<string>("fileName");

                        if (string.Compare(fname, imgname, true) == 0)
                        {
                            flag = true;
                            jtc = jtt;
                            url = jao[0].Value<string>("url");
                            break;
                        }
                    }

                    if (flag)
                    {
                        uri = jtc.Value<string>("uri");
                        break;
                    }

                    stIdx += cnt;

                } while (true);

                if (!flag)
                {
                    Debug.WriteLine($"{imgname} 图片不存在...");
                    return false;
                }



                DateTime sw6 = DateTime.Now;
                // 下载图片
                // for test.
                var temp = Path.Combine(saveFileName, imgname);
                try
                {
                    bRes = WebUtil.DownloadFileBy(url, temp);
                }
                catch (Exception)
                {
                    // log the error information.
                    return false;
                }



                DateTime sw7 = DateTime.Now;
                if (bRes)
                {
                    // 删除图片
                    RequestInfo _deleteContent = new RequestInfo() { Method = "deleteContent", Params = "{\"uri\":[" + $"\"{uri}\"" + "]}", Id = "1", Version = "1.1" };
                    var res = dooPut(_deleteContent, _endpointUrlOfavContent);
                    Debug.WriteLine(res);
                }
                DateTime sw8 = DateTime.Now;

                LogHelperEx.WriteLog($"开始时间：{sw1}");
                LogHelperEx.WriteLog($"切换到拍照状态耗时：{(sw2-sw1).TotalMilliseconds}ms");
                LogHelperEx.WriteLog($"拍照耗时：{(sw3 - sw2).TotalMilliseconds}ms");
                LogHelperEx.WriteLog($"解析图片名称耗时：{(sw4 - sw3).TotalMilliseconds}ms");
                LogHelperEx.WriteLog($"切换到内容传输状态耗时：{(sw5 - sw4).TotalMilliseconds}ms");
                LogHelperEx.WriteLog($"检索当前拍摄图片耗时：{(sw6 - sw5).TotalMilliseconds}ms");
                LogHelperEx.WriteLog($"下载图片耗时：{(sw7 - sw6).TotalMilliseconds}ms");
                LogHelperEx.WriteLog($"删除图片耗时：{(sw8 - sw7).TotalMilliseconds}ms");
                LogHelperEx.WriteLog($"结束时间：{sw8}");
                LogHelperEx.WriteLog($"总耗时:{(sw8 - sw1).TotalMilliseconds}ms");

                LogHelperEx.WriteLog("******************************************************\n");

                //sw.Stop();
                //TimeSpan ts = sw.Elapsed;
                //Debug.WriteLine("这段程序的运行时间：{0} ms.",ts.TotalMilliseconds);
                

            }
            return true;
        }

        private bool IsMatch(JToken jt, string filename)
        {
            var s = jt.Value<JObject>();
            var jpc = s.Property("content");
            var oo = jpc.Value;
            var jao = oo.Value<JArray>("original");
            var fname = jao[0].Value<string>("fileName");

            if (string.Compare(fname, filename, true) == 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 传输图片
        /// </summary>
        /// <returns></returns>
        public bool TransferImage()
        {


            return true;
        }

        /// <summary>
        /// 删除全部内容
        /// </summary>
        /// <returns></returns>
        public bool DeleteAllContents()
        {
            // 将相机设置为内容传输状态
            var bRes = ChangeCameraFunction("Contents Transfer");
            if (!bRes)
                return false;

            // 循环删除全部内容
            var result = "";
            int stIdx = 0;
            int cnt = 100;
            int sum = 0;
            do
            {
                // 获取图片
                RequestInfo _getContentList = new RequestInfo() { Method = "getContentList", Params = "{\"uri\":\"storage:memoryCard1\",\"stIdx\":"+$"{stIdx}"+",\"cnt\":"+$"{cnt}"+",\"view\":\"flat\",\"sort\":\"ascending\"}", Id = "1", Version = "1.3" };
                result = dooPut(_getContentList, _endpointUrlOfavContent);
                Debug.WriteLine(result);

                // 判断是否没有获取到数据
                if (result == null || result == "")
                    break;
                // {"id":1,"error":[1,"Cannot obtain content list."]}
                if (ActionFailed(result))
                    break;

                StringBuilder builder = new StringBuilder();

                // 解析内容URI
                var jo = JObject.Parse(result);
                var jp = jo.Property("result");
                var ja = JArray.Parse(jp.Value.ToString());
                ja = JArray.Parse(ja[0].ToString());
                foreach (var jt in ja)
                {
                    var uri = jt.Value<string>("uri");
                    builder.Append("\""+$"{uri}"+"\",");
                }

                sum += ja.Count();

                var uris = builder.ToString().TrimEnd(new char[] { ',' });

                // 删除图片
                RequestInfo _deleteContent = new RequestInfo() { Method = "deleteContent", Params = "{\"uri\":[" + $"{uris}"+"]}", Id = "1", Version = "1.1" };
                var res = dooPut(_deleteContent, _endpointUrlOfavContent);
                Debug.WriteLine(res);

                Debug.WriteLine($"已删除图像{sum}个");

            } while (true);

            Debug.WriteLine($"共删除图像{sum}个");

            return true;
        }

        /// <summary>
        /// 删除最新的图像
        /// </summary>
        /// <returns></returns>
        public bool DeleteLastContent()
        {


            return true;
        }

        private string dooPut(RequestInfo info, string endpointUrl = @"http://192.168.122.1:8080/sony/camera")
        {
            string result = null;
            try
            {
                result = WebUtil.PostDataByWebRequest(endpointUrl, info.ToContent(), "application/json");
            }
            catch (Exception)
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// 切换相机到指定状态
        /// </summary>
        /// <param name="toCameraFunction"></param>
        /// <returns></returns>
        private bool ChangeCameraFunction(string toCameraFunction)
        {
            // 判断相机当前状态，切换到内容传输状态
            var result = dooPut(_getEventImmediately);
            if (result == null || result == "")
                return false;
            Debug.WriteLine(result);
            var val = GetValue(result, "cameraFunction", "currentCameraFunction");
            if (val != toCameraFunction)
            {
                if (toCameraFunction == "Contents Transfer")
                    result = dooPut(_setCameraTransferInfo);
                else if (toCameraFunction == "Remote Shooting")
                    result = dooPut(_setCameraShootingInfo);
                else
                    return false;
                Debug.WriteLine(result);
                var success = ActionSucceed(result);
                if (!success)
                    return false;

                // 检查相机状态是否已切换到内容传输状态
                do
                {
                    result = dooPut(_getEventPolling);
                    Debug.WriteLine(result);
                    val = GetValue(result, "cameraFunction", "currentCameraFunction");
                    if (val == toCameraFunction)
                        break;
                } while (true);

                // 给相机切换状态留出时间
                Thread.Sleep(300);
            }

            return true;
        }

        /// <summary>
        /// 检查请求是否成功执行
        /// </summary>
        /// <returns></returns>
        private bool ActionSucceed(string result)
        {
            var jo = JObject.Parse(result);
            var jp = jo.Property("result");
            if (jp == null)
                return false;
            var ja = JArray.Parse(jp.Value.ToString());
            if (ja[0].Value<string>() == "0")
                return true;

            return false;
        }

        /// <summary>
        /// 检查请求是否执行失败
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool ActionFailed(string result)
        {
            var jo = JObject.Parse(result);
            var jp = jo.Property("error");
            if (jp != null)
                return true;

            return false;
        }

        private string GetValue(string result, string strType, string strKey, string defaultVal = "")
        {
            if (string.IsNullOrEmpty(result)) return defaultVal;
            var jo = JObject.Parse(result);
            var jp = jo.Property("result");
            var ja = JArray.Parse(jp.Value.ToString());
            var jts = ja.Where(x=>IsValid(x, "type", strType));
            if (jts != null && jts.Count() > 0)
            {
                string strVal = jts.ElementAt(0).Value<string>(strKey);
                if (string.IsNullOrEmpty(strVal))
                    return defaultVal;
                else
                    return strVal;
            }

            return defaultVal;
        }

        private bool IsValid(JToken jt,string strKey, string strVal, bool ignoreCase = false)
        {
            if (jt == null)
                return false;

            if (jt.Type == JTokenType.Object)
            {
                if (string.Compare(jt.Value<string>(strKey), strVal, ignoreCase) == 0)
                    return true;
                else
                    return false;
            }

            return false;
        }

        /// <summary>
        /// 轮询相机设置，直到符合终止条件
        /// </summary>
        /// <param name="endCondition"></param>
        private void Polling(string endCondition)
        {
            do
            {
                string result = dooPut(_getEventPolling);

                if (result == endCondition)
                    break;
            }
            while (true);
        }

        /// <summary>
        /// 相关设置在相机上是否已完成
        /// </summary>
        /// <param name="result"></param>
        /// <param name="strType"></param>
        /// <returns></returns>
        private bool IsSettingFinished(string result, string strType, params string[] vals)
        {
            var jo = JObject.Parse(result);
            var jp = jo.Property("result");
            var ja = JArray.Parse(jp.Value.ToString());
            var jts = ja.Where(x => x.Value<string>("type") == strType);
            if (jts != null && jts.Count() > 0)
            {
                switch (strType)
                {
                    case "cameraFunctionResult":
                        {
                            if (!(jts.ElementAt(0).Value<string>("cameraFunctionResult") == "Success"))
                                return false;
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            
        }

        //测试反馈
        RequestInfo _echo = new RequestInfo() { Method = "echo", Params = "\"hi\"", Id = "1", Version = "1.0" };


        #region 拍摄相关的命令

        /// 设置拍摄模式
        /// 响应数据格式：
        /// 
        RequestInfo _setShootMode = new RequestInfo() { Method= "setShootMode", Params="\"still\"", Id="1", Version="1.0" };
        /// 拍照
        /// 响应数据格式：
        /// 正确：{"result":[["http:\/\/192.168.122.1:8080\/postview\/memory\/DCIM\/100MSDCF\/DSC00010.JPG?size=Scn"]],"id":1}
        RequestInfo _actTakePictureInfo = new RequestInfo() { Method = "actTakePicture", Params = "", Id = "1", Version = "1.0" };

        //设置自动抓拍
        RequestInfo _setSelfTimerInfo = new RequestInfo() { Method = "setSelfTimer", Params = "0", Id = "1", Version = "1.0" };

        RequestInfo _setStillSize = new RequestInfo() { Method = "setStillSize", Params = "\"16:9\",\"17M\"", Id = "1", Version = "1.0" };

        RequestInfo _getStillSize = new RequestInfo() { Method = "getStillSize", Params = "", Id = "1", Version = "1.0" };

        RequestInfo _setStillQuality = new RequestInfo() { Method = "", Params = "", Id = "", Version = "" };

        RequestInfo _getStillQuality = new RequestInfo() { Method = "", Params = "", Id = "", Version = "" };

        #endregion

        #region View angle
        //设置视场角//示例，120：120度,170：170度,-1：无效。不同设备支持的值不同
        RequestInfo _setViewAngleInfo = new RequestInfo() { Method = "setViewAngle", Params = "120", Id = "1", Version = "1.0" };
        //获取当前视场角
        RequestInfo _getViewAngleInfo = new RequestInfo() { Method = "getViewAngle", Params = "", Id = "1", Version = "1.0" };
        //Response
        //{
        //"result": [
        //120
        //],
        //"id": 1
        //}
        //获取设备支持的视场角
        RequestInfo _getSupportedViewAngleInfo = new RequestInfo() { Method = "getSupportedViewAngle", Params = "", Id = "1", Version = "1.0" };
        //Response
        //{
        //"result": [
        //[120,170]
        //],
        //"id": 1
        //}
        #endregion View angle

        #region Transferring images

        /// 删除图片 
        RequestInfo _deleteContent = new CCTVSnapshotServer.Util.RequestInfo() { Method = "deleteContent", Params = "{\"uri\":[]}", Id = "1", Version = "1.1" };

        //获取图片列表
        //RequestInfo _getContentCountInfo = new RequestInfo() { Method = "\"getContentCount\"", Params = "[0]", Id = "1", Version = "\"1.0\"" };
        ///要使用拍摄功能API，客户端应通过“setCameraFunction”将相机功能更改为“远程拍摄”。 “遥控拍摄”是摄像机通过Wi-Fi与客户端连接后的默认功能。)
        RequestInfo _setCameraShootingInfo = new RequestInfo() { Method = "setCameraFunction", Params = "\"Remote Shooting\"", Id = "1", Version = "1.0" };
        ///要使用传输图片功能API，客户端应通过“setCameraFunction”将摄像头功能更改为“内容传输”。 传输图片功能由特定的相机型号支持。
        RequestInfo _setCameraTransferInfo = new RequestInfo() { Method = "setCameraFunction", Params = "\"Contents Transfer\"", Id = "1", Version = "1.0" };


        //获取设备的资源
        RequestInfo _getSchemeListInfo = new RequestInfo() { Method = "getSchemeList", Params = "", Id = "1", Version = "1.0" };
        //Response  JSON Example
        //{
        //"result": [
        //[
        //{
        //"scheme": "storage"
        //}
        //]
        //],
        //"id": 1
        //}
        //上边相机仅支持“storage”作为方案。
        RequestInfo _getSourceListInfo = new RequestInfo() { Method = "getSourceList", Params = "{\"scheme\": \"storage\"}", Id = "1", Version = "1.0" };
        //Response 在特定模式下资源名称列表
        //{
        //"result": [
        //[
        //{
        //"source": "storage:memoryCard1"
        //}
        //]
        //],
        //"id": 1
        //}
        //特别说明（详情）该相机仅支持“存储：存储卡1”作为来源。
        //getContentCount (v1.2) 此API提供了在特定URI下获取内容计数的功能。
        RequestInfo _getContentCountInfo = new RequestInfo() { Method = "getContentCount", Params = "{\"uri\": \"storage:memoryCard1\",\"type\":\"still\",\"view\":\"date\"}", Id = "1", Version = "1.2" };
        //        {
        //"method": "getContentCount",
        //"params": [
        //{
        //"uri": "storage:memoryCard1?path=2014-08-18",
        //"type": [
        //"still",
        //"movie_mp4"
        //],
        //"view": "date"
        //}
        //],
        //"id": 1,
        //"version": "1.2"
        //}
        //Response 
        //{
        //"result": [
        //{
        //"count": 7
        //}
        //],
        //"id": 1
        //}
        //说明：Pdf P198

        #endregion Transferring images

        #region General

        RequestInfo _getEventImmediately = new RequestInfo() { Method = "getEvent", Params = "false", Id = "1", Version = "1.0" };

        // 轮询
        RequestInfo _getEventPolling = new RequestInfo() { Method = "getEvent", Params = "true", Id = "1", Version = "1.0" };

        #endregion
    }
}

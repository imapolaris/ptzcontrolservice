using CCTVModels;
using CCTVPTZControlService;
using PTZControlApi.Models;
using PTZControlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PTZControlApi.Controllers
{
    public class PTZControlController : ApiController
    {
        [Route("api/GetCCTVStaticInfos")]
        public IEnumerable<CCTVStaticInfo> GetCCTVStaticInfos()
        {
            checkAndInitWebApiBaseUri();
            return CCTVStaticInfoManager.Instance.GetAllStaticInfo();
        }

        [Route("api/getactions")]
        public IEnumerable<CameraControlAction> GetActions()
        {
            return CameraControlActionManager.Instance.Actions;
        }

        [HttpGet]
        public IHttpActionResult Control(string videoId, int action, byte data)
        {
            try
            {
                var config = CCTVStaticInfoManager.Instance.GetControlConfig(videoId);
                if (config != null)
                {
                    CameraAction act = (CameraAction)action;
                    CCTVPtzControlManager.Instance.PTZControl(videoId, act, data);
                    return Ok(act.ToString());
                }
            }
            catch(Exception ex)
            {
                return Ok(ex.Message);
            }
            return NotFound();
        }

        [HttpGet]
        public IHttpActionResult ToPTZ(string videoId, double pan, double tilt, double zoom)
        {
            try
            {
                var control = CCTVPtzControlManager.Instance.GetOrAddPtzControl(videoId);
                if (control != null)
                {
                    control.ToPTZ(pan, tilt, zoom);
                    return Ok($"转到{pan},{tilt},{zoom}");
                }
                return Ok("未找到对应设备。");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Ok(ex.Message);
            }
        }

        [HttpGet]
        public IHttpActionResult ToGeometry(string videoId, double lon, double lat, double alt, double width)
        {
            try
            {
                var control = CCTVPtzControlManager.Instance.GetOrAddPtzControl(videoId);
                if(control != null)
                {
                    var staticInfo = CCTVStaticInfoManager.Instance.GetStaticInfo(videoId);
                    if (staticInfo == null)
                        return Ok($"未获取到该设备对应的位置信息");
                    CCTVSnapshoter snapshoter = new CCTVSnapshoter(staticInfo, control, ConfigSettings.ShotshopPath);
                    snapshoter.ToGeometry(lon, lat, alt, width);
                    return Ok($"转到{lon},{lat}");
                }
                return Ok("未找到对应云台：" + videoId);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Ok(ex.Message);
            }
        }

        [Route("api/ptzlimits/{videoid}")]
        public IHttpActionResult GetPTZLimits(string videoid)
        {
            try
            {
                var config = CCTVStaticInfoManager.Instance.GetControlConfig(videoid);
                if (config != null)
                {
                    var ptzLimits = CCTVPtzControlManager.Instance.GetOrAddPtzControl(videoid)?.PTZLimits;
                    if(ptzLimits != null)
                        return Ok(ptzLimits);
                }
            }
            catch { }
            return NotFound();
        }

        [Route("api/ptz/{videoid}")]
        public IHttpActionResult GetPTZ(string videoid)
        {
            try
            {
                var config = CCTVStaticInfoManager.Instance.GetControlConfig(videoid);
                if (config != null)
                {
                    var ptz = CCTVPtzControlManager.Instance.GetOrAddPtzControl(videoid)?.PTZPosition;
                    if (ptz != null)
                        return Ok(ptz);
                }
            }
            catch {
            }
            return NotFound();
        }

        [Route("api/snapshot/{id}")]
        public IHttpActionResult GetSnapshot(string id)
        {
            try
            {
                string fileName = "D:\\Snapshot\\" + id + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg";
                Console.WriteLine(fileName);
                var list = id.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                string videoId = list[0];
                var config = CCTVStaticInfoManager.Instance.GetControlConfig(videoId);
                if (config != null)
                {
                    CCTVPtzControlManager.Instance.CaptureJPEGPicture(videoId, fileName);
                    return Ok(videoId);
                }
            }
            catch (Exception ex)
            {
                return Ok(ex.Message + "<br/>" + id);
            }
            return NotFound();
        }

        private void checkAndInitWebApiBaseUri()
        {
            string webApiBaseUri = System.Configuration.ConfigurationManager.AppSettings["WebApiBaseUri"];
            if (!string.IsNullOrEmpty(webApiBaseUri) && !webApiBaseUri.Equals(CCTVConfig.WebApiBaseUri))
            {
                CCTVConfig.WebApiBaseUri = webApiBaseUri;
                CCTVStaticInfoManager.Instance.Init(webApiBaseUri);
            }
            CCTVConfig.WebApiBaseUri = System.Configuration.ConfigurationManager.AppSettings["WebApiBaseUri"];
            Common.Log.Logger.Default.Trace(CCTVConfig.WebApiBaseUri);
        }
    }
}
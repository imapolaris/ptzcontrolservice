using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public interface IControl: IDisposable
    {
        DateTime LatestTime { get; }
        PTZ PTZPosition { get; }
        PTZLimit PTZLimits { get; }
        Action<PTZ> PTZEvent { get; set; }
        /// <summary>
        /// 云镜控制接口
        /// </summary>
        /// <param name="action">云镜控制参数，
        /// Aux：服务开关；
        /// ZOOM：变倍；
        /// FOCUS:焦距；
        /// IRIS光圈；</param>
        /// <param name="actData">云镜控制对应的参数，无效时设为0</param>
        void CameraControl(CameraAction action, byte actData);

        /// <summary>
        /// 以指定分辨率（需设备支持）将快照存储到指定路径
        /// </summary>
        /// <param name="fileName">快照存储的文件名（全路径）</param>
        /// <param name="wPicSize">图片尺寸：0-CIF(352*288/352*240)，
        /// 1-QCIF(176*144/176*120)，2-4CIF(704*576/704*480)或D1(720*576/720*486)，3-UXGA(1600*1200)， 4-SVGA(800*600)，5-HD720P(1280*720)，
        /// 6-VGA(640*480)，7-XVGA(1280*960)，8-HD900P(1600*900)，9-HD1080P(1920*1080)，10-2560*1920， 11-1600*304，12-2048*1536，
        /// 13-2448*2048，14-2448*1200，15-2448*800，16-XGA(1024*768)，17-SXGA(1280*1024)，18-WD1(960*576/960*480), 19-1080I(1920*1080)，
        /// 20-576*576，21-1536*1536，22-1920*1920，0xff-Auto(使用当前码流分辨率) --
        /// wPicSize设为2抓取的图片分辨率是4CIF还是D1由设备决定，一般为4CIF(P制:704*576/N制:704*480)。</param>
        /// <returns></returns>
        bool CaptureJPEGPicture(string fileName, ushort wPicSize = 2);
        /// <summary>
        /// 转到指定pan,tilt,zoom位置
        /// </summary>
        /// <returns></returns>
        void ToPTZ(double pan, double tilt, double zoom);
    }
}

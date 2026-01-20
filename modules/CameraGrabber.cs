using System;
using Cognex.VisionPro;

namespace ConsoleApp2.modules
{
    public static class CameraGrabber
    {
        public static ICogImage CaptureFromTool(string vppPath)
        {
            try
            {
                object loaded = CogSerializer.LoadObjectFromFile(vppPath);
                var acq = ExtractAcqFifoToolDynamic(loaded);

                if (acq == null)
                {
                    Console.WriteLine("❌ VPP 내부에서 CogAcqFifoTool을 찾지 못했습니다.");
                    return null;
                }

                Console.WriteLine("📸 카메라 이미지 그랩 중...");
                acq.Run();

                var image = acq.OutputImage;
                if (image == null)
                {
                    Console.WriteLine("⚠️ 캡처된 이미지가 없습니다.");
                    return null;
                }

                Console.WriteLine("✅ 카메라 이미지 획득 완료");
                return image;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 카메라 캡처 중 오류: {ex.Message}");
                return null;
            }
        }

        private static dynamic ExtractAcqFifoToolDynamic(object o)
        {
            if (o == null) return null;
            if (o.GetType().Name.Contains("CogAcqFifoTool")) return o;

            var prop = o.GetType().GetProperty("Tools");
            if (prop != null)
            {
                foreach (var t in (System.Collections.IEnumerable)prop.GetValue(o))
                {
                    if (t.GetType().Name.Contains("CogAcqFifoTool"))
                        return t;
                }
            }
            return null;
        }
    }
}

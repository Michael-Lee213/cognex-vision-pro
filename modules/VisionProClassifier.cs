using System;
using Cognex.VisionPro;
using Cognex.VisionPro.Blob;
using System.Linq;

namespace ConsoleApp2.modules
{
    public static class VisionProClassifier
    {
        public static string Analyze(
            ICogImage image,
            double cx,
            double cy,
            int roiSize = 160,
            int threshold = 180,
            int minArea = 300)
        {
            if (image == null)
                return "오류: 입력 이미지 없음";

            // === 1️⃣ 중심 기반 ROI (정사각형, 회전 0) ===
            var roi = new CogRectangleAffine
            {
                CenterX = cx,
                CenterY = cy,
                SideXLength = roiSize,
                SideYLength = roiSize,
                Rotation = 0
            };

            // === 2️⃣ Blob Tool 구성 ===
            var blob = new CogBlobTool
            {
                InputImage = image,
                Region = roi
            };

            // === 3️⃣ 세그멘테이션 설정 (버전 호환 리플렉션) ===
            try
            {
                dynamic seg = blob.RunParams.SegmentationParams;

                // ① 세그멘테이션 모드
                try
                {
                    var modeProp = seg.GetType().GetProperty("Mode");
                    var modeEnum = modeProp?.PropertyType;
                    object manualValue = null;

                    try { manualValue = Enum.Parse(modeEnum, "ManualThreshold", true); }
                    catch { manualValue = Enum.Parse(modeEnum, "Manual", true); }

                    modeProp?.SetValue(seg, manualValue, null);
                }
                catch { Console.WriteLine("⚠️ Mode 설정 실패 - 기본값 사용"); }

                // ② 임계값
                try
                {
                    var thrProp = seg.GetType().GetProperty("Threshold")
                                 ?? seg.GetType().GetProperty("ManualThreshold");
                    thrProp?.SetValue(seg, threshold, null);
                }
                catch { Console.WriteLine("⚠️ Threshold 설정 실패 - 기본값 유지"); }

                // ③ 극성 (밝은 영역 탐지)
                try
                {
                    var polProp = seg.GetType().GetProperty("Polarity");
                    var polEnumType = polProp?.PropertyType;
                    object lightVal = null;

                    try { lightVal = Enum.Parse(polEnumType, "LightOnDark", true); }
                    catch { lightVal = Enum.Parse(polEnumType, "LightBlobs", true); }

                    polProp?.SetValue(seg, lightVal, null);
                }
                catch { Console.WriteLine("⚠️ Polarity 설정 실패 - 기본값 유지"); }

                // ④ 연결 모드
                try
                {
                    var connProp = blob.RunParams.GetType().GetProperty("ConnectivityMode");
                    var connEnumType = connProp?.PropertyType;
                    object connVal = null;

                    try { connVal = Enum.Parse(connEnumType, "EightConnected", true); }
                    catch { connVal = Enum.Parse(connEnumType, "Grey8Connected", true); }

                    connProp?.SetValue(blob.RunParams, connVal, null);
                }
                catch { Console.WriteLine("⚠️ Connectivity 설정 실패 - 기본값 유지"); }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ 세그멘테이션 설정 중 오류: {ex.Message}");
            }

            // === 4️⃣ 실행 ===
            blob.Run();

            // === 5️⃣ 결과 판정 ===
            var results = blob.Results?.GetBlobs();
            bool hasHole = false;

            if (results != null)
            {
                foreach (CogBlobResult b in results)
                {
                    if (b.Area >= minArea)
                    {
                        hasHole = true;
                        break;
                    }
                }
            }

            // === 6️⃣ 결과 반환 ===
            return hasHole ? "하단(구멍 있음)" : "상단(구멍 없음)";
        }
    }
}

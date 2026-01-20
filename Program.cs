using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.PMAlign;
using ConsoleApp2.modules;

namespace ConsoleApp2
{
    internal class Program
    {
        static readonly string ROOT = @"C:\ConsoleApp2";
        static readonly string VPP = Path.Combine(ROOT, "train_PMAlign.vpp");
        static readonly string RESULT_DIR = Path.Combine(ROOT, "result");
        static readonly string[] DATA_DIRS = { "Bush_Image", "WD780", "테스트이미지" };

        // 👉 진짜 로직은 여기
        static int Run()
        {
            Console.WriteLine("=== ConsoleApp2 PMAlign 실행 ===");
            Console.WriteLine($"[BUILD] {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            try
            {
                Directory.CreateDirectory(RESULT_DIR);

                if (!File.Exists(VPP))
                {
                    Console.WriteLine($"❌ 트레인셋(.vpp) 없음: {VPP}");
                    return 1;
                }

                object loaded = CogSerializer.LoadObjectFromFile(VPP);
                var pma = ExtractPMAToolDynamic(loaded);
                if (pma == null)
                {
                    Console.WriteLine("❌ PMAlignTool을 찾지 못했습니다.");
                    return 2;
                }
                Console.WriteLine("✅ PMAlign 툴 로드 완료");

                // --- 이미지 선택 ---
                var images = DATA_DIRS.SelectMany(d => SafeFiles(Path.Combine(ROOT, d)))
                    .Where(f => HasExt(f, ".bmp", ".jpg", ".jpeg", ".png", ".tif", ".tiff"))
                    .ToArray();
                if (images.Length == 0)
                {
                    Console.WriteLine("⚠️ 이미지 없음");
                    return 3;
                }
                string imagePath = images[new Random().Next(images.Length)];
                Console.WriteLine($"📸 사용 이미지: {imagePath}");

                // --- 이미지 로드 ---
                var image = LoadImage(imagePath);
                pma.InputImage = image;

                // 🔹 검색영역 전체로 강제 확장
                int imgW = 0, imgH = 0;
                if (image is CogImage8Grey g8)
                {
                    imgW = g8.Width; imgH = g8.Height;
                }
                else if (image is CogImage24PlanarColor c24)
                {
                    imgW = c24.Width; imgH = c24.Height;
                }

                if (imgW > 0 && imgH > 0)
                {
                    var full = new CogRectangleAffine();
                    full.SetCenterLengthsRotationSkew(imgW / 2.0, imgH / 2.0, imgW, imgH, 0, 0);
                    pma.SearchRegion = full;
                }

                pma.Run();

                int n = pma.Results?.Count ?? 0;
                Console.WriteLine($"🔎 검출 개수: {n}");

                if (n == 0)
                {
                    string outNoMatch = Path.Combine(RESULT_DIR, $"nomatch_{Path.GetFileName(imagePath)}");
                    SaveOriginalWithNote(imagePath, outNoMatch, "NO MATCH");
                    Console.WriteLine($"💾 저장(무검출): {outNoMatch}");

                    return 0;
                }

                var poses = pma.Results.Cast<CogPMAlignResult>()
                    .Select(r => new { Pose = r.GetPose(), Score = r.Score })
                    .ToArray();

                int up = 0, down = 0;
                foreach (var (pose, idx) in poses.Select((v, i) => (v, i)))
                {
                    double x = pose.Pose.TranslationX;
                    double y = pose.Pose.TranslationY;
                    double angle = CogMisc.RadToDeg(pose.Pose.Rotation);
                    double score = pose.Score;

                    Console.WriteLine($"#{idx:D2} X={x:F2}, Y={y:F2}, A={angle:F2}°, S={score:F3}");

                    string result = BrightnessAnalyzer.AnalyzeFromFile(imagePath, x, y);
                    Console.WriteLine($"   → 구분: {result}");
                    if (result.Contains("상단")) up++;
                    else if (result.Contains("하단")) down++;
                }

                Console.WriteLine($"📊 상단={up}, 하단={down}");

                // (선택) 오버레이 저장
                string outPath = Path.Combine(RESULT_DIR, $"result_{Path.GetFileName(imagePath)}");
                TryDrawOverlayAndSave(imagePath, outPath, poses);
                Console.WriteLine($"💾 저장: {outPath}");

                Console.WriteLine("\n▶ Enter 키를 누르면 종료됩니다...");
                Console.ReadLine();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 예외: {ex}");
                Console.WriteLine("\n▶ Enter 키를 누르면 종료됩니다...");
                Console.ReadLine();
                return -1;
            }
        }

        // 👉 여기서부터는 Helper 함수들 (원래 코드 그대로)

        static Bitmap LoadAs24bpp(string path)
        {
            using (var src = (Bitmap)Image.FromFile(path))
            {
                if (src.PixelFormat == PixelFormat.Format24bppRgb)
                    return (Bitmap)src.Clone();
                var dst = new Bitmap(src.Width, src.Height, PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(dst))
                    g.DrawImage(src, 0, 0, src.Width, src.Height);
                return dst;
            }
        }

        static void SaveOriginalWithNote(string srcPath, string outPath, string note)
        {
            using (var bmp = LoadAs24bpp(srcPath))
            using (var g = Graphics.FromImage(bmp))
            using (var f = new Font("Segoe UI", 28, FontStyle.Bold))
            using (var b = new SolidBrush(Color.Red))
            {
                g.DrawString(note, f, b, 20, 20);
                bmp.Save(outPath, ImageFormat.Jpeg);
            }
        }

        static void TryDrawOverlayAndSave(string srcPath, string outPath, dynamic[] poses)
        {
            using (var bmp = LoadAs24bpp(srcPath))
            using (var g = Graphics.FromImage(bmp))
            using (var pen = new Pen(Color.Lime, 2))
            using (var font = new Font("Segoe UI", 12, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.Yellow))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                foreach (var (p, i) in poses.Select((v, i) => (v, i)))
                {
                    float x = (float)p.Pose.TranslationX;
                    float y = (float)p.Pose.TranslationY;
                    g.DrawEllipse(pen, x - 10, y - 10, 20, 20);
                    g.DrawString($"#{i:D2}", font, brush, x + 10, y + 10);
                }
                bmp.Save(outPath, ImageFormat.Jpeg);
            }
        }

        static ICogImage LoadImage(string path)
        {
            var f = new CogImageFile();
            f.Open(path, CogImageFileModeConstants.Read);
            var img = f[0];
            f.Close();
            return img;
        }

        static CogPMAlignTool ExtractPMAToolDynamic(object o)
        {
            if (o is CogPMAlignTool p) return p;
            var type = o.GetType();
            var toolsProp = type.GetProperty("Tools");
            if (toolsProp != null)
            {
                foreach (var t in (System.Collections.IEnumerable)toolsProp.GetValue(o))
                    if (t is CogPMAlignTool r) return r;
            }
            return null;
        }

        static string[] SafeFiles(string dir) =>
            Directory.Exists(dir) ? Directory.GetFiles(dir) : Array.Empty<string>();

        static bool HasExt(string p, params string[] exts) =>
            exts.Any(x => p.EndsWith(x, StringComparison.OrdinalIgnoreCase));

        // 👉 진짜 엔트리포인트(Main)는 여기 한 줄
        static void Main(string[] args)
        {
            int code = Run();
            Environment.Exit(code);
        }
    }
}

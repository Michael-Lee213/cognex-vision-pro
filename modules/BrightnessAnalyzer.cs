using System;
using System.Drawing;

namespace ConsoleApp2.modules
{
    public static class BrightnessAnalyzer
    {
        public static string AnalyzeFromFile(string path, double cx, double cy, int roiSize = 100)
        {
            try
            {
                using (var bmp = new Bitmap(path))
                {
                    int x0 = Math.Max(0, (int)(cx - roiSize / 2));
                    int y0 = Math.Max(0, (int)(cy - roiSize / 2));
                    int w = Math.Min(roiSize, bmp.Width - x0);
                    int h = Math.Min(roiSize, bmp.Height - y0);
                    if (w <= 0 || h <= 0) return "분석불가(ROI 범위)";

                    double total = 0; int count = 0;
                    for (int y = y0; y < y0 + h; y += 2)
                        for (int x = x0; x < x0 + w; x += 2)
                        {
                            Color c = bmp.GetPixel(x, y);
                            total += (c.R + c.G + c.B) / 3.0;
                            count++;
                        }

                    double avg = total / count;
                    return avg < 120 ? "하단(구멍 있음)" : "상단(구멍 없음)";
                }
            }
            catch (Exception ex)
            {
                return $"분석 실패: {ex.Message}";
            }
        }
    }
}

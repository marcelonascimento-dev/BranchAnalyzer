using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

// Generate multi-size ICO file
var sizes = new[] { 16, 32, 48, 256 };
using var ms = new MemoryStream();
using var bw = new BinaryWriter(ms);

// ICO header
bw.Write((short)0);     // reserved
bw.Write((short)1);     // type: icon
bw.Write((short)sizes.Length); // count

var imageData = new List<byte[]>();
int offset = 6 + sizes.Length * 16; // header + directory entries

foreach (var size in sizes)
{
    var bmp = new Bitmap(size, size);
    using var g = Graphics.FromImage(bmp);
    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.Clear(Color.Transparent);

    float scale = size / 32f;

    using var bgBrush = new SolidBrush(Color.FromArgb(30, 80, 180));
    g.FillEllipse(bgBrush, 1*scale, 1*scale, 30*scale, 30*scale);

    using var pen = new Pen(Color.White, 2.2f*scale) { StartCap = LineCap.Round, EndCap = LineCap.Round };
    g.DrawLine(pen, 10*scale, 8*scale, 10*scale, 24*scale);
    g.DrawLine(pen, 10*scale, 14*scale, 22*scale, 8*scale);

    using var nodeBrush = new SolidBrush(Color.FromArgb(80, 220, 120));
    g.FillEllipse(nodeBrush, 7*scale, 5*scale, 6*scale, 6*scale);
    g.FillEllipse(nodeBrush, 7*scale, 21*scale, 6*scale, 6*scale);
    using var nodeBrush2 = new SolidBrush(Color.FromArgb(255, 180, 60));
    g.FillEllipse(nodeBrush2, 19*scale, 5*scale, 6*scale, 6*scale);

    using var pngMs = new MemoryStream();
    bmp.Save(pngMs, System.Drawing.Imaging.ImageFormat.Png);
    imageData.Add(pngMs.ToArray());
    bmp.Dispose();
}

// Write directory entries
for (int i = 0; i < sizes.Length; i++)
{
    bw.Write((byte)(sizes[i] >= 256 ? 0 : sizes[i])); // width
    bw.Write((byte)(sizes[i] >= 256 ? 0 : sizes[i])); // height
    bw.Write((byte)0);    // color palette
    bw.Write((byte)0);    // reserved
    bw.Write((short)1);   // color planes
    bw.Write((short)32);  // bits per pixel
    bw.Write(imageData[i].Length); // size
    bw.Write(offset);     // offset
    offset += imageData[i].Length;
}

// Write image data
foreach (var data in imageData)
    bw.Write(data);

File.WriteAllBytes("app.ico", ms.ToArray());
Console.WriteLine("app.ico created: " + ms.Length + " bytes");

using System.Drawing.Imaging;
using System.Media;

namespace ImageToIcon
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Browse Button Click Event
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            // Open file dialog to select image
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.png; *.jpg)|*.png;*.jpg"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Populate text box with the selected file path
                txtFilePath.Text = openFileDialog.FileName;
            }
        }
        // Convert Button Click Event
        private void btnConvert_Click(object sender, EventArgs e)
        {
            string filePath = txtFilePath.Text;
            string outPath = null;
            if (filePath.ElementAt(filePath.Length - 4) == '.') // 3 letter affix: png, jpg, etc
            {
                outPath = filePath.Substring(0, filePath.Length - 3) + "ico";
            }
            else
            {
                outPath = filePath.Substring(0, filePath.Length - 4) + "ico";   // 4 letter affix: jpeg 
            }
            // Check if a file was selected
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show("Please select a valid image file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var image = Image.FromFile(filePath))
            {
                // Resize the image if it's not square or not in an ideal size for icons
                var resizedImage = Utilities.ResizeImage(image, 256, 256);  // Resize to 256x256 for ICO format

                // Save the image as an ICO
                using (var fs = new FileStream(outPath, FileMode.Create))
                {
                    Utilities.SaveAsIcon(resizedImage, fs);
                }
            }

            txtFilePath.Text = "Conversion complete...";
            SystemSounds.Asterisk.Play();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
    }
    public static class Utilities
    {
        public static Bitmap ResizeImage(Image img, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(img.HorizontalResolution, img.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(img, destRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }
        public static void SaveAsIcon(Bitmap image, Stream outputStream)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);  // ICO is a special case and can accept PNG for modern icons
                var pngBytes = ms.ToArray();

                using (var writer = new BinaryWriter(outputStream))
                {
                    // Write ICO header
                    writer.Write((short)0);    // Reserved, should be 0
                    writer.Write((short)1);    // ICO type
                    writer.Write((short)1);    // Number of images

                    // Write the ICO directory entry
                    writer.Write((byte)image.Width);  // Width
                    writer.Write((byte)image.Height); // Height
                    writer.Write((byte)0);   // Number of colors (0 is no palette)
                    writer.Write((byte)0);   // Reserved
                    writer.Write((short)1);  // Color planes
                    writer.Write((short)32); // Bits per pixel
                    writer.Write(pngBytes.Length); // Size of the PNG data
                    writer.Write(22);        // Offset of the PNG data (header size is 22 bytes)

                    writer.Write(pngBytes);
                }
            }
        }
    }
}
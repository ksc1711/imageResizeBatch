using ImageMagick;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;

namespace ContentsManager.ImageSave
{
    static class ImageSave
    {
        public static void ImageResize(StringBuilder _sb,  FileInfo _fi, Image _image)
        {
            int ResizeNum = Convert.ToInt32(ConfigurationManager.AppSettings["ResizeNum"]);
            
            bool FolderDesignate = Convert.ToBoolean(ConfigurationManager.AppSettings["ResizeFolder_Designate"]);
            string ResizeFolder = ConfigurationManager.AppSettings["ResizeFolder"].ToString();

            // target folder가 없을 시 폴더 생성
            if (!Directory.Exists(ResizeFolder))
                Directory.CreateDirectory(ResizeFolder);

            for (int i = 1; i <= ResizeNum; i++)
            {
                _sb.Clear();
                
                string ImageWidth = "ImageWidth" + i;
                string ImageHeight = "ImageHeight" + i;

                int rImageWidth = Convert.ToInt32(ConfigurationManager.AppSettings[ImageWidth]);
                int rImageHeight = Convert.ToInt32(ConfigurationManager.AppSettings[ImageHeight]);

                using (Bitmap imgBitmap = new Bitmap(rImageWidth, rImageHeight))
                {
                    // * resize
                    Graphics graphics = Graphics.FromImage(imgBitmap);
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    graphics.DrawImage(_image, 0, 0, rImageWidth, rImageHeight);
                    //filename.
                    string file = _fi.Name.Clone().ToString();
                    file = file.Substring(0, file.LastIndexOf('.'));

                    //resize folder 지정
                    string paths = string.Empty;
                    if(FolderDesignate)
                        paths = _sb.AppendFormat("{0}\\{1}({2}){3}", ResizeFolder, file, i, _fi.Extension).ToString();
                    else
                        paths = _sb.AppendFormat("{0}\\{1}({2}){3}", _fi.DirectoryName, file, i, _fi.Extension).ToString();

                    switch(_fi.Extension)
                    {
                        case ".gif":
                            {
                                using (MagickImageCollection collection = new MagickImageCollection(_fi))
                                {
                                    // This will remove the optimization and change the image to how it looks at that point
                                    // during the animation. More info here: http://www.imagemagick.org/Usage/anim_basics/#coalesce
                                    collection.Coalesce();

                                    // Resize each image in the collection to a width of 200. When zero is specified for the height
                                    // the height will be calculated with the aspect ratio.
                                    foreach (MagickImage image in collection)
                                    {
                                        image.Resize(rImageWidth, rImageHeight);
                                    }

                                    // Save the result
                                    collection.Write(paths);
                                }
                            }
                            break;
                        default:
                            imgBitmap.Save(paths);
                            break;
                    }
                    //imgBitmap.Save(paths);
                    //, System.Drawing.Imaging.ImageFormat.Jpeg
                    graphics.Dispose();
                }
            }
        }
    }
}

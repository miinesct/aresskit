using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace aresskit
{
    class FileHandler
    {
        public static string downloadFileToTemp(string url)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2 solution for .Net 4.0
                    var result = client.DownloadData(url);
                    string fileName = "";

                    // Try to extract the filename from the Content-Disposition header
                    if (!String.IsNullOrEmpty(client.ResponseHeaders["Content-Disposition"]))
                    {
                        fileName = client.ResponseHeaders["Content-Disposition"].Substring(client.ResponseHeaders["Content-Disposition"].IndexOf("filename=") + 9).Replace("\"", "");
                    }
                    string FilePath = System.IO.Path.GetTempPath() + Misc.RandomString(16) + fileName;
                    File.WriteAllBytes(FilePath, result);
                    return FilePath + " Exist";
                }
                catch (WebException ex)
                {
                    return ex.Message;
                }
            }
        }
        public static string downloadFile(string filename, string url)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2 solution for .Net 4.0
                    client.DownloadFile(url, filename);

                    return filename + " Exist";
                }
                catch (WebException ex)
                {
                    return ex.Message;
                }
                // File downloaded
            }
        }

        public static string uploadFile(string filename, string url)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2 solution for .Net 4.0
                    byte[] responseArray = client.UploadFile(url, filename);
                    return System.Text.Encoding.ASCII.GetString(responseArray);
                }
                catch (WebException)
                {
                    return "Upload failed";
                }
                // File uploaded
            }
        }

    }
}

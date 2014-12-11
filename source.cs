using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace TenLuaLibrary
{
    public class Tenlua
    {
    /* Author: Pham Quoc Thang
       Email: pycoder@live.com
    */
        public string username = "";
        public string password = "";
        /* Get ID From Link
         * Example: String id = get_id("https://www.tenlua.vn/download/1b37e227e50a6f06/modemcombat-5-512ram");
         * Return: 1b37e227e50a6f06
        */
        public string get_id(string link)
        {
            string idfile = "";
            string splitstr = "";
            link = link.Replace("#download", "/download/");
            if (link.Contains("download/"))
            {
                splitstr = "/download/";
            }
            else if (link.Contains("folder/"))
            {
                splitstr = "/folder/";
            }
            else
            {
                // not found
            }
            string[] cat = Regex.Split(link, splitstr);
            if (cat[1].Contains("/"))
            {
                string[] cat2 = Regex.Split(cat[1], "/");
                idfile = cat2[0];
            }
            else
            {
                idfile = cat[1];
            }
            return idfile;
        }
        /*
         * Example: string session = get_id(link_file);
         * Return: Session logged in
         */
        public string get_session()
        {

            string session = send_request("http://api.tenlua.vn/", "[{\"a\":\"user_login\",\"user\":\""+username+"\",\"password\":\""+password+"\",\"permanent\":true}]");
            session = session.Replace("\"", "");
            session = session.Replace("[", "");
            session = session.Replace("]", "");

            if (session == "" || session == "-9")
            {
                return null;
            }
            return session;
        }
        /* Get direct link VIP Max Speed
         * Example: String direct_link = get_link_file("https://www.tenlua.vn/download/1b37e227e50a6f06/modemcombat-5-512ram");
         * Return: Direct link
        */
        public string get_link_file(string link)
        {
            string page = send_request("http://api.tenlua.vn?sid=" + get_session(), "[{\"a\":\"filemanager_builddownload_getinfo\",\"n\":\"" + get_id(link) + "\",\"r\":0.376080396333181}]");
            if (page == "Time Out")
            {
                return null;
            }
            try
            {
                string[] xx = Regex.Split(page, "\"dlink\":\"");
                string[] xxx = Regex.Split(xx[1], "\",\"n\"");
                string direct_link = get_string_between(page, "\"dlink\":\"", "\"").Replace("\\/", "/");
                return direct_link;
            }
            catch
            {
                return null;
            }             
        }
        /* Get all link in folder
         * Example: string[] List_link = get_link_in_folder("https://tenlua.vn/fm/folder/0e37e62fe60a6d0314/quang-cao-1");
         * Return array all link
        */
        public string[] get_link_in_folder(string link)
        {
            string[] list_link = null;
            string page = send_request("http://api.tenlua.vn?sid=" + get_session(), "[{\"a\":\"filemanager_builddownload_getinfo\",\"n\":\"" + get_id(link) + "\",\"r\":0.376080396333181}]");
            if (page.Contains("type\":\"folder\""))
            {
                string[] arr = Regex.Split(page, "\"link\"");
                int i = 0;
                for (int x = 2; x < arr.Count(); x++)
                {
                    if (arr[x].Contains("http:"))
                    {
                        string linkfile = get_string_between(arr[x], ":\"", "\"");
                        linkfile = linkfile.Replace("\\", "");
                        string namefile = get_string_between(arr[x], "\"name\":\"", "\"");
                        int filesize = Int32.Parse(get_string_between(arr[x], "\"real_size\":\"", "\""));
                        filesize = filesize / 1024;
                        string fsize;

                        if (filesize < 1024)
                        {
                            fsize = filesize + " KB";
                        }
                        else if (filesize < 10240)
                        {
                            fsize = filesize / 1024 + " MB";
                        }
                        else
                        {
                            fsize = filesize / 1024 / 1024 + " GB";
                        }
                        list_link[i] = linkfile + "tlsplit" + namefile + "tlsplit" + fsize;
                        i++;
                    }
                }
            }
            return list_link;
        }
        /* Get info file from link
         * Example: String info = get_info_file(string link);
         * Return: Name File tlsplit File Size
        */
        public string get_info_file(string link)
        {
            string namefile = null; string filesize = null;
            string type = send_request("http://api2.tenlua.vn/", "[{\"a\":\"filemanager_builddownload_getinfo\",\"n\":\"" + get_id(link) + "\",\"r\":0.7345157842396046}]");
            if (type == "Time Out")
            {
                return null;
            }
            if (type.Contains("type\":\"file\""))
            {

                namefile = get_string_between(type, "\"n\":\"", "\"");
                filesize = get_string_between(type, "\"s\":\"", "\"");
            }
            return namefile + "tlsplit" + filesize;
        }

        public string get_string_between(string input, string start, string end)
        {
            string output = "";
            if (input.IndexOf(start) != -1)
            {
                string[] split = Regex.Split(input, start);
                string[] split2 = Regex.Split(split[1], end);
                output = split2[0];
            }
            return output;
        }
        public string send_request(string url, string postData)
        {
            try
            {

                WebRequest request = WebRequest.Create(url);
                request.Timeout = 100000000;
                request.Method = "POST";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;

                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                reader.Close();
                dataStream.Close();
                response.Close();
                return responseFromServer;
            }
            catch
            {
                //MessageBox.Show("Time out");
                return "Time Out";
            }
        }
    }
}

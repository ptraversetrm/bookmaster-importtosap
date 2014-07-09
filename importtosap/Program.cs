using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace importtosap
{
    class Program
    {
        static void Main(string[] args)
        {
            // making a curl call
            string url = "http://corporate.discoverbooks.com/interfaces/admin/reporting/settlements_import_to_sap.php?partner_id=2&signature=D1sc0vEr";

            HttpWebRequest req;
            NetworkCredential myCred = new NetworkCredential("api_user", "thedevteamlikestocurl");
            req = (HttpWebRequest)WebRequest.Create(url);

            req.PreAuthenticate = true;
            req.Credentials = myCred;
            req.Method = "GET";
            req.ContentType = "application/x-www-form-urlencoded";

            HttpWebResponse WebResp = (HttpWebResponse)req.GetResponse();

            int status_code_first_curl = (int)WebResp.StatusCode;
            if (status_code_first_curl == 200)
            {
                //get response from discover books server
                string response = new StreamReader(WebResp.GetResponseStream()).ReadToEnd();
                //Console.WriteLine("Response" + response);
                string[] words = response.Split(',');

                foreach (string word in words)
                {
                    if (word.Length != 0)
                    {
                        //complete file path
                        string[] complete_file_name = word.Split('/');

                        //get file_name
                        string file_name = complete_file_name[6];

                        // extract account_id
                        int num_underscore = file_name.IndexOf('_');
                        string account_id = file_name.Substring(num_underscore);
                        account_id = account_id.Replace("_", "");
                        account_id = account_id.Replace(".xml", "");

                        /*
                        Console.WriteLine("Complete file name: "+file_name);
                        Console.WriteLine("account_id : " + account_id);
                        */

                        //making another curl call to get the file contents 
                        string file_url = "http://corporate.discoverbooks.com/files/reports/settlement_xml/" + complete_file_name[6];

                        req = (HttpWebRequest)WebRequest.Create(file_url);
                        req.PreAuthenticate = true;
                        req.Credentials = myCred;
                        req.Method = "GET";
                        req.ContentType = "application/x-www-form-urlencoded";

                        HttpWebResponse file_response = (HttpWebResponse)req.GetResponse();

                        int status_code = (int)WebResp.StatusCode;

                        // if successfull
                        if (status_code == 200)
                        {
                            string path_string = "";
                            //get file contents
                            string file_content = new StreamReader(file_response.GetResponseStream()).ReadToEnd();

                            // select path based on account_id
                            if (account_id == "9" || account_id == "158")
                            {
                                path_string = @"C:\Uploads\TRM\OrderProc\xml";
                            }
                            else if (account_id == "6" || account_id == "11" || account_id == "12" || account_id == "14" || account_id == "16" || account_id == "18" || account_id == "19" || account_id == "120" || account_id == "124")
                            {
                                path_string = @"C:\Uploads\NCH\OrderProc\xml";
                            }

                            // Use Combine again to add the file name to the path.
                            path_string = System.IO.Path.Combine(path_string, file_name);

                            // write to the file
                            try
                            {
                                System.IO.File.WriteAllText(path_string, file_content);
                            }
                            catch (System.IO.IOException e)
                            {
                                //Console.WriteLine(e.Message);
                            }
                        }
                    }
                }
                // Keep the console window open in debug mode.         
                // Console.ReadLine();
            }
            else
            {
               // Console.WriteLine("No Response");
            }
        }
    }
}

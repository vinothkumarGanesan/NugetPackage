using System;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Diagnostics;
using System.Text.Json;
using System.Data.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AddGitHubAction
{
    class Program
    {
        protected string content = Program.ReadContent();

        Random rnd = new Random();

        protected string token = String.Empty;
        static void Main(string[] args)
        {
            Program program = new Program();
            program.token = args[1];
            List<string> repositoriesList = program.GetRepositories(args[0]);
            program.AddGitHubActionFile(args[0],repositoriesList);

        }

        /// <summary>
        /// AddGitHubActionFile method add GitHubAction file to the github repository if not present in the directory .github/workflows .
        /// </summary>
        /// <param name="organization">Name of the organization</param>
        /// <param name="token">Tokens you have generated that can be used to access the GitHub API.</param>
        /// <param name="repositoryList">list of the repositories present in the organisation</param>
        protected void AddGitHubActionFile(string organization, List<string> repositoryList)
        {


            Parallel.ForEach(repositoryList, repository =>
            {

                    string repos = CurlCommand("curl -X GET  https://api.github.com/repos/" + organization + "/" + repository + "/contents/.github/workflows/gitleaks.yaml  -u"+token+"");

                    //parsing json to jsondocument
                    using JsonDocument doc = JsonDocument.Parse(repos);
                    JsonElement root = doc.RootElement;

                    var temp = root;

                    try
                    {
                        string tempobject = (temp.GetProperty("message")).ToString();

                        if (tempobject == "Not Found")
                        {
                           
                            AddFile(organization, repository);
                        }
                    }
                    catch (System.Collections.Generic.KeyNotFoundException e)
                    {
                       
                    }
               

            });

        }

        /// <summary>
        /// GetRepositories method will return the list of repositories present the organization 
        /// </summary>
        /// <param name="organization">Name of the organization</param>
        /// <param name="token">Tokens you have generated that can be used to access the GitHub API.</param>
        /// <returns>list of repositories present in the organization</returns>
        protected List<string> GetRepositories(string organization)
        {
           
            bool check = true;
            int pageCount = 0;
            List<String> repositoryList = new List<String>();
            while (check)
            {
                pageCount = pageCount + 1;

                //get repository list
                string json = CurlCommand("curl -H \"Accept: application/vnd.github.v3+json\" -u " + token + "  https://api.github.com/orgs/" + organization + "/repos?&per_page=100&page=" + pageCount + "");


                //parsing json to jsondocument
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                
                int x = root.GetArrayLength();

                if (json == "[\n\n]\n")
                {
                    check = false;
                }


                //adding the repository name to list
                for (int i = 0; i < root.GetArrayLength(); i++)
                {
                    var temp = root[i];
                    repositoryList.Add((temp.GetProperty("name")).ToString());
                }

            }

 
            return repositoryList;
        }

        /// <summary>
        /// AddFile method will add the github action file to the repository branch
        /// </summary>
        /// <param name="organization">Name of the organization</param>
        /// <param name="repository">Name of the organization</param>
        /// <param name="token">Tokens you have generated that can be used to access the GitHub API.</param>
        protected void AddFile(string organization, string repository)
        {


            
            string content = ReadContent();
            string randomNumber = rnd.Next(100000).ToString();
            string data = "{\r\n \"message\": \"a new commit message\",\r\n  \"content\": \"" + content + "\" \r\n}";
            File.WriteAllText("Data/" + repository + "-"+randomNumber+".json", data);

            string yaml = CurlCommand("curl -X PUT  https://api.github.com/repos/" + organization + "/" + repository + "/contents/.github/workflows/gitleaks.yaml -u " + token + "    -H \"Accept: application/vnd.github+json\" -H \"Content-Type: application/json\" -d @Data/" + repository +"-"+randomNumber+".json");

            
        }
        /// <summary>
        /// ReadContent method will read the txt from the text file and convert into encoded text  
        /// </summary>
        /// <returns>Returns the encoded content</returns>
        protected static string ReadContent()
        {
            string text = File.ReadAllText("gitleaks.txt");
            var encodedtext = System.Text.Encoding.UTF8.GetBytes(text);
            return System.Convert.ToBase64String(encodedtext);

        }
        /// <summary>
        /// FileCreate method create directory and json file which passes the data for the curl command  
        /// </summary>
        protected void FileCreate()
        {
            //creating directory and files
            if (!Directory.Exists("Data"))
            {
                var mydir = Directory.CreateDirectory("Data");
            }
         
        }
   
        /// <summary>
        /// curlcommand method executes the curl command
        /// </summary>
        /// <param name="commandLinearguments">curl command </param>
        /// <returns>string log the curl</returns>
        protected string CurlCommand(string commandLinearguments)
        {
            Process commandProcess = new Process();
            commandProcess.StartInfo.UseShellExecute = false;
            commandProcess.StartInfo.FileName = @"C:\Program Files\Git\mingw64\bin\curl.exe"; //// this is the path of curl where it is installed;    
            commandProcess.StartInfo.Arguments = commandLinearguments;
            commandProcess.StartInfo.CreateNoWindow = true;
            commandProcess.StartInfo.RedirectStandardInput = true;
            commandProcess.StartInfo.RedirectStandardOutput = true;
            commandProcess.StartInfo.RedirectStandardError = true;
            commandProcess.Start();
            string output = commandProcess.StandardOutput.ReadToEnd();


            commandProcess.WaitForExit();
            return output;
        }

       
    }

}
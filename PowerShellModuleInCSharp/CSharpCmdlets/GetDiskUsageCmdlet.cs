﻿﻿using PowerShellModuleInCSharp.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.UI;

namespace PowerShellModuleInCSharp.CSharpCmdlets
{
    [Cmdlet(VerbsCommon.Get, nameof(DiskUsage))]
    [OutputType(typeof(DiskUsage))]
    public class GetDiskUsageCmdlet : Cmdlet
    {
        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Alias("disk")]
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, ValueFromPipeline = true)]
        public string Drive { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
        public string Export { get; set; }

        private DateTime Date { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();



            DriveInfo drive = new DriveInfo(Drive);
            Console.WriteLine("Total Free Space: " + drive.TotalFreeSpace/(1024*1024*1024) + "GB");
            Console.WriteLine("Volume Label: "+drive.VolumeLabel);

            //File.WriteAllText(Export,TraverseTree(Drive));
            
            File.WriteAllText(Export, WriteTreeMap(Drive));
            



        }

        public static string TraverseTree(string root)
        {
            Stack<string> dirs = new Stack<string>(20);

            StringWriter stringWriter = new StringWriter();
            HtmlTextWriter writer = new HtmlTextWriter(stringWriter);
            
            Dictionary<string, float> folderSizes = new Dictionary<string, float>();

            
            if (!Directory.Exists(root))
            {
                throw new ArgumentException();

            }

            GetFolderSize(new DirectoryInfo(root), folderSizes);
            
            dirs.Push(root);
            DirectoryInfo tempDirectory = new DirectoryInfo(root);
            int dirDiff = 0;
            
            
            
            writer.RenderBeginTag(HtmlTextWriterTag.H1);
            writer.Write(new DirectoryInfo(root).Name);


            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;
                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (IOException e)
                {
                    continue;
                }

                
                subDirs = subDirs.Where(x => folderSizes.ContainsKey(new DirectoryInfo(x).FullName)).OrderBy(x => folderSizes[new DirectoryInfo(x).FullName]).ToArray();
                

                //subDirs = subDirs.OrderBy(x => GetFolderSize(new DirectoryInfo(x))).ToArray();
                foreach (string dir in subDirs)
                {
                    dirs.Push(dir);
                }
                
                


                dirDiff = tempDirectory.FullName.Split('\\').Length - 
                    new DirectoryInfo(currentDir).FullName.Split('\\').Length;

                if (dirDiff >= 0)
                {
                    for(int i = 0; i <= dirDiff; i++)
                    {
                        writer.RenderEndTag();
                        
                    }
                }

           
                tempDirectory = new DirectoryInfo(currentDir);

                string display = tempDirectory.FullName.Split('\\').Length -
                    new DirectoryInfo(root).FullName.Split('\\').Length > 3 ? "none" : "block"; //Show directory depth.
                
                
       

                //long folderSize = (long) GetFolderSize(new DirectoryInfo(currentDir));
                float folderSize = 1;
                float parentFolderSize = 100;
                
                try
                {
                    folderSize = folderSizes[new DirectoryInfo(currentDir).FullName];
                    if (currentDir.Equals(root))
                        parentFolderSize = folderSizes[new DirectoryInfo(currentDir).FullName];
                    else
                        parentFolderSize = folderSizes[new DirectoryInfo(currentDir).Parent.FullName];
                }
                catch (KeyNotFoundException e)
                {
                    continue;
                }


                if (Directory.GetParent(currentDir) != null)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Id,
                        Directory.GetParent(currentDir).Name + "_" + new DirectoryInfo(currentDir).Name);
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, new DirectoryInfo(currentDir).Name);
                }
             
                writer.AddAttribute(HtmlTextWriterAttribute.Style,
                    "border: 1px solid black; " +
                    "padding: 4px; " +
                    "float: left; " +
                    "text-align: left; " +
                    "font-size: 15px;" +
                    "overflow: hidden;" +
                    "position: relative;" +
                    "width:" + (int)((folderSize) / parentFolderSize*100) + "%;" +
                    "height:" + (int)((folderSize) / parentFolderSize*100) + "%;" +
                    "max-width: 90vw;" +
                    "max-height: 90vh;" +
                    "display: " + display + ";");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(new DirectoryInfo(currentDir).Name);
                writer.Write(folderSize/1000 + "GB");
                

                string[] files = null;

                try
                {
                    files = Directory.GetFiles(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (IOException e)
                {
                    continue;
                }
                /*
                if (files != null)
                {
                    foreach (string file in files)
                    {
                        try
                        {
                            FileInfo fi = new FileInfo(file);
                            double fileSize = (fi.Length / 1024f) / 1024f;

                            if (1 <= fileSize / folderSize * 100)
                            {
                                //fileSize = fileSize > 1000 ? 30 : 10;
                                writer.AddAttribute(HtmlTextWriterAttribute.Id, fi.Name);
                                writer.AddAttribute(HtmlTextWriterAttribute.Style,
                                    "border: 1px solid black; " +
                                    "padding: 4px; " +
                                    "float: right; " +
                                    "display: table; " +
                                    "text-align: left; " +
                                    "font-size: 15px;" +
                                    "width: " + (int)(fileSize / folderSize * 100) + "%;" +
                                    "height: " + (int)(fileSize / folderSize * 100) + "%;" +
                                    "background: lightblue;" +
                                    "overflow: hidden;");
                                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                                writer.Write(fi.Name);
                                writer.RenderEndTag();
                            }

                            //Console.WriteLine(fi.FullName);
                        }
                        catch (FileNotFoundException e)
                        {
                            //Console.WriteLine(e.Message);
                            continue;
                        }
                    }
                }*/


            }

            dirDiff = tempDirectory.FullName.Split('\\').Length -
                    new DirectoryInfo(root).FullName.Split('\\').Length;

            for (int i = 0; i <= dirDiff; i++)
                writer.RenderEndTag();

            return stringWriter.ToString();


        }

        public static string WriteTreeMap(String root)
        {
            StringWriter stringWriter = new StringWriter();
            HtmlTextWriter writer = new HtmlTextWriter(stringWriter);
            
            
            Dictionary<string, float> folderSizes = new Dictionary<string, float>();
            
            GetFolderSize(new DirectoryInfo(root), folderSizes);
            
            writer.RenderBeginTag(HtmlTextWriterTag.Head);
            
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
            writer.AddAttribute(HtmlTextWriterAttribute.Src, "https://www.gstatic.com/charts/loader.js");
            writer.RenderBeginTag(HtmlTextWriterTag.Script);
            writer.RenderEndTag();
            
            /*
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
            writer.AddAttribute(HtmlTextWriterAttribute.Src, "https://www.google.com/jsapi");
            writer.RenderBeginTag(HtmlTextWriterTag.Script);
            writer.RenderEndTag();
            
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
            writer.RenderBeginTag(HtmlTextWriterTag.Script);
            writer.Write("google.charts.load('current', {packages: ['treemap']});");
            writer.RenderEndTag();
            */
            
            
            writer.RenderEndTag();
            
            writer.RenderBeginTag(HtmlTextWriterTag.Body);
            
            writer.AddAttribute(HtmlTextWriterAttribute.Id, "container");
            writer.AddAttribute(HtmlTextWriterAttribute.Style,
                "width: 900px;" +
                "height: 500px;" +
                "margin: 0 auto;");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderEndTag();
            
            writer.RenderBeginTag(HtmlTextWriterTag.Script);
            //writer.WriteAttribute("language", "Javascript");
            
            writer.WriteLine("google.charts.load('current', {'packages':['treemap']});");
            writer.WriteLine("google.charts.setOnLoadCallback(drawChart);");
            writer.WriteLine("function drawChart() {");
            //writer.WriteLine("var data = new google.visualization.DataTable();");
            writer.WriteLine("var data = google.visualization.arrayToDataTable([");
            writer.WriteLine(
                "['Path', 'Parent', 'FolderSize', 'color'],");
            writer.WriteLine("['"+new DirectoryInfo(root).FullName.Replace("\\","\\\\")+"', null, "+folderSizes[new DirectoryInfo(root).FullName]+", 0],");
            foreach (KeyValuePair<string, float> folder in folderSizes)
            {
                if (new DirectoryInfo(folder.Key).FullName != new DirectoryInfo(root).FullName)
                    writer.WriteLine("[\"" + folder.Key.Replace("\\","\\\\") + "\", \"" + new DirectoryInfo(folder.Key).Parent.FullName.Replace("\\","\\\\") +"\", " +
                                     folder.Value+ ", 0],");
            }
            writer.WriteLine("]);");
            writer.WriteLine("var options = {");
            writer.WriteLine("minColor: '#f00',");
            writer.WriteLine("midColor: '#ddd',");
            writer.WriteLine("maxColor: '#0d0',");
            writer.WriteLine("headerHeight: 15,");
            writer.WriteLine("fontColor: 'black',");
            writer.WriteLine("showScale: true");
            writer.WriteLine("};");
            writer.WriteLine("var chart = new google.visualization.TreeMap(document.getElementById('container'));");
            writer.WriteLine("chart.draw(data, options);");
            writer.WriteLine("}");
            //writer.WriteLine("google.charts.setOnLoadCallback(drawChart);");
            
            writer.RenderEndTag();
            
            writer.RenderEndTag();

            return stringWriter.ToString();
        }
        
        public static float GetFolderSize(DirectoryInfo path, Dictionary<string,float> folderSizes)
        {

            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;

            float folderSize = 0.0f;


            // process all the files directly under this folder
            try
            {
                files = path.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message+ " RECURSE");
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
               Console.WriteLine(e.Message+ " RECURSE");
            }
            catch (IOException e)
            {
                
            }

            if (files != null)
            {
                foreach (FileInfo file in files)
                {
                    float fileSize = (file.Length / 1024f) / 1024f;
                    folderSize += fileSize;
                }

            }

            try
            {
                subDirs = path.GetDirectories();
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message+ " RECURSE");
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message + " RECURSE");
            }
            catch (IOException e)
            {
                
            }

            if (subDirs != null)
            {
                foreach (DirectoryInfo subDir in subDirs)
                {
                    // Resursive call for each subdirectory.
                    folderSize += GetFolderSize(subDir, folderSizes);
                }
                folderSizes.Add(path.FullName, folderSize);
            }

            return folderSize;

        }

       
    }
}

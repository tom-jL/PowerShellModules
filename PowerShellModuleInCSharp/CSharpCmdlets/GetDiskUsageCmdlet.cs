using PowerShellModuleInCSharp.Containers;
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

            File.WriteAllText(Export,TraverseTree(Drive));
            



        }

        public static string TraverseTree(string root)
        {
            Stack<string> dirs = new Stack<string>(20);

            StringWriter stringWriter = new StringWriter();
            HtmlTextWriter writer = new HtmlTextWriter(stringWriter);

            
            if (!Directory.Exists(root))
            {
                throw new ArgumentException();

            }
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

                foreach (string dir in subDirs)
                    dirs.Push(dir);
                

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
                    new DirectoryInfo(root).FullName.Split('\\').Length > 2 ? "none" : "inline-block";

                
                writer.AddAttribute(HtmlTextWriterAttribute.Id,
                    Directory.GetParent(currentDir).Name + "_" + new DirectoryInfo(currentDir).Name);
                writer.AddAttribute(HtmlTextWriterAttribute.Style, 
                    "border: 1px solid black; " +
                    "padding: 4px; " +
                    "float: left; " +
                    //"display: table; " +
                    "text-align: left; " +
                    "font-size: 15px;" +
                    "overflow: hidden;" +
                    "max-width: 1024px;" +
                    "max-height: 1024px;" +
                    "display: " + display + ";");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(new DirectoryInfo(currentDir).Name);



                string[] files = null;
                long folderSize = 0;
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

                foreach (string file in files)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(file);
                        folderSize += fi.Length / 1024 / 1024;
                        double fileSize = (fi.Length / 1024f) / 1024f;
                        
                        if (fileSize >= 1)
                        {
                            fileSize = fileSize > 1000 ? 30 : 10;
                            writer.AddAttribute(HtmlTextWriterAttribute.Id, fi.Name);
                            writer.AddAttribute(HtmlTextWriterAttribute.Style,
                                "border: 1px solid black; " +
                                "padding: 4px; " +
                                "float: left; " +
                                "display: flex; " +
                                "text-align: left; " +
                                "font-size: 15px;" +
                                "width: " + fileSize  + "px;" +
                                "height: " + fileSize  + "px;" +
                                "background: lightblue;" +
                                "overflow: hidden;");
                            writer.RenderBeginTag(HtmlTextWriterTag.Div);
                            writer.Write(fi.Name);
                            writer.RenderEndTag();
                        }

                        //Console.WriteLine(fi.FullName);
                    }
                    catch(FileNotFoundException e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }


            }

            dirDiff = tempDirectory.FullName.Split('\\').Length -
                    new DirectoryInfo(root).FullName.Split('\\').Length;

            for (int i = 0; i <= dirDiff; i++)
                writer.RenderEndTag();

            return stringWriter.ToString();


        }

       
    }
}

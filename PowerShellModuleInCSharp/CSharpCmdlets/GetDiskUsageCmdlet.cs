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
                  
            writer.WriteLine("google.charts.load('current', {'packages':['treemap']});");
            writer.WriteLine("google.charts.setOnLoadCallback(drawChart);");
            writer.WriteLine("function drawChart() {");
            //writer.WriteLine("var data = new google.visualization.DataTable();");
            writer.WriteLine("var data = google.visualization.arrayToDataTable([");
            writer.WriteLine(
                "['Path', 'Parent', 'FolderSize', 'color'],");
            
            writer.WriteLine("['"+ new DirectoryInfo(root).Name + " " + (folderSizes[new DirectoryInfo(root).FullName]/1024).ToString("0") + "GB" + 
                             "#" + new DirectoryInfo(root).FullName.Replace("\\","\\\\")+ "', null, "+folderSizes[new DirectoryInfo(root).FullName]+", 0],");
            foreach (KeyValuePair<string, float> folder in folderSizes)
            {
                if (new DirectoryInfo(folder.Key).FullName != new DirectoryInfo(root).FullName && folder.Value > 10)
                {
                    string size = folder.Value > 1024 ? (folder.Value / 1024).ToString("0") + "GB" : folder.Value.ToString("0") + "MB";
                    float parentSize = folderSizes[new DirectoryInfo(folder.Key).Parent.FullName];
                    string parentLabel = parentSize > 1024 ? (parentSize / 1024).ToString("0") + "GB" : parentSize.ToString("0") + "MB";
                    string name = new DirectoryInfo(folder.Key).Name;
                    string path = name + " " + size + "#" + folder.Key.Replace("\\", "\\\\");
                    string parentName = new DirectoryInfo(folder.Key).Parent.Name;
                    string parent = parentName + " " + parentLabel + "#" + new DirectoryInfo(folder.Key).Parent.FullName.Replace("\\", "\\\\");
                    float folderSize = folder.Value;
                    int color = folder.Key.Count(x => x == '\\') * 1000;
                    
                    writer.WriteLine("[\"" + path + "\", \"" + parent + "\", " + folderSize + ", " + color + "],");
                }
            }
                  

            writer.WriteLine("]);");
            writer.WriteLine("var options = {");
            writer.WriteLine("minColor: '#009688',");
            writer.WriteLine("midColor: '#f54242',");
            writer.WriteLine("maxColor: '#ee8100',");
            writer.WriteLine("headerHeight: 15,");
            writer.WriteLine("fontColor: 'black',");
            writer.WriteLine("showScale: false,");
            writer.WriteLine("maxDepth: 3,");
            writer.WriteLine("highlightOnMouseOver: true,");
            writer.WriteLine("minHighlightColor: '#8c6bb1',");
            writer.WriteLine("midHighlightColor: '#9ebcda',");
            writer.WriteLine("maxHighlightColor: '#edf8fb',");
            writer.WriteLine("generateTooltip: showStaticTooltip");
            writer.WriteLine("};");
            
            writer.WriteLine("var container = document.getElementById('container');");
            writer.WriteLine("var tree = new google.visualization.TreeMap(container);");
            writer.WriteLine("var newLabelCoords = {x: 8, y: 16};");
            
            writer.WriteLine("google.visualization.events.addListener(tree,'ready',moveOriginalLabels);");
            writer.WriteLine("google.visualization.events.addListener(tree,'select',moveOriginalLabels);");
            
            writer.WriteLine("var observer = new MutationObserver(moveOriginalLabels);");
            writer.WriteLine("observer.observe(container, { childList: true, subtree: true});");
            
            writer.WriteLine("function moveOriginalLabels() {");
            writer.WriteLine("Array.prototype.forEach.call(container.getElementsByTagName('text'), function(text) {");
            writer.WriteLine("var bounds = text.getBBox();");
            writer.WriteLine("var rect = text.parentNode.getElementsByTagName('rect')[0];");
            writer.WriteLine("var pathName = text.textContent.split('#');");
            writer.WriteLine("text.textContent = pathName[0];");
            writer.WriteLine("if ((rect.getAttribute('fill') !== '#cccccc') && (text.getAttribute('text-anchor') === 'middle')) {");
            writer.WriteLine("text.setAttribute('fill', '#424242');");
            writer.WriteLine("text.setAttribute('font-weight','bold');");
            writer.WriteLine("text.setAttribute('x', parseFloat(rect.getAttribute('x')) + newLabelCoords.x + (bounds.width / 2))");
            writer.WriteLine("text.setAttribute('y', parseFloat(rect.getAttribute('y')) + newLabelCoords.y);");
            writer.WriteLine("}");
            writer.WriteLine("});");
            writer.WriteLine("}");
                       
            writer.WriteLine("drawTree();");
            writer.WriteLine("window.addEventListener('resize',drawTree);");
            writer.WriteLine("function drawTree() {");
            writer.WriteLine("tree.draw(data, options);");
            writer.WriteLine("}");
            writer.WriteLine("function showStaticTooltip(row, size, value) {");
            writer.WriteLine("return \"<div style='background:#fd9; padding:10px; border-style:solid'>\" +");
            writer.WriteLine("data.getValue(row, 0).split('#')[1] + \"</div>\";");
            writer.WriteLine("}");
            writer.WriteLine("}");
            
           
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
                //Console.WriteLine(e.Message+ " RECURSE");
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
               //Console.WriteLine(e.Message+ " RECURSE");
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
                //Console.WriteLine(e.Message+ " RECURSE");
            }
            catch (DirectoryNotFoundException e)
            {
                //Console.WriteLine(e.Message + " RECURSE");
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

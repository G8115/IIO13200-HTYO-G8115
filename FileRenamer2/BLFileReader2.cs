using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FileRenamer2
{
    class BLFileReader2
    {
        private List<Dictionary<string, string>> fileInfo = new List<Dictionary<string, string>>();
        bool newFiles = false;
        public void updateFileInfo(TreeViewItem treeView, bool recursive)
        {//updates fileInfo list
            if (recursive)
                fileInfo = RecursiveFileSearch(treeView);
            else
            {
                fileInfo = NonRecursiveFileSearch(treeView);
            }
        }
        private List<Dictionary<string, string>> castFileInfo(FileInfo[] info)
        {//turns basic fileinfo into a List of Dictionaries
            List<Dictionary<string, string>> temp = new List<Dictionary<string, string>>();
            foreach (var item in info)
            {
                Dictionary<string, string> k = new Dictionary<string, string>();
                k.Add("Name", item.Name);
                k.Add("FullName", item.FullName);
                k.Add("Extension", item.Extension);
                temp.Add(k);
            }

            return temp;
        }
        private List<Dictionary<string, string>> NonRecursiveFileSearch(TreeViewItem treeView)
        {
            try
            {
                List<Dictionary<string, string>> info = new List<Dictionary<string, string>>();
                FileInfo[] temp = new DirectoryInfo(treeView.Tag.ToString()).GetFiles("*.*");
                info.AddRange(castFileInfo(temp));
                newFiles = true;
                return info;
            }
            catch (Exception)
            {

            }
            return null;
        }
        private List<Dictionary<string, string>> RecursiveFileSearch(TreeViewItem treeView)
        {
            List<Dictionary<string, string>> info = new List<Dictionary<string, string>>();
            //rekursiivinen tiedostojen haku
            try
            {
                FileInfo[] temp = new DirectoryInfo(treeView.Tag.ToString()).GetFiles("*.*");
                info.AddRange(castFileInfo(temp));
                foreach (TreeViewItem n in treeView.Items)
                {
                    info.AddRange(RecursiveFileSearch(n));
                }
                newFiles = true;
                return info;
            }
            catch (Exception) { }
            return null;
        }
        public List<string> getFileExtensions()
        {//returns a list of all distinct file extensions in found files
            if (fileInfo != null)
            {
                List<string> extensions = new List<string>();
                extensions.Add("*.");
                extensions.AddRange(fileInfo.Select(p => p["Extension"]).Distinct());
                return extensions;
            }
            return null;
        }
        public DataTable getFileInfoAsDataTable(List<string> filters, string name, bool extend)
        {//turns the list of fileinformation into a datatable.
            if (fileInfo.Count > 0)
            {
                if (newFiles && extend)
                {
                    extendFileInfo();
                    newFiles = false;
                }

                DataTable dataTable = new DataTable();
                //hakkaa filter stringin palasiksi ja poistaa mahdolliset tyhjät "" seasta
                filters.RemoveAll(str => String.IsNullOrEmpty(str));

                List<Dictionary<string, string>> templist = new List<Dictionary<string, string>>();
                List<Dictionary<string, string>> templist2 = new List<Dictionary<string, string>>();
                //first filter for name
                templist.AddRange(fileInfo.Where(k => k["Name"].Contains(name)));
                //the filter for file extension
                if (filters.Count() != 1 && !filters[0].Equals("*.*"))
                {
                    foreach (string filter in filters)
                    {
                        templist2.AddRange(templist.Where(k => k["Extension"].Equals(filter)));
                    }
                }
                else
                {//if there were no file extension filters
                    templist2 = templist;
                }
                //generate datatable columns
                foreach (string item in templist2[0].Keys)
                {
                    dataTable.Columns.Add(item);
                }
                //add rows to the datatable based on found data
                foreach (Dictionary<string, string> item in templist2)
                {
                    string tempString = "";
                    DataRow row = dataTable.NewRow();

                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        if (item.TryGetValue(dataTable.Columns[i].ColumnName, out tempString))
                            row[i] = tempString;
                        else
                            row[i] = null;
                    }

                    dataTable.Rows.Add(row);
                }

                return dataTable;
            }
            return null;
        }
        private void extendFileInfo()
        {
            extendProperties();

            //käydään kaikki sanakirjat läpi ja varmistetaan että niissä kaikissa on samat avaimet
            List<string> usedExtensions = new List<string>();
            foreach (Dictionary<string, string> item in fileInfo)
            {
                if (!usedExtensions.Contains(item["Extension"]))
                {
                    usedExtensions.Add(item["Extension"]);
                    foreach (Dictionary<string, string> e in fileInfo)
                    {
                        if (!e["Extension"].Equals(item["Extension"]))
                        {
                            //käy läpi kaikki item sanakirjan avaimet
                            foreach (string s in item.Keys)
                            {
                                //jos e sanakirjassa ei vielä ole avainta lisätään se sinne tyhjän arvon kanssa
                                if (!e.ContainsKey(s))
                                {
                                    e.Add(s, "");
                                }
                            }
                        }
                    }
                }
            }
        }
        private void extendProperties()
        {
            //hakee jokaiseen haettuun tiedostoon kaikki sen ylimääräiseset tiedostoinformaatiot
            string prevFilePath = "";
            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder folder = null;
            foreach (Dictionary<string, string> item in fileInfo)
            {
                string filePath = item["FullName"];
                string name = Path.GetDirectoryName(filePath);
                if (name != prevFilePath)
                {
                    folder = shell.NameSpace(name);
                    prevFilePath = name;
                }

                Shell32.FolderItem folderItem = folder.ParseName(Path.GetFileName(item["FullName"]));
                for (int i = 0; i < short.MaxValue; i++)
                {
                    string propValue = folder.GetDetailsOf(folderItem, i);
                    if (!propValue.Equals("") )
                    {
                        string propName = folder.GetDetailsOf(null, i);
                        if (String.IsNullOrEmpty(propName))
                        {
                            break;
                        }
                        if(!propName.Equals("Name") && !propName.Equals("Path"))
                            item.Add(propName, propValue);
                    }
                }
            }
        }
    }
}
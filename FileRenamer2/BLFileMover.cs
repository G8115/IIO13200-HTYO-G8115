using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileRenamer2
{
    class BLFileMover
    {//Handles renaming and moving of files
        int maxNumberOfFiles = 0;
        List<string> extensions = null;

        public bool renameOrMoveFiles(List<string> files
            , string whatToDo
            , string newFilePath
            , string oldFilePath
            , bool retain
            , bool separate
            , bool separateNames
            , string newName)
        {//the method that renames files and moves or copies them where wanted. 
            while (newFilePath[newFilePath.Count() - 1] == '\\')
                newFilePath.Remove(newFilePath.Count() - 1);

            List<file> f = new List<file>();

            foreach (string item in files)
            {
                f.Add(new file(item));
            }
            extensions = new List<string>(f.Select(p => p.Extension).Distinct());

            if (!renameAllFiles(f, separateNames, newName))
                return false;
            switch (whatToDo)
            {
                case "Rename"://rename files
                    foreach (file file in f)
                    {
                        try
                        {
                            System.IO.File.Move(file.OldName, file.Path + file.Name + file.Extension);
                        }
                        catch (Exception)
                        {

                        }
                    }
                    break;
                case "Copy"://copy files
                    if (retain)
                    {
                        if (generateOldStructure(f, newFilePath, oldFilePath))
                        {
                            foreach (file file in f)
                            {
                                try
                                {
                                    System.IO.File.Copy(file.OldName, file.Path + file.Name + file.Extension);
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                    }
                    else if (separate)
                    {
                        if (generateSeparateStructure(newFilePath))
                        {
                            foreach (file file in f)
                            {
                                try
                                {
                                    string temp = newFilePath + "\\" + file.Extension + "\\" + file.Name + file.Extension;
                                    System.IO.File.Copy(file.OldName, temp);
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (file file in f)
                        {
                            try
                            {
                                string temp = newFilePath + "\\" + file.Name + file.Extension;
                                System.IO.File.Copy(file.OldName, temp);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    break;
                case "Move"://move files
                    if (retain)
                    {
                        if (generateOldStructure(f, newFilePath, oldFilePath))
                        {
                            foreach (file file in f)
                            {
                                try
                                {
                                    System.IO.File.Move(file.OldName, file.Path + file.Name + file.Extension);
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                    }
                    else if (separate)
                    {
                        if (generateSeparateStructure(newFilePath))
                        {
                            foreach (file file in f)
                            {
                                try
                                {
                                    string temp = newFilePath + "\\" + file.Extension + "\\" + file.Name + file.Extension;
                                    System.IO.File.Copy(file.OldName, temp);
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (file file in f)
                        {
                            try
                            {
                                string temp = newFilePath + "\\" + file.Name + file.Extension;
                                System.IO.File.Move(file.OldName, temp);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    break;
            }
            return true;
        }

        private bool renameAllFiles(List<file> f, bool separateNames, string newName)
        {
            //calculate how big of a nuber is neede for all the files
            int tempNumberOfFiles = f.Count;
            maxNumberOfFiles = 1;
            while (tempNumberOfFiles >= 10)
            {
                tempNumberOfFiles = tempNumberOfFiles / 10;
                maxNumberOfFiles++;
            }
            List<string> names = new List<string>(newName.Split(' '));
            //detemine if the given new filename is good e.g. contains only 2 components etc.
            if (names.Count > 2)
                return false;
            if (names.Count == 1)
                names.Add("#");
            if (separateNames)
                renameSeparate(f, names[0], names[1]);
            else
                rename(f, names[0], names[1]);
            return true;
        }

        private void rename(List<file> files, string name, string numbering)
        {//manipulates file containers Name property
            int startNumber = analyzeNumbering(numbering);
            foreach (file item in files)
            {
                item.Name = name + "_" + getFileNumber(startNumber);
                startNumber++;
            }
        }
        private void renameSeparate(List<file> files, string name, string numbering)
        {//same as rename but separate numbering for separate file extensions

            //separate the files into their own lists before renaming
            List<List<file>> separate = new List<List<file>>();
            foreach (string s in extensions)
            {
                separate.Add(new List<file>(files.Where(x => x.Extension.Equals(s))));
            }
            foreach (List<file> l in separate)
            {
                rename(l, name, numbering);
            }
        }
        private string getFileNumber(int number)
        {//generate a numbers that looks like 00001 or 00178
            string temp = "" + number;
            while (temp.Count() < maxNumberOfFiles)
            {
                temp = '0' + temp;
            }
            return temp;
        }
        private int analyzeNumbering(string numbering)
        {//analyze user given numbering parameters and determine if they are valid
            //see if the user gave enough zeroes to the beginning of the number
            if (numbering.Count() > maxNumberOfFiles)
                maxNumberOfFiles = numbering.Count();
            string tempString = "";
            for (int i = 0; i < numbering.Count(); i++)
            {
                if (numbering[i] != '#')
                    tempString += numbering[i];
            }
            try
            {
                return Int32.Parse(tempString);
            }
            catch (Exception)
            {
                return 1;
            }
        }
        private bool generateOldStructure(List<file> files, string newFilePath, string oldFilePath)
        {//generates the old filestructure of the folder filse are beiing copied/moved from
            bool succes = true;
            foreach (file file in files)
            {
                try
                {
                    string temp = newFilePath + file.Path.Remove(0, oldFilePath.Count());
                    file.Path = temp;
                    Directory.CreateDirectory(temp);
                }
                catch (Exception)
                {
                    succes = false;
                }
            }
            return succes;
        }
        private bool generateSeparateStructure(string newFilePath)
        {//generates a folder for each unique extension the files have
            bool succes = true;
            if (extensions == null)
                return false;
            foreach (string s in extensions)
            {
                try
                {
                    Directory.CreateDirectory(newFilePath + "\\" + s);
                }
                catch (Exception)
                {
                    succes = false;
                }
            }
            return succes;
        }
    }


    class file
    {// container for needed fileinformation 
        public string Name = "";
        public string Path = "";
        public string Extension = "";
        public string OldName = "";
        public file(string f)
        {
            OldName = f;
            List<string> temp = new List<string>(f.Split('.'));
            Extension = '.' + temp[temp.Count() - 1];
            temp = new List<string>(f.Split('\\'));
            for (int i = 0; i < temp.Count() - 1; i++)
                Path += temp[i] + "\\";
        }
    }
}

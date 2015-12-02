using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FileRenamer2
{
    class BLTreeView
    {

        public List<TreeViewItem> loadDrives()
        {//finds all logical drives on the computer
            List<TreeViewItem> temp = new List<TreeViewItem>();
            try
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                foreach (DriveInfo d in allDrives)
                {
                    TreeViewItem node = new TreeViewItem();
                    node.Header = d.Name;
                    node.Tag = d.Name;
                    node.ItemsSource = getTreeChildren(node);
                    temp.Add(node);
                }
                return temp;
            }
            catch (Exception) { }
            return null;
        }
        public List<TreeViewItem> getTreeChildren(TreeViewItem node)
        {
            //palauttaa kaikki treeview itemin lapsi itemit
            List<TreeViewItem> temp = new List<TreeViewItem>();
            try
            {
                node.ItemsSource = null;
                List<string> dirs = new List<string>(Directory.EnumerateDirectories(node.Tag.ToString()));
                foreach (var dir in dirs)
                {
                    TreeViewItem childItem = new TreeViewItem();
                    childItem.Header = dir.Substring(dir.LastIndexOf("\\"));
                    childItem.Tag = dir;
                    temp.Add(childItem);
                }
                return temp;
            }
            catch (Exception)
            {
            }
            return null;
        }
        public void getAllChildren(TreeViewItem item)
        {//Listaa TreeViewItemiin kaikki sen alihakemistot
            item.ItemsSource = null;
            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
            stack.Push(item);
            while (stack.Count > 0)
            {
                TreeViewItem currentItem = stack.Pop();
                try
                {
                    currentItem.ItemsSource = getTreeChildren(currentItem);
                    if (currentItem.Items != null)
                    {
                        foreach (TreeViewItem childItem in currentItem.Items)
                        {
                            stack.Push(childItem);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        public TreeViewItem treeViewNavigator(TreeViewItem item, string name)
        {
            //palauttaa item lapsen jonka header on name
            if (item != null)
            {
                foreach (TreeViewItem i in item.Items)
                {
                    if (i.Header.ToString().Equals(name))
                    {
                        i.IsSelected = true;
                        return i;
                    }
                }
            }
            return null;
        }
    }
}

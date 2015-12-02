using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileRenamer2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool treeIsPopulated = false;
        bool textChangeEvent = true;
        bool gotFileExtensions = false;
        TreeViewItem temp = null;
        BLTreeView structure = new BLTreeView();
        BLFileReader2 fileReader = new BLFileReader2();
        BLFileMover fileMover = new BLFileMover();
        public MainWindow()
        {
            InitializeComponent();
            tvDir.ItemsSource = structure.loadDrives();
            treeIsPopulated = tvDir.ItemsSource != null;
            cbCopyOrMove.Items.Add("Rename");
            cbCopyOrMove.Items.Add("Copy");
            cbCopyOrMove.Items.Add("Move");
            cbCopyOrMove.SelectedIndex = 0;
        }

        private void tvDir_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            //selectionchange tapahtumankäsittelijä treeview komponenttiin
            temp = (TreeViewItem)tvDir.SelectedItem;
            if (!temp.Header.ToString().Contains(':'))
            {
                //hakee valitun noden koko kannan
                structure.getAllChildren(temp);

                //textChangeEvent bool kontrolloi txtDir textboxin onChance eventin toiminnan suoritusta
                //pitää aina muistaa laittaa takaisin päälle aina kun disabloi tai tapahtuu kauheita
                textChangeEvent = false;
                txtDir.Text = temp.Tag.ToString();
                textChangeEvent = true;

                SelectionChange(temp);

            }
        }
        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {//tämä ja xml pätkä <TreeView TreeViewItem.Expanded="TreeViewItem_Expanded"
            //antaa käsitellä itemexpansiota
            TreeViewItem tvi = e.OriginalSource as TreeViewItem;
            if (tvi != null)
            {
                foreach (TreeViewItem item in tvi.Items)
                {
                    item.ItemsSource = null;
                    item.ItemsSource = structure.getTreeChildren(item);
                    //getTreeChildren(item);
                }
            }
        }
        private void upDateDataGrid()
        {
            dgFiles.DataContext = null;
            dgFiles.DataContext = fileReader.getFileInfoAsDataTable(getComboBoxFilters(), txtNameFilter.Text, cboxExtendedInfo.IsChecked.Value);
        }
        private void fillcbExtensionFilter()
        {
            //täyttää cbExtensionFilterin tiedostopäätteillä
            cbExtensionFilter.Items.Clear();
            cbExtensionFilter.Items.Add("Valitse tiedostopaatteet");
            List<String> list = fileReader.getFileExtensions();
            foreach (string s in list)
            {
                CheckBox b = new CheckBox();
                b.Content = s;
                cbExtensionFilter.Items.Add(b);
            }
            cbExtensionFilter.SelectedIndex = 0;
            gotFileExtensions = true;
        }
        private void cbSelected(object sender, RoutedEventArgs e)
        {
            if (gotFileExtensions)
            {
                //metodi cbExtensionFilterin valinnan naamiointiin
                if (cbExtensionFilter.SelectedItem != null && cbExtensionFilter.SelectedItem.GetType() == typeof(CheckBox))
                {
                    CheckBox c = (CheckBox)cbExtensionFilter.SelectedItem;
                    //pitää tarkistaa että checkboxilla on arvo ja sitten yryittää muuttaa sitä
                    if (c.IsChecked.HasValue)
                    {
                        c.IsChecked = !c.IsChecked.Value;
                    }

                    updatecbExtensionFilter();
                }
                cbExtensionFilter.SelectedIndex = 0;
            }
        }
        private void cbExtensionFilter_DropDownClosed(object sender, EventArgs e)
        {//tapahtuman käsittelija dropdownclosed eventillle
            if (gotFileExtensions)
            {
                updatecbExtensionFilter();
                cbExtensionFilter.SelectedIndex = 0;
            }
        }
        private void updatecbExtensionFilter()
        {
            String s = "";
            bool all = false;
            foreach (var item in cbExtensionFilter.Items)
            {
                if (item.GetType() == typeof(CheckBox))
                {
                    CheckBox ittem = (CheckBox)item;
                    if (ittem.IsChecked.HasValue && ittem.IsChecked.Value)
                    {

                        s += ittem.Content + " ";
                        if (s.Equals(".* "))
                        {
                            all = true;
                        }
                    }
                }
            }
            if (all || s.Equals(""))
                s = ".*";
            cbExtensionFilter.Items[0] = s;
            cbExtensionFilter.SelectedIndex = 0;
            //tulosten filtteröinti
            upDateDataGrid();
        }
        private List<string> getComboBoxFilters()
        {
            //hakkaa filter stringin palasiksi 
            if (cbExtensionFilter.SelectedItem.ToString().Equals("Valitse tiedostopaatteet"))
                return new List<string>(new String[] { "*.*" });
            return new List<string>(cbExtensionFilter.SelectedItem.ToString().Split(' '));
        }
        private void txtDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            //bool textChangeEvent:llä kontrolloidaan sitä toimiiko event kun puuta navigoidaan
            if (treeIsPopulated && textChangeEvent)
            {
                string tempString = txtDir.Text;
                List<string> stringList = new List<string>(tempString.Split('\\'));
                for (int i = 0; i < stringList.Count(); i++)
                {
                    if (i == 0)
                        stringList[i] += "\\";
                    else
                        stringList[i] = "\\" + stringList[i];

                }

                TreeViewItem node = null;
                foreach (TreeViewItem item in tvDir.Items)
                {
                    if (item.Header.ToString().Equals(stringList[0]))
                    {
                        node = item;
                    }
                }
                if (node != null)
                {
                    for (int i = 1; i < stringList.Count(); i++)
                    {
                        TreeViewItem tempItem = structure.treeViewNavigator(node, stringList[i]);
                        if (tempItem != null)
                            node = tempItem;
                    }
                }
            }
        }
        private void txtNameFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            upDateDataGrid();
        }
        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            if (temp != null)
            {
                List<string> fileList = new List<string>();
                foreach (DataRowView v in dgFiles.Items)
                {
                    fileList.Add(v[1].ToString());
                }
                fileMover.renameOrMoveFiles(fileList
                    , cbCopyOrMove.SelectedItem.ToString()
                    , txtCopyDir.Text
                    , temp.Tag.ToString()
                    , cboxRetainStructure.IsChecked.Value
                    , cboxSeparateFolders.IsChecked.Value
                    , cboxDiffNames.IsChecked.Value
                    , txtNewName.Text);
                //update datagrid and iteminformation to show the user
                if (temp != null)
                {
                    SelectionChange(temp);
                    upDateDataGrid();
                }
            }
        }
        private void cboxSeparateFolders_Checked(object sender, RoutedEventArgs e)
        {
            cboxRetainStructure.IsChecked = false;
            cboxDiffNames.IsChecked = true;
        }
        private void cboxRetainStructure_Checked(object sender, RoutedEventArgs e)
        {
            cboxSeparateFolders.IsChecked = false;
        }
        private void cboxExtendedInfo_Checked(object sender, RoutedEventArgs e)
        {
            upDateDataGrid();
        }
        private void cboxRecursive_Checked(object sender, RoutedEventArgs e)
        {
            if (temp != null)
            {
                if (!temp.Header.ToString().Contains(':'))
                {
                    SelectionChange(temp);
                }
            }
        }
        private void cboxRecursive_Unchecked(object sender, RoutedEventArgs e)
        {
            if (temp != null)
            {
                if (!temp.Header.ToString().Contains(':'))
                {
                    SelectionChange(temp);
                }
            }
        }
        private void SelectionChange(TreeViewItem temp)
        {
            //ensin päivitetään fileinfo
            //boolean merkitsee haetaanko tietoa rekursiivisesti vai ei
            fileReader.updateFileInfo(temp, cboxRecursive.IsChecked.Value);
            //sitten päivitetään filtter informaatio
            fillcbExtensionFilter();
            //sitten päivitetään datagrid
            upDateDataGrid();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace FileHistoryFixer
{
    public partial class frmMain : Form
    {
        private FolderBrowserDialog _dialog;
        private DirectoryModel _root;

        public frmMain()
        {
            InitializeComponent();

            _dialog = new FolderBrowserDialog();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            var result = _dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                btnScan.Enabled = false;
                btnFix.Enabled = false;

                _root = TraverseDirectory(_dialog.SelectedPath);

                UpdateTree(_root, treeView.Nodes);

                btnScan.Enabled = true;
                btnFix.Enabled = true;
            }
        }

        private void btnFix_Click(object sender, EventArgs e)
        {
            if (_root != null)
            {
                var result = MessageBox.Show("This will delete all duplicate files and rename the latest versions. Ensure you back up all files first. Continue?", "Idiot Check", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    btnScan.Enabled = false;
                    btnFix.Enabled = false;

                    Fix(_root);

                    btnScan.Enabled = true;

                    treeView.Nodes.Clear();
                    MessageBox.Show("Done, check your files before deleting the backup.", "Complete", MessageBoxButtons.OK);                    
                }
            }
        }

        private DirectoryModel TraverseDirectory(string path)
        {
            var root = new DirectoryModel { 
                Path = path
            };
            
            foreach (var directory in Directory.GetDirectories(path))
            {
                try
                {
                    root.Directories.Add(TraverseDirectory(directory));
                }
                catch (UnauthorizedAccessException)
                { 
                    //do nothing.
                }
            }

            foreach (var file in Directory.GetFiles(path))
            {
                var extension = Path.GetExtension(file);
                var fileName = Path.GetFileNameWithoutExtension(file);

                if (fileName.EndsWith(" UTC)"))
                {
                    var version = fileName.Substring(fileName.Length - 25, 25);
                    var original = fileName.Substring(0, fileName.Length - 25);

                    var existing = root.Files.FirstOrDefault(f => f.OriginalName == original && f.Extension == extension);

                    if (existing == null)
                    {
                        existing = new FileModel
                        {
                            Extension = extension,
                            OriginalName = original
                        };

                        root.Files.Add(existing);
                    }

                    existing.Versions.Add(
                        new VersionModel(version
                        )
                    );
                }                
            }

            return root;
        }

        private void UpdateTree(DirectoryModel root, TreeNodeCollection nodes) {

            foreach (var directory in root.Directories)
            {
                var treeNode = new TreeNode(directory.Path);

                nodes.Add(treeNode);

                UpdateTree(directory, treeNode.Nodes);
            }

            foreach (var file in root.Files)
            {
                var treeNode = new TreeNode(file.OriginalName + file.Extension);

                treeNode.Nodes.AddRange(file.Versions.Select(v => new TreeNode(v.DateTime.ToString())).ToArray());

                nodes.Add(treeNode);
            }
        }

        private void Fix(DirectoryModel root)
        {
            foreach (var directory in root.Directories)
            {
                Fix(directory);
            }

            foreach (var file in root.Files)
            {
                var versions = file.Versions.OrderByDescending(v => v.DateTime).ToArray();

                var retain = versions[0];

                for (var i = 1; i < versions.Count(); i++)
                {
                    //Murder additional copies.
                    File.Delete(root.Path + Path.DirectorySeparatorChar + file.OriginalName + versions[i].Text + file.Extension);
                }

                //Rename the one we're keeping
                File.Move(root.Path + Path.DirectorySeparatorChar + file.OriginalName + retain.Text + file.Extension, root.Path + Path.DirectorySeparatorChar + file.OriginalName.TrimEnd(' ') + file.Extension);
            }
        }
    }
}

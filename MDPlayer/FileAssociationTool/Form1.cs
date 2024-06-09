using Microsoft.Win32;
using System.Windows.Forms;

namespace FileAssociationTool
{
    public partial class FileAssociationTool : Form
    {
        public FileAssociationTool()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string extss = txtExt.Text;
                string newPrefix = txtNewPrefix.Text;
                string iconPath = txtIconPath.Text + " , -0";
                string execPath = txtExecPath.Text;
                string subkey;
                RegistryKey key;

                string[] exts = extss.Split(";", StringSplitOptions.RemoveEmptyEntries);

                foreach (string ext in exts)
                {
                    iconPath = txtIconPath.Text;
                    if (iconPath.IndexOf("???") >= 0)
                    {
                        iconPath = iconPath.Replace("???", ext.Replace(".", ""));
                    }

                    if (!File.Exists(iconPath))
                    {
                        MessageBox.Show(string.Format(".ico�t�@�C��({0})��������Ȃ������̂ŏ����𒆒f���܂�", iconPath));
                        return;
                    }

                    iconPath += " , -0";
                    //explorer��UserChoice������
                    subkey = string.Format("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\{0}", ext);
                    deleteKey(subkey);

                    //�K��̒l��V�����T�u�L�[�ɕύX
                    subkey = string.Format("{0}", ext);
                    key = Registry.ClassesRoot.CreateSubKey(subkey);
                    subkey = string.Format("{0}{1}", newPrefix, ext);
                    key.SetValue(null, subkey);//���
                    Registry.ClassesRoot.Close();

                    //�V�����T�u�L�[���쐬
                    key = Registry.ClassesRoot.CreateSubKey(subkey);
                    //defaultIcon�̐ݒ�
                    key = Registry.ClassesRoot.CreateSubKey(subkey + "\\DefaultIcon");
                    key.SetValue(null, iconPath);//���

                    key = Registry.ClassesRoot.CreateSubKey(subkey + "\\shell\\open\\command");
                    key.SetValue(null, execPath);//���

                    //�K��̒l��V�����T�u�L�[�ɕύX
                    subkey = string.Format("Software\\Classes\\{0}", ext);
                    key = Registry.CurrentUser.CreateSubKey(subkey);
                    subkey = string.Format("{0}{1}", newPrefix, ext);
                    key.SetValue(null, subkey);//���
                    Registry.CurrentUser.Close();

                    //�V�����T�u�L�[���쐬
                    key = Registry.CurrentUser.CreateSubKey("Software\\Classes\\" + subkey);
                    //defaultIcon�̐ݒ�
                    key = Registry.CurrentUser.CreateSubKey("Software\\Classes\\" + subkey + "\\DefaultIcon");
                    key.SetValue(null, iconPath);//���
                    key = Registry.CurrentUser.CreateSubKey("Software\\Classes\\" + subkey + "\\shell\\open\\command");
                    key.SetValue(null, execPath);//���

                    //�K��̒l��V�����T�u�L�[�ɕύX
                    subkey = string.Format("Software\\Classes\\{0}", ext);
                    key = Registry.LocalMachine.CreateSubKey(subkey);
                    subkey = string.Format("{0}{1}", newPrefix, ext);
                    key.SetValue(null, subkey);//���
                    Registry.LocalMachine.Close();

                    //�V�����T�u�L�[���쐬
                    key = Registry.LocalMachine.CreateSubKey("Software\\Classes\\" + subkey);
                    //defaultIcon�̐ݒ�
                    key = Registry.LocalMachine.CreateSubKey("Software\\Classes\\" + subkey + "\\DefaultIcon");
                    key.SetValue(null, iconPath);//���
                    key = Registry.LocalMachine.CreateSubKey("Software\\Classes\\" + subkey + "\\shell\\open\\command");
                    key.SetValue(null, execPath);//���
                }

                MessageBox.Show("Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Message: {0} StackTrace:{1}", ex.Message, ex.StackTrace));
            }
        }

        private void deleteKey(string subkey)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(subkey);
            if (key != null)
            {
                while (key.SubKeyCount > 0)
                {
                    deleteKey(subkey + "\\" + key.GetSubKeyNames()[0]);
                }
                Registry.CurrentUser.DeleteSubKey(subkey);
                Console.WriteLine($"{subkey} ���폜����܂����B");
            }
        }

        private void FileAssociationTools_Load(object sender, EventArgs e)
        {
            string path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "ico");
            txtExt.Text = ".vgm";


            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                FileInfo[] fiAlls = di.GetFiles("*.ico");
                string pp = "";
                foreach (FileInfo f in fiAlls)
                {
                    string ext = Path.GetFileNameWithoutExtension(f.FullName);
                    if (ext.IndexOf("_") >= 0)
                    {
                        ext = ext.Substring(ext.IndexOf("_") + 1);
                    }
                    pp += string.Format(".{0};", ext);
                }
                if (!string.IsNullOrEmpty(pp))
                {
                    txtExt.Text = pp;
                    txtIconPath.Text = Path.Combine(path, "mdp_???.ico");
                }
            }
        }

        private void FileAssociationTools_Shown(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show(
@"���̃c�[�����g�p�����Explorer�Ȃǂ̊֘A���郌�W�X�g�����폜�A�X�V���āA�t�@�C���̃A�C�R���ƋN���A�v���̎w����s���܂��B
�z��O�̓��쌋�ʂɂȂ邱�Ƃ��\�z����܂��̂Ŏ��s�O�Ƀo�b�N�A�b�v�Ȃǂ̑΍���s���Ă��������B
���s�́u���ȐӔC�v�ɂȂ�A����ɂ���ĉ����N�����Ă������ł͈�ؐӔC�������܂���B
This tool will delete and update the relevant registry of Explorer and other applications, and specify file icons and startup applications.
Please back up your computer before executing this tool, as it may cause unexpected results.
Please note that you do so at your own risk, and we will not be held responsible for any problems that may occur.",
                "�x��",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (res == DialogResult.Cancel) this.Close();
        }
    }
}

using MDPlayer.form;
using System.IO;

namespace MDPlayerx64
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string fn = checkFiles();
            if (fn != null)
            {
                MessageBox.Show(string.Format("����ɕK�v�ȃt�@�C��({0})���݂���܂���B", fn), "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            frmMain frm = null;
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                frm = new frmMain();
                Application.Run(frm);
            }
            catch (InvalidOperationException)
            {
                ;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("�s���ȃG���[���������܂����B\nException Message:\n{0}", e.Message), "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static string checkFiles()
        {
            List<string> chkFn = new List<string>()
            {
                "MDSound.dll"
                , "NAudio.dll"
                , "RealChipCtlWrap64.dll"
                , "lib\\scci.dll"
                , "lib\\c86ctl.dll"
            };
            chkFn.AddRange(MDPlayer.vstMng.chkFn);

            foreach (string fn in chkFn)
            {
                if (!File.Exists(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), fn))) return fn;
            }

            return null;
        }
    }
}
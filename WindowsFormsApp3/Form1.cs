using IniFiles;
using InputBoxdlg;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {

        const int n = 12;
        const int t = 1;
        int x = 1;
        int y = 1;
        int a;
        string t1 = " ";
        DateTime DTN;
        //const string crypto = "YES";
        bool mainhide;
        readonly List<TabPage> pages = new List<TabPage>();
        readonly List<Label> names = new List<Label>();
        readonly List<TextBox> logins = new List<TextBox>();
        readonly List<TextBox> passwords = new List<TextBox>();
        readonly Data list = new Data();
        readonly Timer timer;
        Timer autosave;
        TabControl table;
        StatusStrip status;
        ToolStripStatusLabel time;
        ToolStripStatusLabel data;
        ToolStripStatusLabel enc;
        ToolStripStatusLabel auto;
        ToolStripMenuItem autorun;
        ToolStripMenuItem encrypt;
        ToolStripMenuItem backup;
        ToolStripMenuItem decrypt;
        NotifyIcon tray;
        ContextMenuStrip tray_menu;
        ToolStripMenuItem hide;
        ToolStripMenuItem tray_exit;
        ToolStripMenuItem tray_about;

        public Form1()
        {
            var settings = new IniFile(@"settings.ini");
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem addpage = new ToolStripMenuItem();
            ToolStripMenuItem exit = new ToolStripMenuItem();
            ToolStripMenuItem prop = new ToolStripMenuItem();
            ToolStripMenuItem save = new ToolStripMenuItem();
            ToolStripMenuItem delete = new ToolStripMenuItem();

            tray_menu = new ContextMenuStrip();
            hide = new ToolStripMenuItem();
            tray_exit = new ToolStripMenuItem();
            tray_about = new ToolStripMenuItem();

            autorun = new ToolStripMenuItem();
            backup = new ToolStripMenuItem();
            encrypt = new ToolStripMenuItem();
            decrypt = new ToolStripMenuItem();
            status = new StatusStrip();
            time = new ToolStripStatusLabel();
            data = new ToolStripStatusLabel();
            enc = new ToolStripStatusLabel();
            auto = new ToolStripStatusLabel();
            tray = new NotifyIcon();

            InitializeComponent();
            Width = 280;
            Height = 580;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            TopMost = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            ContextMenuStrip = menu;
            FormClosing += Form1_FormClosing;
            this.Text = "Password Holder";
            Icon = Properties.Resources.book_bookmark;

            timer = new Timer();
            timer.Enabled = true;
            timer.Interval = 1000;
            timer.Start();
            timer.Tick += Timer_Tick;

            autosave = new Timer();
            autosave.Interval = 5000;
            autosave.Enabled = true;
            autosave.Start();
            autosave.Tick += Savedata;

            table = new TabControl();
            table.Name = "table";
            Controls.Add(table);
            table.Dock = DockStyle.Fill;

            menu.Name = "menu";
            menu.Items.AddRange(new[] { addpage, delete, save, prop, exit });
            table.ContextMenuStrip = menu;

            addpage.Text = "Добавить вкладку";
            addpage.Name = "addpage";
            addpage.Image = Properties.Resources.plus;
            addpage.Click += Addpage_Click;

            delete.Name = "delete";
            delete.Text = "Удалить вкладку";
            delete.Image = Properties.Resources.delete_1;
            delete.Click += Delete_Click1;

            exit.Text = "Выход";
            exit.Name = "exit";
            exit.Image = Properties.Resources.exit;
            exit.Click += Exit_Click;

            prop.Text = "Настройки";
            prop.Name = "prop";
            prop.Image = Properties.Resources.options;
            prop.DropDownItems.AddRange(new[] { autorun, encrypt, decrypt, backup });

            autorun.Name = "autorun";
            autorun.Text = "Автозагрузка";
            autorun.Image = Properties.Resources.balloon;
            autorun.CheckOnClick = true;
            autorun.Click += Autorun_Click;

            encrypt.Name = "crypt";
            encrypt.Text = "Шифровать";
            encrypt.Image = Properties.Resources.import;
            encrypt.Click += Crypt_Click;
            encrypt.CheckOnClick = true;

            decrypt.Name = "decrypt";
            decrypt.Text = "Расшифровать";
            decrypt.Image = Properties.Resources.еxport;
            decrypt.Click += Decrypt_Click;
            decrypt.Visible = false;

            backup.Name = "backup";
            backup.Text = "Резервная копия";
            backup.Image = Properties.Resources.cd_disk;
            backup.Click += Backup_Click;

            save.Text = "Сохранить";
            save.Name = "save";
            save.Image = Properties.Resources.save;
            save.Click += Savedata;

            // status = new StatusBar();
            status.Name = "statusbar";
            status.Width = Width;
            status.ShowItemToolTips = true;
            status.Items.AddRange(new[] { time, data, enc, auto });
            Controls.Add(status);
            time.Name = "time";
            data.Name = "data";
            enc.Name = "Encoded";
            auto.Name = "Autoload";

            tray.Visible = true;
            tray.Icon = Properties.Resources.book_bookmark;
            tray.Text = "Password Holder";

            tray_menu.Name = "traymenu";
            tray_menu.Items.AddRange(new[] { hide, tray_about, tray_exit });
            tray.ContextMenuStrip = tray_menu;
            tray_menu.Opened += Tray_menu_Opened;

            hide.Name = "hide";
            hide.Text = "Hide";
            hide.Image = Properties.Resources.down;
            hide.Click += Hide_Click;

            tray_about.Name = "trayabout";
            tray_about.Text = "О программе";
            tray_about.Image = Properties.Resources.balloon;
            tray_about.Click += Tray_about_Click;

            tray_exit.Name = "trayexit";
            tray_exit.Text = "Выход";
            tray_exit.Image = Properties.Resources.exit;
            tray_exit.Click += Tray_exit_Click;
        }

        private void Tray_menu_Opened(object sender, EventArgs e)
        {
            if (this.Visible == true)
            {
                hide.Text = "Свернуть";
                mainhide = false;
                hide.Image = Properties.Resources.down;
            }
            if (this.Visible == false)
            {
                hide.Text = "Развернуть";
                mainhide = true;
                hide.Image = Properties.Resources.up;
            }
        }

        private void Tray_exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Tray_about_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Программа для хранения аккаунтов (логин, пароль)" + "\n" + "Версия 3 (build " + Application.ProductVersion.ToString() + ")", "Password Holder");
        }

        private void Hide_Click(object sender, EventArgs e)
        {
            if (mainhide == true)
            {
                this.Show();
            }
            if (mainhide == false)
            {
                this.Hide();
            }

        }

        private void Decrypt_Click(object sender, EventArgs e)
        {
            var settings = new IniFile(@"settings.ini");
            string @CRYPT_KEY = settings.Read("cryptkey", "Settings");

            Decrypt("data.dat", @CRYPT_KEY);
            settings.Write("Crypto", "NO", "Settings");
            MessageBox.Show("Данные открыты", "Шифрование");
        }

        private void Backup_Click(object sender, EventArgs e)
        {
            DialogResult result;
            try
            {
                //Создание резервной копии данных
                string backup = "archive.zip";

                //Архивация файла данных
                if (File.Exists("archive.zip"))
                {
                    System.IO.File.Delete(@"archive.zip");
                    string archivePath = backup;
                    using (ZipArchive zipArchive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
                    {
                        const string pathFileToAdd = @"data.dat";
                        const string nameFileToAdd = "data.dat";
                        zipArchive.CreateEntryFromFile(pathFileToAdd, nameFileToAdd);

                    }
                }
                else
                {
                    const string archivePath = @"archive.zip";
                    using (ZipArchive zipArchive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
                    {
                        const string pathFileToAdd = @"data.dat";
                        const string nameFileToAdd = "data.dat";
                        zipArchive.CreateEntryFromFile(pathFileToAdd, nameFileToAdd);

                    }
                }
                MessageBox.Show("Резервная копия создана!", "Password Holder");
               result = MessageBox.Show(this, "Открыть папку с файлом?", "Password Holder", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (result == DialogResult.OK)
                {
                    Process PrFolder = new Process();
                    ProcessStartInfo psi = new ProcessStartInfo();
                    string file = @"archive.zip";
                    psi.CreateNoWindow = true;
                    psi.WindowStyle = ProcessWindowStyle.Normal;
                    psi.FileName = "explorer";
                    psi.Arguments = @"/n, /select, " + file;
                    PrFolder.StartInfo = psi;
                    PrFolder.Start();
                }
            }
            catch (Exception e3)
            {
                DTN = DateTime.Now;
                string date = DTN.Year.ToString() + "-" + DTN.Month.ToString() + "-" + DTN.Day.ToString() + " - " + DTN.Hour.ToString() + ":" + DTN.Minute.ToString() + ":" + DTN.Second.ToString() + " - ";
                StreamWriter str = new StreamWriter("log.txt", true);
                MessageBox.Show(e3.Message + "\n" + "\n" + "Создана записть в лог-файле.", "Password Holder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                str.WriteLine(date + e3.Message);
                str.Close();
                File.WriteAllText("data.dat", "");
            }
        }

        public void Encrypt(string inputFile, string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    rng.GetBytes(salt);
                }
            }

            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;
                AES.Padding = PaddingMode.PKCS7;
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(passwordBytes, salt, 2048);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);
                AES.Mode = CipherMode.CFB;

                using (FileStream inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.ReadWrite))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(inputStream, AES.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            try
                            {
                                inputStream.CopyTo(memoryStream);

                                inputStream.SetLength(0);

                                inputStream.Position = 0;

                                inputStream.Write(salt, 0, salt.Length);

                                memoryStream.WriteTo(cryptoStream);
                            }
                            catch (OutOfMemoryException oome)
                            {
                                Debug.WriteLine("Error: " + oome.Message);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Error: " + ex.Message);
                            }
                            finally
                            {
                                cryptoStream.Close();
                                cryptoStream.Dispose();

                                inputStream.Close();
                                inputStream.Dispose();

                                memoryStream.Close();
                                memoryStream.Dispose();

                                key.Dispose();
                                AES.Dispose();
                            }
                        }
                    }
                }
            }
        }

        public void Decrypt(string inputFile, string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[32];

            using (FileStream inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.ReadWrite))
            {
                inputStream.Read(salt, 0, salt.Length);
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(passwordBytes, salt, 2048);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Padding = PaddingMode.PKCS7;
                    AES.Mode = CipherMode.CFB;

                    using (CryptoStream cryptoStream = new CryptoStream(inputStream, AES.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            try
                            {
                                cryptoStream.CopyTo(memoryStream);

                                inputStream.SetLength(0);

                                memoryStream.Position = 0;

                                memoryStream.CopyTo(inputStream);
                            }
                            catch (CryptographicException ex_CryptographicException)
                            {
                                Debug.WriteLine("CryptographicException error: " + ex_CryptographicException.Message);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Error: " + ex.Message);
                            }

                            try
                            {
                                cryptoStream.Close();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Error by closing CryptoStream: " + ex.Message);
                            }
                            finally
                            {
                                memoryStream.Close();
                                inputStream.Close();
                                key.Dispose();
                                AES.Dispose();
                            }
                        }
                    }
                }
            }
        }

        public void Crypt_Click(object sender, EventArgs e)
        {
            var settings = new IniFile(@"settings.ini");
            string @CRYPT_KEY = settings.Read("cryptkey", "Settings");
            string crypto = settings.Read("Crypto", "Settings");
            if (encrypt.Checked == true)
            {
                if (@CRYPT_KEY != null)
                {
                    InputBoxResult crypt = InputBox.Show("Ключ шифрования", "Ключ шифрования", "", 100, 0);
                    if (crypt.ReturnCode == DialogResult.OK)
                        @CRYPT_KEY = crypt.Text;
                    settings.Write("cryptkey", @CRYPT_KEY, "Settings");

                    Encrypt("data.dat", @CRYPT_KEY);
                }
                settings.Write("Crypto", "YES", "Settings");
                MessageBox.Show("Данные зашифрованы", "Шифрование");
            }
            else
            {
                Decrypt("data.dat", @CRYPT_KEY);
                settings.Write("Crypto", "NO", "Settings");
                MessageBox.Show("Данные открыты", "Шифрование");
            }


        }

        public bool SetAutorunValue(bool autorun)
        {
            string ExePath = System.Windows.Forms.Application.ExecutablePath;
            RegistryKey reg;
            //MessageBox.Show(ExePath);
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            try
            {
                if (autorun)
                    reg.SetValue("Password Holder", ExePath);
                else
                    reg.DeleteValue("Password Holder");

                reg.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void Autorun_Click(object sender, EventArgs e)
        {
            var settings = new IniFile(@"settings.ini");
            if (autorun.Checked == true)
            {
                SetAutorunValue(true);
                settings.Write("Autorun", "true", "Settings");
            }
            else
            {
                SetAutorunValue(false);
                settings.Write("Autorun", "false", "Settings");
            }
        }

        private void Delete_Click1(object sender, EventArgs e)
        {
            int index = table.SelectedIndex;
            var st = table.SelectedTab;
            // table.TabPages.RemoveAt(index);
            var allnames = names.ToArray();
            if (table.Controls.Contains(st))
            {
                pages.Remove(st);
                st.Dispose();
            }
            if (table.TabCount == 0)
            {
                File.Delete(@"data.dat");
            }
        }

        private void SelectedTab_Click(object sender, EventArgs e)
        {
            TabPage tp = sender as TabPage;
            if (tp != null)
            {
                InputBoxResult name = InputBox.Show("Введите имя", "Введите имя", "", 100, 0);
                if (name.ReturnCode == DialogResult.OK)
                    tp.Text = name.Text;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            var settings = new IniFile(@"settings.ini");
            settings.Write("Top", Top.ToString(), "Window");
            settings.Write("Left", Left.ToString(), "Window");
            settings.Write("Pages", table.TabPages.Count.ToString(), "Settings");
            // Savedata(null, null);
        }

        private void Prop_Click(object sender, EventArgs e)
        {
            Form pform = new Form();
            pform.Width = 500;
            pform.Height = 300;
            pform.Text = "Properties";
            pform.MaximizeBox = false;
            pform.MinimizeBox = false;
            pform.FormBorderStyle = FormBorderStyle.FixedDialog;
            pform.StartPosition = FormStartPosition.CenterScreen;
            pform.TopMost = true;
            pform.ShowDialog();


            var btok = new Button();
            btok.Name = "btnok";
            btok.Text = "OK";
            btok.Size = new Size(30, 20);
            btok.Location = new Point(30, 40);
            pform.Controls.Add(btok);
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Addpage_Click(object sender, EventArgs e)
        {
            Label[] nam = new Label[n];
            TextBox[] log = new TextBox[n];
            TextBox[] pass = new TextBox[n];
            TabPage[] page = new TabPage[t];
            for (int j = 0; j < t; j++)
            {
                page[j] = new TabPage();
                page[j].Name = String.Format("page{0}", y);
                InputBoxResult name = InputBox.Show("Введите имя", "Введите имя", "", 100, 0);
                if (name.ReturnCode == DialogResult.OK)
                    page[j].Text = name.Text;
                //page[j].Text = string.Format("Page {0}", y);
                table.Controls.Add(page[j]);
                pages.Add(page[j]);
                table.SelectedTab = page[j];

                for (int i = 0; i < n; i++)
                {
                    nam[i] = new Label();
                    nam[i].Name = "nam" + x.ToString();
                    nam[i].Text = "unnamed";
                    nam[i].Height = 20;
                    nam[i].Location = new Point(5, 5 + i * 40);
                    nam[i].DoubleClick += NameAdd;
                    page[j].Controls.Add(nam[i]);
                    names.Add(nam[i]);

                    log[i] = new TextBox();
                    log[i].Name = "log" + x.ToString();
                    log[i].Text = "";
                    log[i].Size = new Size(120, 20);
                    log[i].Location = new Point(5, 25 + i * 40);
                    log[i].ReadOnly = true;
                    log[i].ContextMenuStrip = new ContextMenuStrip();
                    log[i].Click += CopyClick;
                    log[i].DoubleClick += LoginAdd;
                    page[j].Controls.Add(log[i]);
                    logins.Add(log[i]);

                    pass[i] = new TextBox();
                    pass[i].Name = "pass" + x.ToString();
                    pass[i].Text = "";
                    pass[i].Size = new Size(120, 20);
                    pass[i].Location = new Point(130, 25 + i * 40);
                    pass[i].ReadOnly = true;
                    pass[i].ContextMenuStrip = new ContextMenuStrip();
                    pass[i].Click += CopyClick;
                    pass[i].DoubleClick += PasswordAdd;
                    page[j].Controls.Add(pass[i]);
                    passwords.Add(pass[i]);
                    x++;
                }
                y++;
            }
            var settings = new IniFile(@"settings.ini");
            settings.Write("Pages", table.TabPages.Count.ToString(), "Settings");
        }

        private void Timer_Tick(object sender, System.EventArgs e)
        {
            var settings = new IniFile(@"settings.ini");
            string crypto = settings.Read("Crypto", "Settings");
            string autoran = settings.Read("Autorun", "Settings");

            // string alldata = string.Format("T: {0} ", pages.Count.ToString()) + string.Format("N: {0} ", names.Count.ToString()) + string.Format("L: {0} ", logins.Count.ToString()) + string.Format("P: {0}", passwords.Count.ToString());
            string alldata = String.Format("{0}", names.Count.ToString());
            data.Text = alldata;
            data.ToolTipText = "Всего аккаунтов в базе";
            //  status.Text = alldata;
            if (t1 == " ")
                t1 = ":";
            else
                t1 = " ";
            DTN = DateTime.Now;
            time.Text = DTN.Hour.ToString().PadLeft(2, '0') + t1 +
                DTN.Minute.ToString().PadLeft(2, '0') + t1 +
                    DTN.Second.ToString().PadLeft(2, '0');
            time.ToolTipText = "Текущее время";
            enc.Text = crypto == "YES" ? "enc" : "";
            enc.ToolTipText = crypto == "YES" ? "Зашифровано" : "";
            auto.Text = autoran == "true" ? "auto" : "";
            auto.ToolTipText = autoran == "true" ? "Автозагрузка включена" : "";
        }

        public void Form1_Load(object sender, System.EventArgs e)
        {
            try
            {
                var settings = new IniFile(@"settings.ini");
                string crypto = settings.Read("Crypto", "Settings");
                string autoran = settings.Read("Autorun", "Settings");
                encrypt.Checked = crypto == "YES";
                autorun.Checked = autoran == "true";

                if (File.Exists(@"settings.ini"))
                {
                    string @CRYPT_KEY = settings.Read("cryptkey", "Settings");
                    Decrypt("data.dat", @CRYPT_KEY);

                    int cnt = Convert.ToInt32(settings.Read("Pages", "Settings"));
                    if (cnt != null)
                        LoadData(cnt);
                    Loaddata();
                    //  @CRYPT_KEY = settings.Read("cryptkey", "Settings");
                    Encrypt("data.dat", @CRYPT_KEY);
                }
                else
                {
                    int cnt = 0;
                    LoadData(cnt);
                    Loaddata();
                    settings.Write("cryptkey", "123", "Settings");
                    settings.Write("Pages", "0", "Settings");
                    settings.Write("Crypto", "NO", "Settings");
                    //  File.WriteAllText("data.dat", "");
                }
            }
            catch (Exception e1)
            {
                DTN = DateTime.Now;
                string date = DTN.Year.ToString() + "-" + DTN.Month.ToString() + "-" + DTN.Day.ToString() + " - " + DTN.Hour.ToString() + ":" + DTN.Minute.ToString() + ":" + DTN.Second.ToString() + " - ";
                StreamWriter str = new StreamWriter("log.txt", true);
                MessageBox.Show(e1.Message + "\n" + "\n" + "Создана записть в лог-файле.", "Password Holder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                str.WriteLine(date + e1.Message);
                str.Close();
                File.WriteAllText("data.dat", "");
            }
        }

        public void LoadData(int cnt)
        {
            Label[] nam = new Label[n];
            TextBox[] log = new TextBox[n];
            TextBox[] pass = new TextBox[n];
            TabPage[] page = new TabPage[cnt];
            for (int j = 0; j < cnt; j++)
            {
                page[j] = new TabPage();
                page[j].Name = String.Format("page{0}", y);
                page[j].Text = String.Format("Page {0}", y);
                table.Controls.Add(page[j]);
                pages.Add(page[j]);

                for (int i = 0; i < n; i++)
                {
                    nam[i] = new Label
                    {
                        Name = "nam" + x.ToString(),
                        Text = "unnamed",
                        Height = 20,
                        Location = new Point(5, 5 + i * 40)
                    };
                    nam[i].DoubleClick += NameAdd;
                    page[j].Controls.Add(nam[i]);
                    names.Add(nam[i]);

                    log[i] = new TextBox
                    {
                        Name = "log" + x.ToString(),
                        Text = "",
                        Size = new Size(120, 20),
                        Location = new Point(5, 25 + i * 40),
                        ReadOnly = true,
                        ContextMenuStrip = new ContextMenuStrip()
                    };
                    log[i].Click += CopyClick;
                    log[i].DoubleClick += LoginAdd;
                    page[j].Controls.Add(log[i]);
                    logins.Add(log[i]);

                    pass[i] = new TextBox
                    {
                        Name = "pass" + x.ToString(),
                        Text = "",
                        Size = new Size(120, 20),
                        Location = new Point(130, 25 + i * 40),
                        ReadOnly = true,
                        ContextMenuStrip = new ContextMenuStrip()
                    };
                    pass[i].Click += CopyClick;
                    pass[i].DoubleClick += PasswordAdd;
                    page[j].Controls.Add(pass[i]);
                    passwords.Add(pass[i]);
                    x++;
                }
                y++;
            }
        }
        public void Save()
        {
            Savedata(null, null);
        }
        public void Savedata(object sender, EventArgs e)
        {
            try
            {
                var settings = new IniFile(@"settings.ini");
                var data = new IniFile(@"data.dat");
                var allnames = names.ToArray();
                var alllogins = logins.ToArray();
                var allpasswords = passwords.ToArray();
                var allpages = pages.ToArray();
                string crypto = settings.Read("Crypto", "Settings");
                if (crypto == "NO")
                {
                    foreach (var name in allnames)
                    {
                        data.Write(name.Name, name.Text, "Data");
                    }
                    foreach (var login in alllogins)
                    {
                        data.Write(login.Name, login.Text, "Data");
                    }
                    foreach (var password in allpasswords)
                    {
                        data.Write(password.Name, password.Text, "Data");
                    }
                    foreach (var page in allpages)
                    {
                        data.Write(page.Name, page.Text, "Data");
                    }
                }
                if (crypto == "YES")
                {
                    if (File.Exists(@"data.dat"))
                    {
                        File.Delete(@"data.dat");
                        data = new IniFile(@"data.dat");
                        allnames = names.ToArray();
                        alllogins = logins.ToArray();
                        allpasswords = passwords.ToArray();
                        allpages = pages.ToArray();

                        foreach (var name in allnames)
                        {
                            data.Write(name.Name, name.Text, "Data");
                        }
                        foreach (var login in alllogins)
                        {
                            data.Write(login.Name, login.Text, "Data");
                        }
                        foreach (var password in allpasswords)
                        {
                            data.Write(password.Name, password.Text, "Data");
                        }
                        foreach (var page in allpages)
                        {
                            data.Write(page.Name, page.Text, "Data");
                        }
                        crypto = settings.Read("Crypto", "Settings");
                    }
                    string @CRYPT_KEY = settings.Read("cryptkey", "Settings");
                    Encrypt("data.dat", @CRYPT_KEY);
                }
            }
            catch (Exception e2)
            {
                DTN = DateTime.Now;
                string date =
                          DTN.Year.ToString() +
                    "-" + DTN.Month.ToString() +
                    "-" + DTN.Day.ToString() +
                    " - " + DTN.Hour.ToString() +
                    ":" + DTN.Minute.ToString() +
                    ":" + DTN.Second.ToString() +
                    " - ";
                StreamWriter str = new StreamWriter("log.txt", true);
                MessageBox.Show(e2.Message + "\n" + "\n" + "Создана записть в лог-файле.", "Password Holder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                str.WriteLine(date + e2.Message);
                str.Close();
            }
        }
        public void SaveXML()
        {
            var allnames = names.ToList(); //массив имен из коллекции Label
            var alllogins = logins.ToList(); //массив логинов из коллекции TextBox
            var allpasswords = passwords.ToList(); //массив паролей из коллекции TextBox


            for (int i = 0; i < n; i++)
            {
                var data = new Account();
                data.Name = names[i].Text;
                data.Login = logins[i].Text;
                data.Password = passwords[i].Text;
                list.Add(data);
            }
            list.SaveData(@"data.xml");

            Text = list.Count().ToString();
        }

        void Loaddata()
        {
            var data = new IniFile(@"data.dat");

            var allnames = names.ToArray();
            var alllogins = logins.ToArray();
            var allpasswords = passwords.ToArray();
            var allpages = pages.ToArray();

            foreach (Label name in allnames)
            {
                name.Text = data.Read(name.Name, "Data");
            }
            foreach (TextBox login in alllogins)
            {
                login.Text = data.Read(login.Name, "Data");
            }
            foreach (TextBox password in allpasswords)
            {
                password.Text = data.Read(password.Name, "Data");
            }
            foreach (TabPage tp in allpages)
            {
                tp.Text = data.Read(tp.Name, "Data");
            }
        }

        public void LoadXML(int cnt)
        {
            if (File.Exists(@"data.xml"))
            {
                list.LoadData(@"data.xml");
                for (int i = 0; i < cnt; i++)
                {
                    names[i].Text = list.Return(i).Name;
                    logins[i].Text = list.Return(i).Login;
                    passwords[i].Text = list.Return(i).Password;
                }
            }
        }

        public void NameAdd(object sender, EventArgs e)
        {
            Label lb = sender as Label;
            if (lb != null)
            {
                InputBoxResult name = InputBox.Show("Введите имя", "Введите имя", "", 100, 0);
                if (name.ReturnCode == DialogResult.OK)
                    lb.Text = name.Text;
            }

        }

        public void LoginAdd(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                InputBoxResult login = InputBox.Show("Введите логин", "Введите логин", "", 100, 0);
                if (login.ReturnCode == DialogResult.OK)
                    tb.Text = login.Text;
            }
        }

        public void PasswordAdd(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                InputBoxResult pass = InputBox.Show("Введите пароль", "Введите пароль", "", 100, 0);
                if (pass.ReturnCode == DialogResult.OK)
                    tb.Text = pass.Text;
            }
        }

        public void CopyClick(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                tb.SelectAll();
                tb.Copy();
            }
        }

    }
}

﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace WpfGUI
{
    // <summary>
    // MainWindow.xaml 的交互逻辑
    // </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker encryptworker;
        private BackgroundWorker decryptworker;
        private Key newKey;
        private Encryption encryption;
        private Decryption decryption;
        private DateTime time1;
        private DateTime time2;
        private string inFilePath;
        private string outFilePath;
        private long datasize;
        private int fileprocessresult;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBackgroundWorker();

            getKeyButton.Click += new RoutedEventHandler(GetKey);
            savePrivateKeyButton.Click += new RoutedEventHandler(SavePrivateKey);
            savePublicKeyButton.Click += new RoutedEventHandler(SavePublicKey);

            getPublicKeyFilePathButton.Click += new RoutedEventHandler(GetPublicKeyFilePath);
            encryptionTypeComboBox.SelectionChanged += ChangeEncryptionInterface;
            encryptButton.Click += new RoutedEventHandler(Encrypt);
            getPlainFilePathButton.Click += new RoutedEventHandler(GetPlainFilePath);
            getCipherFilePathButton.Click += new RoutedEventHandler(GetCipherFilePath);

            getPrivateKeyFilePathButton.Click += new RoutedEventHandler(GetPrivateKeyFilePath);
            decryptionTypeComboBox.SelectionChanged += ChangeDecryptionInterface;
            decryptButton.Click += new RoutedEventHandler(Decrypt);
            getEncryptedFilePathButton.Click += new RoutedEventHandler(GetEncryptedFilePath);
            getDecryptedFilePathButton.Click += new RoutedEventHandler(GetDecryptedFilePath);

            plaintextTextBox.GotFocus += RealTimeEncryptActivation;
            plaintextTextBox.TextChanged += RealTimeEncrypt;
            encryptedtextTextBox.GotFocus += RealTimeDecryptActivation;
            encryptedtextTextBox.TextChanged += RealTimeDecrypt;

            aboutTextBlock.Text = "此程序使用ECC实现文本和文件加密。\n私钥文件后缀为.key\n公钥文件后缀为.pub\n加密文件的后缀为.encrypted";
        }

        private void InitializeBackgroundWorker()
        {
            encryptworker = new BackgroundWorker();
            decryptworker = new BackgroundWorker();
            encryptworker.WorkerReportsProgress = false;
            encryptworker.WorkerSupportsCancellation = true;
            decryptworker.WorkerReportsProgress = false;
            decryptworker.WorkerSupportsCancellation = true;

            encryptworker.DoWork += new DoWorkEventHandler(EncryptWorker);
            encryptworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(EncryptWorkerRunWorkerCompleted);
            decryptworker.DoWork += new DoWorkEventHandler(DecryptWorker);
            decryptworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DecryptWorkerRunWorkerCompleted);

        }

        //密钥对生成的方法
        private void GetKey(object sender, RoutedEventArgs e)
        {
            newKey = new Key();
            newKey.GetKey(curveNameComboBox.Text);
            privateKeyTextBox.Text = newKey.privatekey.ToString();
            publicKeyTextBox.Text = newKey.publickey.ToString();
        }
        private void SavePrivateKey(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "私钥文件|*.key";
            saveFileDialog.Title = "保存私钥";
            if (saveFileDialog.ShowDialog() == true)
            {
                newKey.SavePrivateKey(saveFileDialog.FileName);
            }
        }
        private void SavePublicKey(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "公钥文件|*.pub";
            saveFileDialog.Title = "保存私钥";
            if (saveFileDialog.ShowDialog() == true)
            {
                newKey.SavePublicKey(saveFileDialog.FileName);
            }
        }

        //实时加密和解密
        private void RealTimeEncryptActivation(object sender, RoutedEventArgs e)
        {
            try
            {
                if (publicKeyFilePathTextBox.Text != "")
                {
                    encryption = new Encryption(File.ReadAllText(publicKeyFilePathTextBox.Text));
                }
                else
                {
                    encryption = null;
                }
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }
        private void RealTimeEncrypt(object sender, TextChangedEventArgs e)
        {
            try
            {
                if ((plaintextTextBox.Text != "") && (encryption != null) && (encryption.flag == 0))
                {
                    byte[] plain = Encoding.Default.GetBytes(plaintextTextBox.Text);
                    byte[] cipher = encryption.Encrypt(plain);
                    ciphertextTextBox.Text = String.Concat(Array.ConvertAll(cipher, x => x.ToString("X2")));
                }
                else
                {
                    ciphertextTextBox.Text = "";
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }

        }
        private void RealTimeDecryptActivation(object sender, RoutedEventArgs e)
        {
            try
            {
                if (privateKeyFilePathTextBox.Text != "")
                {
                    decryption = new Decryption(File.ReadAllText(privateKeyFilePathTextBox.Text));
                }
                else
                {
                    decryption = null;
                }
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }
        private void RealTimeDecrypt(object sender, TextChangedEventArgs e)
        {
            try
            {
                if ((encryptedtextTextBox.Text != "") && ((encryptedtextTextBox.Text.Trim().Length - 58) % 32 == 0) && (decryption != null))
                {
                    int encryptedlength = encryptedtextTextBox.Text.Trim().Length >> 1;
                    byte[] encrypted = new byte[encryptedlength];
                    for (int i = 0; i < encryptedlength; i++)
                    {
                        encrypted[i] = Byte.Parse(encryptedtextTextBox.Text.Trim().Substring(i * 2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    byte[] decrypted = decryption.Decrypt(encrypted);
                    if (decryption.flag == 0)
                    {
                        decryptedtextTextBox.Text = Encoding.Default.GetString(decrypted);
                    }
                    else
                    {
                        decryptedtextTextBox.Text = "";
                    }
                }
                else
                {
                    decryptedtextTextBox.Text = "";
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        //加密的方法
        private void GetPublicKeyFilePath(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "公钥文件|*.pub";
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "选择公钥文件";
            if (openFileDialog.ShowDialog() == true)
            {
                publicKeyFilePathTextBox.Text = openFileDialog.FileName;
            }
        }
        private void ChangeEncryptionInterface(object sender, SelectionChangedEventArgs e)
        {
            if (encryptionTypeComboBox.SelectedIndex == 0)
            {
                textEncryptionInterface.Visibility = Visibility.Visible;
                filetEncryptionInterface.Visibility = Visibility.Collapsed;
            }
            else if (encryptionTypeComboBox.SelectedIndex == 1)
            {
                textEncryptionInterface.Visibility = Visibility.Collapsed;
                filetEncryptionInterface.Visibility = Visibility.Visible;
            }
        }
        private void Encrypt(object sender, RoutedEventArgs e)
        {
            try
            {
                if (publicKeyFilePathTextBox.Text == "")
                {
                    throw new ArgumentException("公钥文件路径为空！");
                }
                if (!File.Exists(publicKeyFilePathTextBox.Text))
                {
                    throw new ArgumentException("公钥文件不存在！");
                }
                statusBar.Visibility = Visibility.Visible;
                statusBarTextBlock.Text = "读取公钥文件";
                encryption = new Encryption(File.ReadAllText(publicKeyFilePathTextBox.Text));
                if (encryption.flag == 1)
                {
                    throw new ArgumentException("公钥错误！");
                }
                //文本加密
                if (encryptionTypeComboBox.SelectedIndex == 0)
                {
                    if (plaintextTextBox.Text == "")
                    {
                        throw new ArgumentException("明文为空！");
                    }
                    byte[] plain = Encoding.Default.GetBytes(plaintextTextBox.Text);
                    byte[] cipher = encryption.Encrypt(plain);
                    ciphertextTextBox.Text = String.Concat(Array.ConvertAll(cipher, x => x.ToString("X2")));
                    statusBar.Visibility = Visibility.Collapsed;
                    statusBarTextBlock.Text = "";
                    encryption = null;
                }
                //文件加密
                else if (encryptionTypeComboBox.SelectedIndex == 1)
                {

                    if (plainFilePathTextBox.Text == "")
                    {
                        throw new ArgumentException("原文件路径为空！");
                    }
                    if (cipherFilePathTextBox.Text == "")
                    {
                        throw new ArgumentException("加密文件保存路径为空！");
                    }
                    if (!File.Exists(plainFilePathTextBox.Text))
                    {
                        throw new ArgumentException("原文件不存在！");
                    }
                    if (File.Exists(cipherFilePathTextBox.Text))
                    {
                        if (new FileInfo(cipherFilePathTextBox.Text).IsReadOnly)
                        {
                            throw new ArgumentException("加密文件只读，无法覆盖！");
                        }
                    }
                    inFilePath = plainFilePathTextBox.Text;
                    datasize = new FileInfo(inFilePath).Length;
                    outFilePath = cipherFilePathTextBox.Text;
                    statusBarTextBlock.Text = "加密文件中...";

                    if (encryptworker.IsBusy != true)
                    {
                        encryptworker.RunWorkerAsync();
                    }
                    else
                    {
                        MessageBox.Show("上一个加密操作尚未完成！");
                    }
                }
            }
            catch (ArgumentException error)
            {
                statusBar.Visibility = Visibility.Collapsed;
                statusBarTextBlock.Text = "";
                MessageBox.Show(error.Message.ToString());
            }
            catch (FileNotFoundException error)
            {
                statusBar.Visibility = Visibility.Collapsed;
                statusBarTextBlock.Text = "";
                MessageBox.Show(error.Message.ToString());
            }
            catch (UnauthorizedAccessException error)
            {
                MessageBox.Show(error.Message.ToString());
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }
        private void EncryptWorker(object sender, DoWorkEventArgs e)
        {
            time1 = DateTime.Now;
            fileprocessresult = -1;
            fileprocessresult = encryption.Encrypt(inFilePath, outFilePath);
            encryption = null;
        }
        private void EncryptWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (fileprocessresult == 0)
            {
                time2 = DateTime.Now;
                statusBar.Visibility = Visibility.Collapsed;
                statusBarTextBlock.Text = "";
                MessageBox.Show("耗时:" + (time2 - time1).ToString("c").Substring(0, 11) + "\n" + "速度:" + (datasize / (time2 - time1).TotalSeconds / 1048576).ToString("f3") + "MB/s" + "\n" + "加密完成");
            }
            else if (fileprocessresult == 1)
            {
                MessageBox.Show("打开文件错误！");
            }
            else if (fileprocessresult == 2)
            {
                MessageBox.Show("保存文件错误！");
            }
            else if (fileprocessresult == 3)
            {
                MessageBox.Show("公钥错误！");
            }
            else
            {
                MessageBox.Show("发生未知错误！\n" + "错误代码：" + fileprocessresult);
            }
        }
        private void GetPlainFilePath(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "选择文件";
            if (openFileDialog.ShowDialog() == true)
            {
                plainFilePathTextBox.Text = openFileDialog.FileName;
                cipherFilePathTextBox.Text = openFileDialog.FileName + ".encrypted";
            }
        }
        private void GetCipherFilePath(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "加密文件|*.encrypted";
            saveFileDialog.Title = "选择文件保存位置";
            if (cipherFilePathTextBox.Text != "")
            {
                saveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(cipherFilePathTextBox.Text);
                saveFileDialog.FileName = System.IO.Path.GetFileName(cipherFilePathTextBox.Text);
            }
            if (saveFileDialog.ShowDialog() == true)
            {
                cipherFilePathTextBox.Text = saveFileDialog.FileName;
            }
        }

        //解密的方法
        private void GetPrivateKeyFilePath(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "私钥文件|*.key";
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "选择私钥文件";
            if (openFileDialog.ShowDialog() == true)
            {
                privateKeyFilePathTextBox.Text = openFileDialog.FileName;
            }
        }
        private void ChangeDecryptionInterface(object sender, SelectionChangedEventArgs e)
        {
            if (decryptionTypeComboBox.SelectedIndex == 0)
            {
                textDecryptionInterface.Visibility = Visibility.Visible;
                filetDecryptionInterface.Visibility = Visibility.Collapsed;
            }
            else if (decryptionTypeComboBox.SelectedIndex == 1)
            {
                textDecryptionInterface.Visibility = Visibility.Collapsed;
                filetDecryptionInterface.Visibility = Visibility.Visible;
            }
        }
        private void Decrypt(object sender, RoutedEventArgs e)
        {
            try
            {
                if (privateKeyFilePathTextBox.Text == "")
                {
                    throw new ArgumentException("私钥文件路径为空！");
                }
                if (!File.Exists(privateKeyFilePathTextBox.Text))
                {
                    throw new ArgumentException("私钥文件不存在！");
                }
                statusBar.Visibility = Visibility.Visible;
                statusBarTextBlock.Text = "读取私钥文件";
                decryption = new Decryption(File.ReadAllText(privateKeyFilePathTextBox.Text));
                if (decryptionTypeComboBox.SelectedIndex == 0)
                {
                    if (encryptedtextTextBox.Text == "")
                    {
                        throw new ArgumentException("密文为空！");
                    }
                    if ((encryptedtextTextBox.Text.Trim().Length - 58) % 32 != 0)
                    {
                        throw new ArgumentException("密文格式错误！");
                    }
                    int encryptedlength = encryptedtextTextBox.Text.Trim().Length >> 1;
                    byte[] encrypted = new byte[encryptedlength];
                    for (int i = 0; i < encryptedlength; i++)
                    {
                        encrypted[i] = Byte.Parse(encryptedtextTextBox.Text.Trim().Substring(i * 2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    byte[] decrypted = decryption.Decrypt(encrypted);
                    if (decryption.flag == 0)
                    {
                        decryptedtextTextBox.Text = Encoding.Default.GetString(decrypted);
                        statusBar.Visibility = Visibility.Collapsed;
                        statusBarTextBlock.Text = "";
                        MessageBox.Show("解密完成");
                    }
                    else if (decryption.flag == 1)
                    {
                        decryptedtextTextBox.Text = "";
                        statusBar.Visibility = Visibility.Collapsed;
                        statusBarTextBlock.Text = "";
                        MessageBox.Show("加密数据已损坏！");
                    }
                    else if (decryption.flag == 2)
                    {
                        decryptedtextTextBox.Text = "";
                        statusBar.Visibility = Visibility.Collapsed;
                        statusBarTextBlock.Text = "";
                        MessageBox.Show("私钥文件不匹配！");
                    }
                    decryption = null;
                }
                else if (decryptionTypeComboBox.SelectedIndex == 1)
                {
                    if (encryptedFilePathTextBox.Text == "")
                    {
                        throw new ArgumentException("加密文件路径为空！");
                    }
                    if (decryptedFilePathTextBox.Text == "")
                    {
                        throw new ArgumentException("解密文件保存路径为空！");
                    }
                    if (!File.Exists(encryptedFilePathTextBox.Text))
                    {
                        throw new ArgumentException("加密文件不存在！");
                    }
                    if (File.Exists(decryptedFilePathTextBox.Text))
                    {
                        if (new FileInfo(decryptedFilePathTextBox.Text).IsReadOnly)
                        {
                            throw new ArgumentException("解密文件只读，无法覆盖！");
                        }
                    }
                    inFilePath = encryptedFilePathTextBox.Text;
                    outFilePath = decryptedFilePathTextBox.Text;
                    datasize = 0;
                    statusBarTextBlock.Text = "解密文件中...";

                    if (decryptworker.IsBusy != true)
                    {
                        decryptworker.RunWorkerAsync();
                    }
                    else
                    {
                        MessageBox.Show("上一个解密操作尚未完成！");
                    }
                }
            }
            catch (ArgumentException error)
            {
                statusBar.Visibility = Visibility.Collapsed;
                statusBarTextBlock.Text = "";
                MessageBox.Show(error.Message.ToString());
            }
            catch (FileNotFoundException error)
            {
                statusBar.Visibility = Visibility.Collapsed;
                statusBarTextBlock.Text = "";
                MessageBox.Show(error.Message.ToString());
            }
            catch (UnauthorizedAccessException error)
            {
                MessageBox.Show(error.Message.ToString());
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }
        private void DecryptWorker(object sender, DoWorkEventArgs e)
        {
            time1 = DateTime.Now;
            fileprocessresult = -1;
            fileprocessresult = decryption.Decrypt(inFilePath, outFilePath);
            decryption = null;
        }
        private void DecryptWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (fileprocessresult == 0)
            {
                time2 = DateTime.Now;
                datasize = new FileInfo(outFilePath).Length;
                MessageBox.Show("耗时:" + (time2 - time1).ToString("c").Substring(0, 11) + "\n" + "速度:" + (datasize / (time2 - time1).TotalSeconds / 1048576).ToString("f3") + "MB/s" + "\n" + "解密完成");
            }
            else if (fileprocessresult == 1)
            {
                MessageBox.Show("打开加密文件错误！");
            }
            else if (fileprocessresult == 2)
            {
                MessageBox.Show("无法保存解密文件！");
            }
            else if (fileprocessresult == 3)
            {
                MessageBox.Show("解密文件受损，解密失败！");
            }
            else if (fileprocessresult == 4)
            {
                MessageBox.Show("私钥错误！");
            }
            else
            {
                MessageBox.Show("发生未知错误！\n" + "错误代码：" + fileprocessresult);
            }
            statusBar.Visibility = Visibility.Collapsed;
            statusBarTextBlock.Text = "";
        }
        private void GetEncryptedFilePath(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "加密文件|*.encrypted";
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "选择加密文件";
            if (openFileDialog.ShowDialog() == true)
            {
                encryptedFilePathTextBox.Text = openFileDialog.FileName;
                decryptedFilePathTextBox.Text = openFileDialog.FileName.Substring(0, openFileDialog.FileName.Length - 10);
            }
        }
        private void GetDecryptedFilePath(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "选择文件保存位置";
            if (decryptedFilePathTextBox.Text != "")
            {
                saveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(decryptedFilePathTextBox.Text);
                saveFileDialog.FileName = System.IO.Path.GetFileName(decryptedFilePathTextBox.Text);
            }
            if (saveFileDialog.ShowDialog() == true)
            {
                decryptedFilePathTextBox.Text = saveFileDialog.FileName;
            }
        }
    }

    class Key
    {
        [DllImport("basefunc.dll", EntryPoint = "get_key", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        extern static int get_key([MarshalAs(UnmanagedType.LPStr)] string curve, [MarshalAs(UnmanagedType.LPStr)] StringBuilder privatekey, [MarshalAs(UnmanagedType.LPStr)] StringBuilder public_x, [MarshalAs(UnmanagedType.LPStr)] StringBuilder public_y);

        public String curvename { get; private set; }
        public StringBuilder privatekey { get; private set; }
        public StringBuilder publickey { get; private set; }
        public StringBuilder public_x { get; private set; }
        public StringBuilder public_y { get; private set; }

        public Key()
        {
            privatekey = new StringBuilder(65);
            publickey = new StringBuilder();
            public_x = new StringBuilder(65);
            public_y = new StringBuilder(65);
        }
        //根据曲线名生成密钥对
        public void GetKey(string curvename)
        {
            this.curvename = curvename;
            get_key(curvename, privatekey, public_x, public_y);
            publickey.Append(public_x).Append('\n').Append(public_y);
        }
        //保存私钥
        public void SavePrivateKey(string path)
        {
            File.WriteAllText(path, curvename + '\n' + privatekey.ToString());
        }
        //保存公钥
        public void SavePublicKey(string path)
        {
            File.WriteAllText(path, curvename + '\n' + publickey.ToString());
        }
    }

    class Encryption
    {
        [DllImport("basefunc.dll", EntryPoint = "ecc_encrypt@text", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        extern static IntPtr textencrypt([MarshalAs(UnmanagedType.LPStr)] string curve, [MarshalAs(UnmanagedType.LPStr)] string pub_x, [MarshalAs(UnmanagedType.LPStr)] string pub_y, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] info, ulong info_length_byte, ref ulong cipherdata_length_byte);
        [DllImport("basefunc.dll", EntryPoint = "ecc_encrypt@file", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        extern static int fileencrypt([MarshalAs(UnmanagedType.LPStr)] string curve, [MarshalAs(UnmanagedType.LPStr)] string pub_x, [MarshalAs(UnmanagedType.LPStr)] string pub_y, [MarshalAs(UnmanagedType.LPStr)] string infile, [MarshalAs(UnmanagedType.LPStr)] string outfile);
        [DllImport("basefunc.dll", EntryPoint = "wpoc", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        extern static int whetherpointisoncurve([MarshalAs(UnmanagedType.LPStr)] string curve, [MarshalAs(UnmanagedType.LPStr)] string pub_x, [MarshalAs(UnmanagedType.LPStr)] string pub_y);

        public string curve;
        public string pub_x;
        public string pub_y;
        public int flag = 1;

        public Encryption(string publickey)
        {
            publickey = publickey.Replace("\r\n", "\n");
            string[] publickeys = publickey.Split('\n');

            curve = publickeys[0];
            pub_x = publickeys[1];
            pub_y = publickeys[2];
            flag = whetherpointisoncurve(curve, pub_x, pub_y);
        }
        public Encryption(string curve, string pub_x, string pub_y)
        {
            this.curve = curve;
            this.pub_x = pub_x;
            this.pub_y = pub_y;
            flag = whetherpointisoncurve(curve, pub_x, pub_y);
        }
        public byte[] Encrypt(byte[] plain)
        {
            ulong cipherdata_length_byte = 0;
            IntPtr temp = textencrypt(curve, pub_x, pub_y, plain, (ulong)plain.Length, ref cipherdata_length_byte);
            byte[] cipher = new byte[cipherdata_length_byte];
            Marshal.Copy(temp, cipher, 0, (int)cipherdata_length_byte);
            Marshal.FreeHGlobal(temp);
            return cipher;
        }
        public int Encrypt(string infile, string outfile)
        {
            return fileencrypt(curve, pub_x, pub_y, infile, outfile);
        }
    }

    class Decryption
    {
        [DllImport("basefunc.dll", EntryPoint = "ecc_decrypt@text", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        extern static IntPtr textdecrypt([MarshalAs(UnmanagedType.LPStr)] string key, [MarshalAs(UnmanagedType.LPArray)] byte[] secret, ulong cipherdata_length_byte, ref ulong plaindata_length_byte, ref int flag);
        [DllImport("basefunc.dll", EntryPoint = "ecc_decrypt@file", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        extern static int filedecrypt([MarshalAs(UnmanagedType.LPStr)] string key, [MarshalAs(UnmanagedType.LPStr)] string infile, [MarshalAs(UnmanagedType.LPStr)] string outfile);

        public string privatekey;
        public int flag = 0;

        public Decryption(string privatekey)
        {
            privatekey = privatekey.Replace("\r\n", "\n");
            this.privatekey = privatekey.Split('\n')[1];
        }

        public byte[] Decrypt(byte[] encrypted)
        {
            if ((encrypted[20] == 0x00) && (privatekey.Length != 40) || (encrypted[20] == 0x01) && (privatekey.Length != 48) || (encrypted[20] == 0x02) && (privatekey.Length != 56) || (encrypted[20] == 0x03) && (privatekey.Length != 64))
            {
                throw new ArgumentException("私钥文件不匹配！");
            }

            ulong decrypteddata_length_byte = 0;
            IntPtr temp = textdecrypt(privatekey, encrypted, (ulong)encrypted.Length, ref decrypteddata_length_byte, ref flag);
            byte[] decrypted = new byte[decrypteddata_length_byte];
            Marshal.Copy(temp, decrypted, 0, (int)decrypteddata_length_byte);
            Marshal.FreeHGlobal(temp);
            return decrypted;
        }
        public int Decrypt(string infile, string outfile)
        {
            return filedecrypt(privatekey, infile, outfile);
        }
    }
}

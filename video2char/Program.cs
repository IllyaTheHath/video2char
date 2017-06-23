using System;
using System.Drawing;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace video2char {
    class Program {

        private static string app_dir = System.AppDomain.CurrentDomain.BaseDirectory;
        private static string img_dir = "img";
        private static string txt_dir = "txt";

        private static void Main(string[] args) {
            Console.WriteLine("Input name of video/gif,use \"reload\" to play the already converted file");
            string vname =  Console.ReadLine();

            if (vname == "reload") {
                if (!Play()) {
                    Console.Write("Play Error");
                    return;
                }
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Input Resolution of video/gif,  dafalut 800x450");
            string reso = Console.ReadLine();
            if (reso == "" || reso == null) {
                reso = "800x450";
            }

            Console.WriteLine("Input frame rate, default 10");
            string frame = Console.ReadLine();
            if (frame == "" || frame == null) {
                frame = "10";
            }

            // 如果文件夹存在则清空内容
            // 如果文件夹不存在则新建
            if (Directory.Exists(app_dir + img_dir)) {
                DelectFile(app_dir + img_dir);
            } else {
                Directory.CreateDirectory(app_dir + img_dir);
            }
            if (Directory.Exists(app_dir + txt_dir)) {
                DelectFile(app_dir + txt_dir);
            } else {
                Directory.CreateDirectory(app_dir + txt_dir);
            }

            Console.Write("Converting video to image...");
            Video2Pic(vname, reso, frame);

            if (!Pic2Char()) {
                Console.Write("Pic2Char Error");
                return;
            }

            // 暂停5秒提示
            Console.Clear();
            for (int i = 3; i > 0; i--) {
                Console.Write("\rConversion over, play in {0} seconds", i);
                Thread.Sleep(1000);
            }


            if (!Play()) {
                Console.Write("Play Error");
                return;
            }

            Console.ReadKey();
        }

        //视频转图片
        private static void Video2Pic(string VideoName, string reso, string frame) {
            Process p = new Process();//建立外部调用线程
            p.StartInfo.FileName = "ffmpeg.exe";
            p.StartInfo.Arguments = " -i " + VideoName + " -r " + frame + " -f image2 -s " + reso + " img/%d.jpg"; // 每秒帧数,分辨率
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit();//阻塞等待进程结束
            p.Close();
            p.Dispose();
        }

        //图片转字符
        private static Boolean Pic2Char() {
            long len = 0;
            int fileNum = 0;
            DirectoryInfo di = new DirectoryInfo(app_dir + img_dir);
            foreach (FileInfo fi in di.GetFiles()) {
                len += fi.Length;
            }
            fileNum += di.GetFiles().Length;
            if (fileNum == 0) {
                return false;
            }
            Console.Clear();
            for (int i = 0; i < fileNum; i++) {
                Console.Write("\r{0}%..Converting the no.{1} image to ASCII code...", ((i + 1) * 100 / fileNum), i + 1);
                try {
                    Bitmap bm = new Bitmap(Path.Combine(img_dir, string.Format("{0}.jpg", i + 1)));
                    StreamWriter sw = new StreamWriter(Path.Combine(txt_dir, string.Format("{0}.txt", i + 1)), false, Encoding.ASCII);

                    for (int j = 0; j < bm.Height; j += 8) {
                        for (int k = 0; k < bm.Width; k += 5) {
                            byte g = bm.GetPixel(k, j).G;

                            if (g < 80) {
                                sw.Write(" ");
                            }else if (g >= 75 && g <100) {
                                sw.Write("-");
                            } else if (g >= 100 && g < 120) {
                                sw.Write(":");
                            } else if (g >= 120 && g < 150) {
                                sw.Write("+");
                            } else if (g >= 150 && g < 175) {
                                sw.Write("=");
                            } else if (g >= 175 && g < 200) {
                                sw.Write("*");
                            } else {
                                sw.Write("#");
                            }
                        }

                        sw.Write("\r\n");
                    }
                    sw.Close();
                    bm.Dispose();
                } catch {
                    return false;
                }
            }
            return true;
        }

        //输出字符文件序列
        private static Boolean Play() {
            long len = 0;
            int fileNum = 0;
            DirectoryInfo di = new DirectoryInfo(app_dir + txt_dir);
            foreach (FileInfo fi in di.GetFiles()) {
                len += fi.Length;
            }
            fileNum += di.GetFiles().Length;
            if (fileNum == 0) {
                return false;
            }
            try {
                for (int i = 0; i < fileNum; i++) {
                    StreamReader sr = new StreamReader(Path.Combine(txt_dir, string.Format("{0}.txt", i + 1)), Encoding.ASCII);
                    Console.SetCursorPosition(0, 0);//防抖动

                    while (!sr.EndOfStream) {
                        Console.WriteLine(sr.ReadLine());
                    }

                    Thread.Sleep(100);
                    //Console.Clear();
                    sr.Close();
                }
            } catch {
                return false;
            }

            return true;
        }

        public static void DelectFile(string srcPath) {
            try {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
                foreach (FileSystemInfo i in fileinfo) {
                    if (i is DirectoryInfo)            //判断是否文件夹
                    {
                        DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                        subdir.Delete(true);          //删除子目录和文件
                    } else {
                        File.Delete(i.FullName);      //删除指定文件
                    }
                }
            } catch (Exception e) {
                throw;
            }
        }

    }
}

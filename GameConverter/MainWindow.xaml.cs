using System.Windows;
using System.IO;
using Path = System.IO.Path;
using Iros._7th.Workshop;

namespace GameConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class App : Application
    {

        void Application_Startup(object sender, StartupEventArgs e)
        {

            string steam;
            string ff7or;

            // Console.WriteLine(e.Args.Length);


            // MessageBox.Show(e.Args[0].ToString());
            // MessageBox.Show(e.Args[1].ToString());
            // MessageBox.Show(e.Args[2].ToString());

            if (e.Args.Length == 3)
            {
                Console.WriteLine(e.Args[0].ToString());
                Console.WriteLine(e.Args[1].ToString());
                Console.WriteLine(e.Args[2].ToString());
                if (e.Args[0] == "run")
                {
                    steam = e.Args[1];
                    ff7or = e.Args[2];

                    RunConvert(steam, ff7or);
                    Console.WriteLine("Game converted");
                    Environment.Exit(0);
                }
                else
                {

                    Console.WriteLine("Game not converted");
                    Environment.Exit(1);
                }

            }
            else
            {

                Console.WriteLine("Game not converted");
                Environment.Exit(1);
            }

        }
        
       

        private void RunConvert(string steam_path, string ff7or_path)
        {
            // use game converter to copy files over
            StandAloneGameConverter converter = new StandAloneGameConverter(steam_path);

            // MessageBox.Show(converter.InstallPath);

            if (converter.IsGamePirated())
            {
                // MessageBox.Show("GAME PIRATED");
                System.Windows.Forms.Application.Exit();
            }

            // if (converter.IsGameLocatedInSystemFolders())



            // MessageBox.Show("VerifyFullInstallation START");
            if (!converter.VerifyFullInstallation())
            {

                // MessageBox.Show("Verify ERROR");
                System.Windows.Forms.Application.Exit();
            }

            // MessageBox.Show("VerifyAdditionalFilesExist START");
            if (!converter.VerifyAdditionalFilesExist())
            {
                // MessageBox.Show("Verify Additional Files ERROR");
                System.Windows.Forms.Application.Exit();
            }


            // copy installation and update settings with new path
            string newInstallationPath = ff7or_path;
            bool didCopy = converter.CopyGame(newInstallationPath);

            if (!didCopy)
            {
                // MessageBox.Show("COPY ERROR");
            }
            // MessageBox.Show("COPY OK");

            // update settings with new path
            // Sys.Settings.SetPathsFromInstallationPath(newInstallationPath);
            converter.InstallPath = newInstallationPath;

            // MessageBox.Show("UPDATE SETTINGS OK");

            converter.CreateMissingDirectories();

            // MessageBox.Show("CreateMissingDirectories OK");


            // converter.ConvertToEnglishInstall();
            //MessageBox.Show("ConvertToEnglishInstall OK");

            // MessageBox.Show("COPY START");

            string pathToFF7Movies = $"{newInstallationPath}\\data\\movies";
                
            // MessageBox.Show(pathToFF7Movies);

            converter.CopyMovieFilesToFolder(pathToFF7Movies);

            bool movies_ok = converter.AllMovieFilesExist(pathToFF7Movies);

            if (!movies_ok)
            {


                if (!converter.CopyMovieFilesToFolder(pathToFF7Movies))
                {


                    // MessageBox.Show("movies ERROR");


                }
            }

            if (!converter.AllMusicFilesExist())
            {

                converter.CopyMusicFiles();

                if (!converter.AllMusicFilesExist())
                {


                    // MessageBox.Show("Music ERROR");


                }
            }
            // MessageBox.Show("COPY END");

            // MessageBox.Show(converter.InstallPath);
            // MessageBox.Show(converter.BackupFolderName);
            // MessageBox.Show(DateTime.Now.ToString("yyyyMMddHHmmss"));

            string backupFolderPath = Path.Combine(converter.InstallPath, converter.BackupFolderName, DateTime.Now.ToString("yyyyMMddHHmmss"));

            // MessageBox.Show("BAckup OK");

            converter.CheckAndCopyOldGameConverterFiles(backupFolderPath);

            // MessageBox.Show("check OK");
            /*
            if (new FileInfo(Sys.Settings.FF7Exe).Name.Equals("ff7_en.exe", StringComparison.InvariantCultureIgnoreCase))
            {
                // only compare exes are different if ff7.exe set in Settings (and not something like ff7_bc.exe)
                if (converter.IsExeDifferent())
                {
                    if (converter.BackupExe(backupFolderPath))
                    {
                        didCopy = converter.CopyFF7ExeToGame();

                        if (!didCopy)
                        {
                            MessageBox.Show("exe copy bad");
                            System.Windows.Forms.Application.Exit();

                        }
                    }
                    else
                    {
                        MessageBox.Show("backup exe copy bad");
                    }
                }
            }
            */
            /* 
            if (converter.IsConfigExeDifferent())
            {
                if (converter.BackupFF7ConfigExe(backupFolderPath))
                {
                    didCopy = converter.CopyFF7ConfigExeToGame();
                    if (!didCopy)
                    {

                        MessageBox.Show("exe copy to game bad");
                    }
                }
                else
                {
                    MessageBox.Show("exe copy to game 2 bad");
                }
            }
            */
            Console.WriteLine("Game converted");
            System.Environment.Exit(0);
        }

    }
}
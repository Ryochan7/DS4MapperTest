using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace DS4MapperTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private AppGlobalData appGlobal;
        private BackendManager manager;
        public BackendManager Manager { get => manager; }

        private Thread testThread;
        private Timer collectTimer;
        private ArgumentParser _parser;
        private LoggerHolder logHolder;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _parser = new ArgumentParser();
            _parser.Parse(e.Args);

            try
            {
                Process.GetCurrentProcess().PriorityClass =
                    ProcessPriorityClass.High;
            }
            catch { } // Ignore problems raising the priority.

            // Force Normal IO Priority
            IntPtr ioPrio = new IntPtr(2);
            Util.NtSetInformationProcess(Process.GetCurrentProcess().Handle,
                Util.PROCESS_INFORMATION_CLASS.ProcessIoPriority, ref ioPrio, 4);

            // Force Normal Page Priority
            IntPtr pagePrio = new IntPtr(5);
            Util.NtSetInformationProcess(Process.GetCurrentProcess().Handle,
                Util.PROCESS_INFORMATION_CLASS.ProcessPagePriority, ref pagePrio, 4);

            // Allow sleep time durations less than 16 ms
            Util.timeBeginPeriod(1);

            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            appGlobal = AppGlobalDataSingleton.Instance;
            appGlobal.FindConfigLocation();
            bool createdSkel = false;
            if (!appGlobal.appSettingsDirFound)
            {
                createdSkel = appGlobal.CreateBaseConfigSkeleton();
                if (!createdSkel)
                {
                    MessageBox.Show($"Cannot create config folder structure in {appGlobal.appdatapath}. Exiting",
                        "Test", MessageBoxButton.OK, MessageBoxImage.Error);
                    Current.Shutdown(1);
                    return;
                }

                // Only copy template profiles if app settings directory
                // was just created
                appGlobal.CheckAndCopyExampleProfiles();
            }
            else
            {
                // Create dirs for new devices if necessary
                appGlobal.CreateDeviceProfilesSkeleton();
            }

            appGlobal.RefreshBaseDriverInfo();
            appGlobal.StartupLoadAppSettings();
            if (!File.Exists(appGlobal.ControllerConfigsPath))
            {
                appGlobal.CreateControllerDeviceSettingsFile();
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Use all display space
            appGlobal.PrepareAbsMonitorBounds(string.Empty);

            testThread = new Thread(() =>
            {
                manager = new BackendManager(_parser, appGlobal);
                //manager.RequestOSD += Manager_RequestOSD;
                //manager.Start();
                //mapper = new Mapper();
                //mapper.Start();
            });

            testThread.IsBackground = true;
            testThread.Start();
            testThread.Join();

            logHolder = new LoggerHolder(manager, appGlobal);
            Logger logger = logHolder.Logger;
            logger.Info($"DS4MapperTest v. {AppGlobalData.exeversion}");
            logger.Info($"OS Version: {Environment.OSVersion}");
            logger.Info($"OS Product Name: {Util.GetOSProductName()}");
            logger.Info($"OS Release ID: {Util.GetOSReleaseId()}");

            MainWindow window = new MainWindow();
            window.PostInit(appGlobal);
            window.Show();

            window.StartCheckProcess();

            collectTimer = new Timer(GarbageTask, null, 30000, 30000);
        }

        private void GarbageTask(object state)
        {
            GC.Collect(0, GCCollectionMode.Forced, false);
        }
        
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exp = e.ExceptionObject as Exception;
            bool canAccessMain = Current.Dispatcher.CheckAccess();
            //Trace.WriteLine($"CRASHED {help}");
            Logger logger = logHolder.Logger;
            if (e.IsTerminating)
            {
                logger.Error($"Thread Crashed with message {exp.Message}");
                logger.Error(exp.ToString());

                if (canAccessMain)
                {
                    CleanShutDown();
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        CleanShutDown();
                    });
                }
            }
        }
        
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            CleanShutDown();
        }

        private void CleanShutDown()
        {
            {
                manager?.LogDebug($"Stopping manager");

                Task tempTask = Task.Run(() =>
                {
                    manager?.PreAppStopDown();
                    manager?.Stop();
                });
                tempTask.Wait();

                manager.ShutDown();

                manager?.LogDebug($"Manager stopped");
            }

            manager?.LogDebug($"Stopping program");

            LogManager.Flush();
            LogManager.Shutdown();

            //osdTestWindow.Close();
            //osdTestWindow = null;

            // Reset timer
            Util.timeEndPeriod(1);
        }
    }
}

using System;
using System.IO;
using System.Text;
using System.Threading;
using NLog;
using NLog.Targets.Wrappers;

namespace DS4MapperTest
{
    public class LoggerHolder
    {
        private Logger logger;// = LogManager.GetCurrentClassLogger();
        public Logger Logger { get => logger; }
        private ReaderWriterLockSlim logLock = new ReaderWriterLockSlim();

        public LoggerHolder(BackendManager service, AppGlobalData appGlobal)
        {
            var configuration = LogManager.Configuration;
            var wrapTarget = configuration.FindTargetByName<WrapperTargetBase>("logfile") as WrapperTargetBase;
            var fileTarget = wrapTarget.WrappedTarget as NLog.Targets.FileTarget;
            fileTarget.FileName = Path.Combine(appGlobal.appdatapath,
                AppGlobalData.LOGS_FOLDER_NAME,
                "ds4mapper_log.txt");
            fileTarget.ArchiveFileName = Path.Combine(appGlobal.appdatapath,
                AppGlobalData.LOGS_FOLDER_NAME,
                "ds4mapper_log_{#}.txt");
            LogManager.Configuration = configuration;
            LogManager.ReconfigExistingLoggers();

            logger = LogManager.GetCurrentClassLogger();

            service.Debug += WriteToLog;
            //DS4Windows.AppLogger.GuiLog += WriteToLog;
        }

        private void WriteToLog(object sender, DebugEventArgs e)
        {
            using WriteLocker locker = new WriteLocker(logLock);
            if (!e.Warning)
            {
                logger.Info(e.Message);
            }
            else
            {
                logger.Warn(e.Message);
            }
        }
    }
}

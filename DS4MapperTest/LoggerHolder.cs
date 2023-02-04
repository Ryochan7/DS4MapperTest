using System;
using System.IO;
using System.Text;
using NLog;
using NLog.Targets.Wrappers;

namespace DS4MapperTest
{
    public class LoggerHolder
    {
        private Logger logger;// = LogManager.GetCurrentClassLogger();
        public Logger Logger { get => logger; }

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

            //service.Debug += WriteToLog;
            //DS4Windows.AppLogger.GuiLog += WriteToLog;
        }

        //private void WriteToLog(object sender, DS4Windows.DebugEventArgs e)
        //{
        //    if (e.Temporary)
        //    {
        //        return;
        //    }

        //    if (!e.Warning)
        //    {
        //        logger.Info(e.Data);
        //    }
        //    else
        //    {
        //        logger.Warn(e.Data);
        //    }
        //}
    }
}

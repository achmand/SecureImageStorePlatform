using System;
using Common;
using Common.Utilities;
using DataAccess.Repositories;
using Logic.DomainObjects;

namespace Logic.Domain
{
    public sealed class LogsDomain : IBasicDomain<string, LogException>, IDisposable
    {
        #region properties and variables 

        private LogsRepository LogsRepo { get; }
        private const string Message = "Something went wrong";

        #endregion

        #region ctors 

        public LogsDomain()
        {
            LogsRepo = new LogsRepository();
        }

        #endregion

        #region public methods

        public DomainResult<string> Add(LogException logException)
        {
            var domainResult = new DomainResult<string> { MessageResult = Message };

            if (logException == null)
            {
                return domainResult;
            }

            LogsRepo.Add(logException);
            return domainResult;
        }

        public static void LogLevel(string level, string exception)
        {
            var logException = new LogException
            {
                ApplicationLevel = level,
                ExceptionName = exception, 
                DateCreated = HomelessMethods.GetCurrentTime()
            };

            new LogsDomain().Add(logException);
        }

        public void Dispose()
        {
            LogsRepo.Dispose();
        }

        #endregion

    }
}

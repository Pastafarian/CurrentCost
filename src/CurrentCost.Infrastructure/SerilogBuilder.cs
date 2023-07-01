using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace CurrentCost.Infrastructure
{
	public static class SerilogBuilder
	{
		public static ILogger Build<TImplementingType>(IConfiguration configuration)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Seq("http://localhost:5341")
                .Enrich.FromLogContext()
                .CreateLogger()
                .ForContext(typeof(TImplementingType));

            return logger;
        }

        public static ILogger Build(IConfiguration configuration)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Seq("http://localhost:5341")
                .Enrich.FromLogContext()
                .CreateLogger();

            Serilog.Debugging.SelfLog.Enable(x =>
            {
                Debug.Write(x);
                logger.Warning(x);
            });

            return logger;
        }
    }
}
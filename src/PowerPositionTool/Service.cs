using System;
using System.Configuration;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using log4net;
using PowerPosition;
using Services;
using Topshelf.Logging;
using Environment = PowerPosition.Environment;

namespace PowerPositionTool
{
    /// <summary>
    /// The power position tool windows service implementation.
    /// </summary>
    public class Service
    {
        private Environment _environment;
        private IDisposable _subscription;
        private Pipeline _pipeline;

        public void Start()
        {
            // Build the computation environment.
            _environment = new Environment
            {
                TokenSource = new CancellationTokenSource(), 
                Interval = TimeSpan.FromMinutes(XmlConvert.ToDouble(ConfigurationManager.AppSettings["schedule-interval-minutes"])),
                PowerService = new PowerService(),
                GetTrades = (p,d) => PositionReport.GetTrades(p,d),
                ValidateTrades = t => PositionReport.ValidateTrades(t),
                BuildPosition = t => PositionReport.BuildPosition(t),
                BuildReport = (rs,p) => PositionReport.BuildReport(rs,p),
                WriteReport = (rs,r) => PositionReport.WriteReport(rs,r),
                PipelineBufferSize = XmlConvert.ToInt32(ConfigurationManager.AppSettings["pipeline-buffer-size"]),
                ReportSpecification = new ReportSpecification 
                {
                        FilenameFormat = ConfigurationManager.AppSettings["report-filename-format"],
                        FilenameDateFormat = ConfigurationManager.AppSettings["report-filename-date-format"],
                        OutputPath = ConfigurationManager.AppSettings["report-output-path"],
                        Headers = ConfigurationManager.AppSettings["report-headers"],
                        LocalTimeFormat = ConfigurationManager.AppSettings["report-localtime-format"]
                }
            };
           
            // Create the position report pipelined computation.
            _pipeline = Pipeline.Create(LogManager.GetLogger("Pipeline"), _environment);

            // Run the position report pipelined computation at the specified interval until a cancellation request is received.
            _subscription =
                Observable.Generate(0,
                    i => !_environment.TokenSource.IsCancellationRequested,
                    i => i + 1,
                    i => i,
                    i => i == 0 ? TimeSpan.Zero : _environment.Interval)
                    .Timestamp()
                    .Select(x => x.Timestamp)
                    .Subscribe(_pipeline.AsObserver());
        }

        public async void Stop()
        {
            try
            {
                // Stop the postion report pipelined computation by issuing a cancellation request.
                _environment.TokenSource.Cancel();
                await Task.WhenAll(_pipeline.Completion);
            }
            catch (OperationCanceledException)
            {
                HostLogger.Get<Service>().Debug("The Power Position Tool was cancelled.");
            }
            catch (Exception ex)
            {
                HostLogger.Get<Service>().Error("The Power Position Tool completed with an error.", ex);
            }
            finally
            {
                if (_subscription != null)
                {
                    _subscription.Dispose();
                    _subscription = null;
                }

                if (_environment != null)
                {
                    _environment.TokenSource.Dispose();
                    _environment = null;
                }
            }
        }        
    }
}
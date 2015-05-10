using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using log4net;
using PowerPosition;
using Environment = PowerPosition.Environment;

namespace PowerPositionTool
{
    /// <summary>
    /// The pipelined computation used to produce position reports. The pipeline
    /// comprises these stages:
    ///   GetTrades -> ValidateTrades -> BuildPosition -> BuildReport -> WriteReport
    /// </summary>
    public class Pipeline
    {
        public static Pipeline Create(ILog log, Environment env)
        {
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            var tradesBlock = new TransformBlock<DateTimeOffset, IEither<PowerTrades,Error>>(date =>
            {
                var message = string.Format("getting trades for {0}", date);
                try
                {
                    log.Debug(message);
                    return Either.Left<PowerTrades,Error>(env.GetTrades(env.PowerService, date));
                }
                catch (Exception e)
                {
                    log.Error(message, e);
                    return Either.Right<PowerTrades, Error>(new Error { Context = message, Exception = e });
                }                
            }, ExecutionOptions(env));

            var validationBlock = new TransformBlock<IEither<PowerTrades,Error>, IEither<PowerTrades, Error>>(trades =>
            {
                var message = trades.Case(ts => string.Format("validating trades for {0}", ts.Day), e => string.Format("skip validating trades due to error {0}", e.Context));
                try
                {
                    log.Debug(message);
                    return trades.Case(ts => Either.Left<PowerTrades, Error>(env.ValidateTrades(ts)), Either.Right<PowerTrades, Error>);
                }
                catch (Exception e)
                {
                    log.Error(message, e);
                    return Either.Right<PowerTrades, Error>(new Error { Context = message, Exception = e });
                }
            }, ExecutionOptions(env));

            var positionBlock = new TransformBlock<IEither<PowerTrades,Error>, IEither<Position,Error>>(trades =>
            {
                var message = trades.Case(ts => string.Format("building position for {0}", ts.Day), e => string.Format("skip building position due to error {0}", e.Context));
                try
                {
                    log.Debug(message);
                    return trades.Case(ts => Either.Left<Position, Error>(env.BuildPosition(ts)), Either.Right<Position, Error>);
                }
                catch (Exception e)
                {
                    log.Error(message, e);
                    return Either.Right<Position, Error>(new Error { Context = message, Exception = e });
                }
            }, ExecutionOptions(env));

            var reportBlock = new TransformBlock<IEither<Position, Error>, IEither<Report, Error>>(position =>
            {
                var message = position.Case(p => string.Format("building report for {0}", p.Day), e => string.Format("skip building report due to error {0}", e.Context));
                try
                {
                    log.Debug(message);
                    return position.Case(p => Either.Left<Report, Error>(env.BuildReport(env.ReportSpecification,p)), Either.Right<Report, Error>);
                }
                catch (Exception e)
                {
                    log.Error(message, e);
                    return Either.Right<Report, Error>(new Error { Context = message, Exception = e });
                }
            }, ExecutionOptions(env));

            var writeBlock = new ActionBlock<IEither<Report,Error>>(report =>
            {
                var message = report.Case(r => string.Format("writing report for {0}", r.Day), e => string.Format("skip writing report due to error {0}", e.Context));
                try
                {
                    log.Debug(message);
                    report.Case(p => env.WriteReport(env.ReportSpecification, p), e => {});
                }
                catch (Exception e)
                {
                    log.Error(message, e);
                }
            }, ExecutionOptions(env));

            tradesBlock.LinkTo(validationBlock, linkOptions);
            validationBlock.LinkTo(positionBlock, linkOptions);
            positionBlock.LinkTo(reportBlock, linkOptions);
            reportBlock.LinkTo(writeBlock, linkOptions);

            tradesBlock.Completion.ContinueWith(t => validationBlock.Completion);
            validationBlock.Completion.ContinueWith(t => positionBlock.Completion);
            positionBlock.Completion.ContinueWith(t => reportBlock.Completion);
            reportBlock.Completion.ContinueWith(t => writeBlock.Completion);

            return new Pipeline
            {
                StartBlock = tradesBlock,
                EndBlock = writeBlock
            };
        }

        /// <summary>
        /// The pipeline observes a stream of DateTimeOffsets. Each DateTimeOffset
        /// received will cause a report to be produced.
        /// </summary>
        /// <returns></returns>
        public IObserver<DateTimeOffset> AsObserver()
        {
            return StartBlock.AsObserver();
        }

        /// <summary>
        /// When this task completes the pipeline has finished.
        /// </summary>
        public Task Completion
        {
            get { return EndBlock.Completion; }
        }

        private TransformBlock<DateTimeOffset, IEither<PowerTrades, Error>> StartBlock { get; set; }
        private ActionBlock<IEither<Report, Error>> EndBlock { get; set; }

        private static ExecutionDataflowBlockOptions ExecutionOptions(Environment env)
        {
            return new ExecutionDataflowBlockOptions
            {
                CancellationToken = env.TokenSource.Token,
                BoundedCapacity = env.PipelineBufferSize
            };
        }

    }
}

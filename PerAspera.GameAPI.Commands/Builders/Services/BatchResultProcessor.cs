using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.Builders.Services
{
    /// <summary>
    /// Processor for batch command results with aggregation and analysis capabilities
    /// Handles result validation, error reporting, and success metrics
    /// </summary>
    public class BatchResultProcessor
    {
        /// <summary>
        /// Create final batch result with success analysis
        /// </summary>
        public static BatchCommandResult CreateFinalResult(IReadOnlyList<CommandResult> results, string overrideErrorMessage = null)
        {
            if (results == null) throw new ArgumentNullException(nameof(results));

            var overallSuccess = results.TrueForAll(r => r.Success);
            var errorMessage = overrideErrorMessage ?? (overallSuccess ? null : "Some commands failed");

            return new BatchCommandResult(results.ToList(), overallSuccess, errorMessage);
        }

        /// <summary>
        /// Create timeout result with partial results
        /// </summary>
        public static BatchCommandResult CreateTimeoutResult(IReadOnlyList<CommandResult> partialResults, string timeoutMessage = null)
        {
            return new BatchCommandResult(
                partialResults?.ToList() ?? new List<CommandResult>(),
                false,
                timeoutMessage ?? "Batch execution timed out"
            );
        }

        /// <summary>
        /// Create failure result for specific command index
        /// </summary>
        public static BatchCommandResult CreateFailureResult(
            IReadOnlyList<CommandResult> results,
            int failedCommandIndex,
            string commandError)
        {
            var errorMessage = $"Command {failedCommandIndex + 1} failed: {commandError}";
            return new BatchCommandResult(results.ToList(), false, errorMessage);
        }

        /// <summary>
        /// Create result for condition evaluation failure
        /// </summary>
        public static BatchCommandResult CreateConditionFailureResult(
            IReadOnlyList<CommandResult> results,
            string conditionError)
        {
            return new BatchCommandResult(
                results.ToList(),
                false,
                $"Condition evaluation failed: {conditionError}"
            );
        }

        /// <summary>
        /// Validate partial results from parallel execution and fill gaps
        /// </summary>
        public static IReadOnlyList<CommandResult> ValidateAndFillPartialResults(
            CommandResult[] results,
            int expectedCount,
            string fillErrorMessage = "Task incomplete or faulted")
        {
            if (results == null) throw new ArgumentNullException(nameof(results));

            var validatedResults = new CommandResult[expectedCount];

            for (int i = 0; i < expectedCount; i++)
            {
                if (i < results.Length && results[i] != null)
                {
                    validatedResults[i] = results[i];
                }
                else
                {
                    // Fill gaps with error results
                    validatedResults[i] = new CommandResult(false, fillErrorMessage, null);
                }
            }

            return validatedResults.ToList();
        }

        /// <summary>
        /// Analyze results and provide detailed statistics
        /// </summary>
        public static BatchResultStatistics AnalyzeResults(IReadOnlyList<CommandResult> results)
        {
            if (results == null) throw new ArgumentNullException(nameof(results));

            var totalCount = results.Count;
            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count(r => !r.Success);
            var skippedCount = results.Count(r => r.Success && r.Message == "Skipped due to condition");
            var executedCount = totalCount - skippedCount;

            var successRate = totalCount > 0 ? (double)successCount / totalCount * 100 : 0;
            var executionRate = totalCount > 0 ? (double)executedCount / totalCount * 100 : 0;

            return new BatchResultStatistics
            {
                TotalCommands = totalCount,
                SuccessfulCommands = successCount,
                FailedCommands = failureCount,
                SkippedCommands = skippedCount,
                ExecutedCommands = executedCount,
                SuccessRate = successRate,
                ExecutionRate = executionRate,
                FirstFailure = results.FirstOrDefault(r => !r.Success),
                AllErrors = results.Where(r => !r.Success).Select(r => r.Error).ToList()
            };
        }

        /// <summary>
        /// Handle parallel execution failure with stop-on-failure logic
        /// </summary>
        public static BatchCommandResult HandleParallelFailure(
            IReadOnlyList<CommandResult> results,
            bool stopOnFailure,
            string executionType = "Parallel execution")
        {
            if (!stopOnFailure)
            {
                return CreateFinalResult(results);
            }

            var firstFailure = results.FirstOrDefault(r => !r.Success);
            if (firstFailure != null)
            {
                return new BatchCommandResult(
                    results.ToList(),
                    false,
                    $"{executionType} failed: {firstFailure.Error}"
                );
            }

            return CreateFinalResult(results);
        }

        /// <summary>
        /// Process results from indexed parallel execution
        /// </summary>
        public static IReadOnlyList<CommandResult> ProcessIndexedResults(
            IEnumerable<(int index, CommandResult result)> indexedResults,
            int expectedCount)
        {
            var results = new CommandResult[expectedCount];

            foreach (var (index, result) in indexedResults)
            {
                if (index >= 0 && index < expectedCount)
                {
                    results[index] = result;
                }
            }

            return ValidateAndFillPartialResults(results, expectedCount);
        }
    }

    /// <summary>
    /// Statistical analysis of batch command execution results
    /// </summary>
    public class BatchResultStatistics
    {
        public int TotalCommands { get; set; }
        public int SuccessfulCommands { get; set; }
        public int FailedCommands { get; set; }
        public int SkippedCommands { get; set; }
        public int ExecutedCommands { get; set; }
        public double SuccessRate { get; set; }
        public double ExecutionRate { get; set; }
        public CommandResult FirstFailure { get; set; }
        public List<string> AllErrors { get; set; }
    }
}
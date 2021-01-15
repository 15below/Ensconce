using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ensconce.Console
{
    public static class ExceptionDetails
    {
        public static string GetDetails(string message, Exception ex)
        {
            var sb = new StringBuilder(message);

            if (!message.EndsWith(Environment.NewLine))
            {
                sb.Append(Environment.NewLine);
            }

            Recurse(sb, ex, 0);

            return sb.ToString();
        }

        private static void Recurse(StringBuilder sb, Exception ex, int level = 0, int index = -1)
        {
            if (ex == null)
            {
                return;
            }

            if (ex is AggregateException agg)
            {
                Print(sb, ex, level, index);

                int i = 0;

                foreach (var subEx in agg.InnerExceptions)
                {
                    Recurse(sb, subEx, level + 1, i++);
                }
            }
            else
            {
                Recurse(sb, ex.InnerException, level);

                Print(sb, ex, level, index);
            }
        }

        private static void Print(StringBuilder sb, Exception ex, int level = 0, int index = -1)
        {
            var message = ex.Message.EndsWith(Environment.NewLine) ? ex.Message : $"{ex.Message}{Environment.NewLine}";

            if (ex.StackTrace != null)
            {
                string stackTrace = Tidy(ex.StackTrace, level);
                sb.Append($"{Environment.NewLine}{new string(' ', level * 3)}>> {(index >= 0 ? $"#{index} " : "")}{ex.GetType().Name}: {message}{Environment.NewLine}{stackTrace}{Environment.NewLine}");
            }
            else
            {
                sb.Append($"{Environment.NewLine}{new string(' ', level * 3)}>> {(index >= 0 ? $"#{index} " : "")}{ex.GetType().Name}: {message}");
            }
        }

        private const string IgnoreTaskAwaiter = "System.Runtime.CompilerServices.TaskAwaiter";
        private const string IgnoreTaskContinuation = "System.Threading.Tasks.ContinuationResultTaskFromResultTask";
        private const string IgnoreTaskExecute = "System.Threading.Tasks.Task.Execute()";
        private const string IgnoreTaskInnerInvoke = "System.Threading.Tasks.Task`1.InnerInvoke()";
        private const string IgnoreTaskGetResultCore = "System.Threading.Tasks.Task`1.GetResultCore";
        private const string IgnoreTaskGetResult = "System.Threading.Tasks.Task`1.get_Result";
        private const string IgnoreTaskFactory = "System.Threading.Tasks.TaskFactory`1.FromAsyncCoreLogic";
        private const string IgnoreValueTaskGetResult = "System.Threading.Tasks.ValueTask`1.get_Result()";

        private static readonly string[] Ignores = new string[] {
            IgnoreTaskAwaiter,
            IgnoreTaskContinuation,
            IgnoreTaskExecute,
            IgnoreTaskInnerInvoke,
            IgnoreTaskGetResultCore,
            IgnoreTaskGetResult,
            IgnoreTaskFactory,
            IgnoreValueTaskGetResult
        };

        private const string StackBreak = "--- End of stack trace from previous location where exception was thrown ---";
        private const string NewStackBreak = "   :";

        private static string Tidy(string stackTrace, int level = 0)
        {
            stackTrace = stackTrace.Replace(StackBreak, NewStackBreak);
            var lines = stackTrace.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            stackTrace = String.Join(Environment.NewLine, lines.Where(line => !Ignores.Any(ignore => line.Contains(ignore)))
                                                               .Select(TidyLine)
                                                               .Select(line => $"{new string(' ', level * 3)}{line}"));
            return stackTrace;
        }

        private static readonly Regex removeSrcDir = new Regex(@"^(\s+at .* in )(.*[\\/]src[\\/])(.*)$");

        private static string TidyLine(string line)
        {
            var match = removeSrcDir.Match(line);
            if (match.Success)
            {
                return $"{match.Groups[1].Value}{match.Groups[3].Value}";
            }
            return line;
        }
    }
}

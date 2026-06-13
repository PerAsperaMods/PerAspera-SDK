using PerAspera.Core;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Wrappers;

namespace PerAspera.GameAPI.Commands.ModActions.BuiltinActions
{
    /// <summary>
    /// Evaluates a math expression and writes the result into a universe blackboard variable.
    /// Variable names in the expression are resolved from the same blackboard at evaluation time.
    ///
    /// Supported operators : + - * /  (standard precedence, unary -)
    /// Grouping           : ( expr )
    /// Literals           : 42  3.14  1e3
    /// Variables          : any blackboard key — resolved to 0 if absent (with log warning)
    /// Functions          : min(a,b)  max(a,b)  abs(x)  floor(x)  ceil(x)
    ///                      round(x)  sqrt(x)   clamp(x, lo, hi)
    ///
    /// YAML usage:
    /// <code>
    /// # Simple increment (replaces AddBlackboardNumber)
    /// - command: SetBlackboardMath
    ///   arguments: ["ami_affinity", "ami_affinity + 10"]
    ///
    /// # Scale from another variable
    /// - command: SetBlackboardMath
    ///   arguments: ["ami_score", "ami_base * difficulty_mult + ami_bonus"]
    ///
    /// # Clamp result to a range
    /// - command: SetBlackboardMath
    ///   arguments: ["ami_affinity", "clamp(ami_affinity + reward, 0, 100)"]
    ///
    /// # Combine multiple variables
    /// - command: SetBlackboardMath
    ///   arguments: ["total_resources", "iron + aluminum * 2 + steel * 3"]
    /// </code>
    /// </summary>
    public class SetBlackboardMathAction : IModTextAction
    {
        private static readonly LogAspera _log = new LogAspera("SetBlackboardMath");
        public string CommandName => "SetBlackboardMath";

        public bool Execute(string[] args, GameCommandsReadyEvent? ctx)
        {
            var target = ActionContextHelper.GetString(args, 0);
            if (target is null)
            {
                _log.Warning($"[{CommandName}] missing target variable at args[0]");
                return false;
            }

            var expression = ActionContextHelper.GetString(args, 1);
            if (expression is null)
            {
                _log.Warning($"[{CommandName}] missing math expression at args[1]");
                return false;
            }

            var blackboard = BaseGameWrapper.GetCurrent()?.GetUniverse()?.GetMainBlackBoard();
            if (blackboard is null)
            {
                _log.Warning($"[{CommandName}] main blackboard unavailable");
                return false;
            }

            // Variable lookup: unknown variables resolve to 0 (engine behavior in criteria)
            float Lookup(string varName)
            {
                float val = blackboard.GetNumber(varName);
                if (val == 0f && !blackboard.ContainsKey(varName))
                    _log.Warning($"[{CommandName}] variable '{varName}' not found in blackboard → 0");
                return val;
            }

            var (result, ok) = BlackboardExpressionEvaluator.Evaluate(expression, Lookup, _log);
            if (!ok) return false;

            blackboard.SetValue(target, result);
            _log.Info($"[{CommandName}] {target} = {expression} → {result}");
            return true;
        }
    }
}

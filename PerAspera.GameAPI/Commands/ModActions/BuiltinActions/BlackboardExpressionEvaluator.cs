using System;
using System.Globalization;
using PerAspera.Core;

namespace PerAspera.GameAPI.Commands.ModActions.BuiltinActions
{
    /// <summary>
    /// Evaluates a math expression string, resolving variable names via a lookup delegate.
    ///
    /// Supports:
    ///   operators : +  -  *  /  (with standard precedence and unary -)
    ///   grouping  : ( expr )
    ///   literals  : integer and decimal numbers  (InvariantCulture)
    ///   variables : any [A-Za-z_][A-Za-z0-9_]* identifier → resolved by the lookup delegate
    ///   functions : min(a,b)  max(a,b)  abs(x)  floor(x)  ceil(x)  round(x)
    ///               sqrt(x)   clamp(x, lo, hi)
    ///
    /// Thread-safety: create a new <see cref="BlackboardExpressionEvaluator"/> per call,
    /// or use the static <see cref="Evaluate"/> helper which does so internally.
    ///
    /// YAML example:
    /// <code>
    /// - command: SetBlackboardMath
    ///   arguments:
    ///     - "ami_score"
    ///     - "clamp(ami_base * difficulty_mult + ami_bonus, 0, 100)"
    /// </code>
    /// </summary>
    public sealed class BlackboardExpressionEvaluator
    {
        private string _src = "";
        private int _pos;
        private Func<string, float>? _lookup;
        private LogAspera? _log;

        // ─── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Evaluate <paramref name="expression"/> and return the numeric result.
        /// Unknown variables resolve to 0 (logged as warnings).
        /// Returns (result=0, ok=false) on parse errors.
        /// </summary>
        public static (float result, bool ok) Evaluate(
            string expression,
            Func<string, float> variableLookup,
            LogAspera log)
            => new BlackboardExpressionEvaluator().Run(expression, variableLookup, log);

        // ─── Internal entry point ──────────────────────────────────────────────────

        private (float, bool) Run(string expression, Func<string, float> lookup, LogAspera log)
        {
            _src = expression?.Trim() ?? "";
            _pos = 0;
            _lookup = lookup;
            _log = log;

            if (_src.Length == 0)
            {
                log.Warning("[BlackboardMath] empty expression");
                return (0f, false);
            }

            try
            {
                float result = ParseExpr();
                SkipWs();

                if (_pos < _src.Length)
                {
                    log.Warning($"[BlackboardMath] unexpected '{_src[_pos]}' at pos {_pos} in: {expression}");
                    return (0f, false);
                }

                return (result, true);
            }
            catch (MathParseException ex)
            {
                log.Warning($"[BlackboardMath] {ex.Message} in: {expression}");
                return (0f, false);
            }
        }

        // ─── Grammar: expr → term (('+' | '-') term)* ─────────────────────────────

        private float ParseExpr()
        {
            float v = ParseTerm();
            while (true)
            {
                SkipWs();
                if (_pos >= _src.Length) break;
                char c = _src[_pos];
                if (c == '+') { _pos++; v += ParseTerm(); }
                else if (c == '-') { _pos++; v -= ParseTerm(); }
                else break;
            }
            return v;
        }

        // ─── Grammar: term → unary (('*' | '/') unary)* ───────────────────────────

        private float ParseTerm()
        {
            float v = ParseUnary();
            while (true)
            {
                SkipWs();
                if (_pos >= _src.Length) break;
                char c = _src[_pos];
                if (c == '*') { _pos++; v *= ParseUnary(); }
                else if (c == '/')
                {
                    _pos++;
                    float divisor = ParseUnary();
                    if (divisor == 0f)
                    {
                        _log!.Warning("[BlackboardMath] division by zero → 0");
                        return 0f;
                    }
                    v /= divisor;
                }
                else break;
            }
            return v;
        }

        // ─── Grammar: unary → '-' factor | factor ─────────────────────────────────

        private float ParseUnary()
        {
            SkipWs();
            if (_pos < _src.Length && _src[_pos] == '-') { _pos++; return -ParsePrimary(); }
            return ParsePrimary();
        }

        // ─── Grammar: primary → '(' expr ')' | NUMBER | IDENT ['(' args ')'] ──────

        private float ParsePrimary()
        {
            SkipWs();
            if (_pos >= _src.Length)
                throw new MathParseException("unexpected end of expression");

            char c = _src[_pos];

            if (c == '(')
            {
                _pos++;
                float v = ParseExpr();
                SkipWs();
                Expect(')', "closing ')'");
                return v;
            }

            if (char.IsDigit(c) || c == '.')
                return ParseNumber();

            if (char.IsLetter(c) || c == '_')
                return ParseIdentifier();

            throw new MathParseException($"unexpected character '{c}' at pos {_pos}");
        }

        // ─── Number ──────────────────────────────────────────────────────────────

        private float ParseNumber()
        {
            int start = _pos;
            while (_pos < _src.Length && (char.IsDigit(_src[_pos]) || _src[_pos] == '.'))
                _pos++;

            // optional scientific notation suffix: e+3  E-2
            if (_pos < _src.Length && (_src[_pos] == 'e' || _src[_pos] == 'E'))
            {
                _pos++;
                if (_pos < _src.Length && (_src[_pos] == '+' || _src[_pos] == '-')) _pos++;
                while (_pos < _src.Length && char.IsDigit(_src[_pos])) _pos++;
            }

            string raw = _src.Substring(start, _pos - start);
            if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                throw new MathParseException($"invalid number '{raw}'");
            return val;
        }

        // ─── Identifier → variable or function call ───────────────────────────────

        private float ParseIdentifier()
        {
            int start = _pos;
            while (_pos < _src.Length && (char.IsLetterOrDigit(_src[_pos]) || _src[_pos] == '_'))
                _pos++;

            string name = _src.Substring(start, _pos - start);
            SkipWs();

            if (_pos < _src.Length && _src[_pos] == '(')
                return CallFunction(name);

            // Variable lookup — unknown → 0 (with warning)
            float val = _lookup!(name);
            return val;
        }

        // ─── Function calls ───────────────────────────────────────────────────────

        private float CallFunction(string name)
        {
            _pos++; // consume '('
            var args = new System.Collections.Generic.List<float>(4);

            SkipWs();
            if (_pos < _src.Length && _src[_pos] != ')')
            {
                args.Add(ParseExpr());
                SkipWs();
                while (_pos < _src.Length && _src[_pos] == ',')
                {
                    _pos++;
                    args.Add(ParseExpr());
                    SkipWs();
                }
            }

            Expect(')', $"')' after function '{name}'");

            return name.ToLowerInvariant() switch
            {
                "min"   => RequireArgs(name, args, 2) ? Math.Min(args[0], args[1]) : 0f,
                "max"   => RequireArgs(name, args, 2) ? Math.Max(args[0], args[1]) : 0f,
                "abs"   => RequireArgs(name, args, 1) ? Math.Abs(args[0]) : 0f,
                "floor" => RequireArgs(name, args, 1) ? (float)Math.Floor(args[0]) : 0f,
                "ceil"  => RequireArgs(name, args, 1) ? (float)Math.Ceiling(args[0]) : 0f,
                "round" => RequireArgs(name, args, 1) ? (float)Math.Round(args[0]) : 0f,
                "sqrt"  => RequireArgs(name, args, 1) ? (float)Math.Sqrt(args[0]) : 0f,
                "clamp" => RequireArgs(name, args, 3) ? Math.Max(args[1], Math.Min(args[2], args[0])) : 0f,
                _       => throw new MathParseException($"unknown function '{name}'")
            };
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        private bool RequireArgs(string name, System.Collections.Generic.List<float> args, int count)
        {
            if (args.Count >= count) return true;
            throw new MathParseException($"{name}() requires {count} argument(s), got {args.Count}");
        }

        private void Expect(char expected, string description)
        {
            if (_pos >= _src.Length || _src[_pos] != expected)
                throw new MathParseException($"expected {description} at pos {_pos}");
            _pos++;
        }

        private void SkipWs()
        {
            while (_pos < _src.Length && char.IsWhiteSpace(_src[_pos]))
                _pos++;
        }

        // ─── Private exception ────────────────────────────────────────────────────

        private sealed class MathParseException : Exception
        {
            internal MathParseException(string msg) : base(msg) { }
        }
    }
}

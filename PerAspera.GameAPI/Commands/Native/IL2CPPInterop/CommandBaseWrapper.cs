using System;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Commands.Native.IL2CPPInterop
{
    /// <summary>
    /// Wrapper over native CommandBase instances.
    /// All property/method access delegates to IL2CppExtensions (RS0030-exempt in Core),
    /// eliminating direct reflection (GetProperty/GetMethod/Invoke) from GameAPI layer.
    ///
    /// MIGRATION 2026-06-10 - rewrite en delegues IL2CppExtensions.
    /// Elimine ~30 RS0030 (GetMethod/GetProperty/Invoke en GameAPI).
    /// Source de verite : Tools/InteropDump/ScriptsAssembly/PerAspera.Commands/CommandBase.cs
    ///   - faction      : virtual Faction
    ///   - senderFaction: Faction field (writable)
    ///   - isValid()    : virtual bool
    ///   - ToTabbedString(): virtual string
    /// </summary>
    public class CommandBaseWrapper
    {
        private readonly object _nativeCommand;
        private readonly System.Type _commandType;
        private static readonly LogAspera _logger = new LogAspera("GameAPI.Commands.Wrapper");

        public CommandBaseWrapper(object nativeCommand)
        {
            _nativeCommand = nativeCommand ?? throw new ArgumentNullException(nameof(nativeCommand));
            _commandType = nativeCommand.GetType();
        }

        /// <summary>Native command as object.</summary>
        public object NativeCommand => _nativeCommand;

        /// <summary>Runtime type of the native command. GetType() is not RS0030-banned.</summary>
        public System.Type CommandType => _commandType;

        /// <summary>Command name derived from type (strips "Cmd" prefix).</summary>
        public string CommandName
        {
            get
            {
                var name = _commandType.Name;
                return name.StartsWith("Cmd", StringComparison.Ordinal) ? name.Substring(3) : name;
            }
        }

        /// <summary>
        /// Calls isValid() via IL2CppExtensions.InvokeMethod (RS0030-exempt in Core).
        /// </summary>
        public bool IsValid()
        {
            try
            {
                var result = _nativeCommand.InvokeMethod<bool>("isValid");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Debug("IsValid fallback (no isValid method): " + ex.Message);
                return true;
            }
        }

        /// <summary>
        /// Returns ToTabbedString() via IL2CppExtensions.InvokeMethod (RS0030-exempt in Core).
        /// </summary>
        public string GetDescription()
        {
            try
            {
                return _nativeCommand.InvokeMethod<string>("ToTabbedString") ?? CommandName + "()";
            }
            catch (Exception ex)
            {
                _logger.Error("GetDescription error: " + ex.Message);
                return CommandName + "(error)";
            }
        }

        /// <summary>
        /// Gets faction via IL2CppExtensions.GetMemberValue (RS0030-exempt in Core).
        /// </summary>
        public object? GetFaction()
        {
            try   { return _nativeCommand.GetMemberValue<object>("faction"); }
            catch (Exception ex) { _logger.Error("GetFaction error: " + ex.Message); return null; }
        }

        /// <summary>
        /// Sets senderFaction via IL2CppExtensions.SetMemberValue (RS0030-exempt in Core).
        /// </summary>
        public bool SetFaction(object? faction)
        {
            try   { _nativeCommand.SetMemberValue("senderFaction", faction); return true; }
            catch (Exception ex) { _logger.Warning("SetFaction error: " + ex.Message); return false; }
        }

        /// <summary>
        /// Generic property getter via IL2CppExtensions.GetMemberValue (RS0030-exempt in Core).
        /// </summary>
        public object? GetProperty(string propertyName)
        {
            try   { return _nativeCommand.GetMemberValue<object>(propertyName); }
            catch (Exception ex) { _logger.Error("GetProperty " + propertyName + " error: " + ex.Message); return null; }
        }

        /// <summary>
        /// Generic property setter via IL2CppExtensions.SetMemberValue (RS0030-exempt in Core).
        /// </summary>
        public bool SetProperty(string propertyName, object? value)
        {
            try   { _nativeCommand.SetMemberValue(propertyName, value); return true; }
            catch (Exception ex) { _logger.Warning("SetProperty " + propertyName + " error: " + ex.Message); return false; }
        }

        /// <summary>
        /// Generic method invoke via IL2CppExtensions.InvokeMethod (RS0030-exempt in Core).
        /// </summary>
        public object? InvokeMethod(string methodName, params object[] parameters)
        {
            try   { return _nativeCommand.InvokeMethod<object>(methodName, parameters); }
            catch (Exception ex) { _logger.Error("InvokeMethod " + methodName + " error: " + ex.Message); return null; }
        }

        /// <summary>True if the type name matches (case-insensitive). Uses GetType().Name (not RS0030-banned).</summary>
        public bool IsCommandType(string commandTypeName)
            => _commandType.Name.Equals(commandTypeName, StringComparison.OrdinalIgnoreCase);

        public override string ToString() => GetDescription();
    }
}
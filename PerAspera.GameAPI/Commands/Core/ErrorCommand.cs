using System;
using System.Collections.Generic;

namespace PerAspera.GameAPI.Commands.Core
{
    /// <summary>
    /// Represents an error command when no specific command can be constructed
    /// </summary>
    public sealed class ErrorCommand : GameCommandBase
    {
        public override object Faction { get; }
        
        private readonly string _errorType;

        /// <summary>
        /// Create an error command with the specified error type
        /// </summary>
        public ErrorCommand(string errorType, object faction = null)
        {
            _errorType = errorType;
            Faction = faction ?? "System";
        }

        public override bool IsValid()
        {
            // Error commands are never considered valid for execution
            return false;
        }

        public override string ToString()
        {
            return $"ErrorCommand[{_errorType}]";
        }
    }
}
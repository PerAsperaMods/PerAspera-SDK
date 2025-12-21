using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Wrappers.Core;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for native TextAction class
    /// Handles command creation for InteractionManager.DispatchAction
    /// 
    /// 📚 Native Structure:
    /// - daysDelay: float - Delay before execution
    /// - command: string - Command name (e.g., "FactionAddResourceDistributed")  
    /// - arguments: List<string> - Command parameters
    /// - showInFrontend: bool - Display in UI
    /// </summary>
    public class TextAction : WrapperBase
    {
        /// <summary>
        /// Create TextAction wrapper from native TextAction instance
        /// </summary>
        public TextAction(object nativeTextAction) : base(nativeTextAction)
        {
        }
        
        /// <summary>
        /// Get/Set command delay in days
        /// </summary>
        public float DaysDelay
        {
            get => GetNativeField<float?>("daysDelay") ?? 0f;
            set => SetNativeField("daysDelay", value);
        }
        
        /// <summary>
        /// Get/Set command name
        /// </summary>
        public string Command
        {
            get => GetNativeField<string>("command") ?? "";
            set => SetNativeField("command", value);
        }
        
        /// <summary>
        /// Get/Set command arguments list
        /// </summary>
        public List<string> Arguments
        {
            get => GetNativeField<List<string>>("arguments") ?? new List<string>();
            set => SetNativeField("arguments", value);
        }
        
        /// <summary>
        /// Get/Set whether to show in frontend
        /// </summary>
        public bool ShowInFrontend
        {
            get => GetNativeField<bool?>("showInFrontend") ?? false;
            set => SetNativeField("showInFrontend", value);
        }
        
        // ==================== FACTORY METHODS ====================
        
        /// <summary>
        /// Create new TextAction with command and arguments
        /// </summary>
        /// <param name="command">Command name</param>
        /// <param name="arguments">Command arguments</param>
        /// <returns>TextAction wrapper instance</returns>
        public static TextAction? Create(string command, params string[] arguments)
        {
            try
            {
                // Use ReflectionHelpers for safer type discovery (avoids Unity type loading issues)
                var textActionType = PerAspera.Core.IL2CPP.ReflectionHelpers.FindType("TextAction");
                
                if (textActionType == null)
                {
                    Log.LogError("[TextAction] Native TextAction type not found via ReflectionHelpers");
                    return null;
                }
                
                Log.LogInfo($"[TextAction] Found TextAction type: {textActionType.FullName}");
                
                // Create native instance with constructor(string command, params string[] arguments)
                var nativeInstance = Activator.CreateInstance(textActionType, command, arguments);
                if (nativeInstance == null)
                {
                    Log.LogError("[TextAction] Failed to create native TextAction instance");
                    return null;
                }
                
                Log.LogInfo($"[TextAction] Successfully created native TextAction instance");
                return new TextAction(nativeInstance);
            }
            catch (Exception ex)
            {
                Log.LogError($"[TextAction] Create failed: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Parse tabbed string format into TextAction
        /// Format: "Command\tArg1\tArg2\tArg3"
        /// Example: "FactionAddResourceDistributed\tIce\t1000"
        /// </summary>
        public static TextAction? FromTabbedString(string tabbedString)
        {
            try
            {
                if (string.IsNullOrEmpty(tabbedString))
                {
                    Log.LogWarning("[TextAction] Empty tabbed string provided");
                    return null;
                }
                
                var parts = tabbedString.Split('\t');
                if (parts.Length < 1)
                {
                    Log.LogWarning("[TextAction] Invalid tabbed string format - no command found");
                    return null;
                }
                
                var command = parts[0];
                var arguments = parts.Skip(1).ToArray();
                
                Log.LogInfo($"[TextAction] Parsing: command='{command}', args=[{string.Join(", ", arguments)}]");
                
                return Create(command, arguments);
            }
            catch (Exception ex)
            {
                Log.LogError($"[TextAction] FromTabbedString failed: {ex.Message}");
                return null;
            }
        }
        
        // ==================== CONVENIENCE METHODS FOR COMMON COMMANDS ====================
        
        /// <summary>
        /// Create TextAction for adding resources to faction
        /// </summary>
        public static TextAction? CreateAddResource(string resourceName, int amount, float delay = 0f)
        {
            var textAction = Create("FactionAddResourceDistributed", resourceName, amount.ToString());
            if (textAction != null)
            {
                textAction.DaysDelay = delay;
            }
            return textAction;
        }
        
        /// <summary>
        /// Get native TextAction object for IL2CPP calls
        /// </summary>
        public object GetNativeTextActionObject() => GetNativeObject();
        
        /// <summary>
        /// Convert to tabbed string representation
        /// </summary>
        public override string ToString()
        {
            try
            {
                // Try native ToTabbedString method first
                var nativeToString = CallNative<string>("ToTabbedString");
                if (!string.IsNullOrEmpty(nativeToString))
                    return nativeToString;
                    
                // If native method fails, try fallback via field access
                var command = GetNativeField<string>("command") ?? "UnknownCommand";
                var arguments = GetNativeField<List<string>>("arguments") ?? new List<string>();
                return $"{command}\t{string.Join("\t", arguments)}";
            }
            catch (Exception ex)
            {
                Log.LogWarning($"[TextAction] ToString failed: {ex.Message}");
                // Ultimate fallback - basic representation
                return "TextAction[FieldAccessFailed]";
            }
        }
    }
}






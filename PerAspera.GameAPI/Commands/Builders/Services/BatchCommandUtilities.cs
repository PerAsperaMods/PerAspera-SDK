using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.Builders.Services
{
    /// <summary>
    /// Utilities for managing batch command collections and operations
    /// Handles cloning, manipulation, and collection management
    /// </summary>
    public class BatchCommandUtilities
    {
        /// <summary>
        /// Create a deep copy of command and condition lists
        /// </summary>
        public static (List<CommandBuilder> commands, List<Func<bool>> conditions) CloneCommandLists(
            IReadOnlyList<CommandBuilder> commands,
            IReadOnlyList<Func<bool>> conditions)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));

            var clonedCommands = new List<CommandBuilder>(commands);
            var clonedConditions = new List<Func<bool>>(conditions);

            return (clonedCommands, clonedConditions);
        }

        /// <summary>
        /// Validate that commands and conditions lists are synchronized
        /// </summary>
        public static void ValidateListsSynchronization(
            IReadOnlyList<CommandBuilder> commands,
            IReadOnlyList<Func<bool>> conditions)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            if (commands.Count != conditions.Count)
                throw new ArgumentException("Commands and conditions lists must have the same count");
        }

        /// <summary>
        /// Remove command at specified index from both lists
        /// </summary>
        public static void RemoveAt(
            List<CommandBuilder> commands,
            List<Func<bool>> conditions,
            int index)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            if (index < 0 || index >= commands.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            commands.RemoveAt(index);
            conditions.RemoveAt(index);
        }

        /// <summary>
        /// Insert command at specified index in both lists
        /// </summary>
        public static void InsertAt(
            List<CommandBuilder> commands,
            List<Func<bool>> conditions,
            int index,
            CommandBuilder command,
            Func<bool> condition = null)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (index < 0 || index > commands.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            commands.Insert(index, command);
            conditions.Insert(index, condition ?? (() => true));
        }

        /// <summary>
        /// Add command to both lists
        /// </summary>
        public static void AddCommand(
            List<CommandBuilder> commands,
            List<Func<bool>> conditions,
            CommandBuilder command,
            Func<bool> condition = null)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            if (command == null) throw new ArgumentNullException(nameof(command));

            commands.Add(command);
            conditions.Add(condition ?? (() => true));
        }

        /// <summary>
        /// Add multiple commands with default condition
        /// </summary>
        public static void AddCommands(
            List<CommandBuilder> commands,
            List<Func<bool>> conditions,
            IEnumerable<CommandBuilder> commandsToAdd)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            if (commandsToAdd == null) throw new ArgumentNullException(nameof(commandsToAdd));

            foreach (var command in commandsToAdd)
            {
                AddCommand(commands, conditions, command);
            }
        }

        /// <summary>
        /// Add multiple commands with specific conditions
        /// </summary>
        public static void AddCommandsWithConditions(
            List<CommandBuilder> commands,
            List<Func<bool>> conditions,
            IEnumerable<(CommandBuilder command, Func<bool> condition)> commandsWithConditions)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            if (commandsWithConditions == null) throw new ArgumentNullException(nameof(commandsWithConditions));

            foreach (var (command, condition) in commandsWithConditions)
            {
                AddCommand(commands, conditions, command, condition);
            }
        }

        /// <summary>
        /// Clear both command and condition lists
        /// </summary>
        public static void Clear(
            List<CommandBuilder> commands,
            List<Func<bool>> conditions)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));

            commands.Clear();
            conditions.Clear();
        }

        /// <summary>
        /// Combine conditional block logic with existing conditions
        /// </summary>
        public static Func<bool> CombineConditions(Func<bool> blockCondition, Func<bool> commandCondition)
        {
            if (blockCondition == null) throw new ArgumentNullException(nameof(blockCondition));
            if (commandCondition == null) throw new ArgumentNullException(nameof(commandCondition));

            return () => blockCondition() && commandCondition();
        }

        /// <summary>
        /// Add conditional block of commands
        /// </summary>
        public static void AddConditionalBlock(
            List<CommandBuilder> commands,
            List<Func<bool>> conditions,
            Func<bool> blockCondition,
            Action<List<CommandBuilder>, List<Func<bool>>> blockBuilder)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            if (blockCondition == null) throw new ArgumentNullException(nameof(blockCondition));
            if (blockBuilder == null) throw new ArgumentNullException(nameof(blockBuilder));

            var tempCommands = new List<CommandBuilder>();
            var tempConditions = new List<Func<bool>>();

            blockBuilder(tempCommands, tempConditions);

            // Combine block condition with individual command conditions
            for (int i = 0; i < tempCommands.Count; i++)
            {
                var combinedCondition = CombineConditions(blockCondition, tempConditions[i]);
                AddCommand(commands, conditions, tempCommands[i], combinedCondition);
            }
        }
    }
}

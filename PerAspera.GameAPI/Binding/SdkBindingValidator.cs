#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Binding
{
    /// <summary>
    /// Validates all remaining string-based SafeInvoke bindings in the SDK at startup.
    /// Writes sdk-contract-report.txt so game updates that rename methods are caught immediately.
    /// Call Validate() once on GameCommandsReady.
    /// </summary>
    public static class SdkBindingValidator
    {
        private static readonly LogAspera _log = new LogAspera("BindingValidator");

        private record BindingCheck(string TypeName, string MemberName, bool IsMethod, string CallerFile);

        private static readonly List<BindingCheck> _checks = new()
        {
            // DialogueWrapper.cs:33 — Universe.StartDialogue(params)
            new("Universe",          "StartDialogue",    IsMethod: true,  "DialogueWrapper.cs"),
            // DialogueWrapper.cs:56 — DialoguePresenter.NotifyDialogue(params)
            new("DialoguePresenter", "NotifyDialogue",   IsMethod: true,  "DialogueWrapper.cs"),
            // PerAsperaExtensions.cs:248 — InteractionManager.ExecuteCommand(string, object[])
            new("InteractionManager","ExecuteCommand",   IsMethod: true,  "PerAsperaExtensions.cs"),
        };

        /// <summary>
        /// Run all binding checks and write sdk-contract-report.txt.
        /// Returns true when every binding resolves successfully.
        /// </summary>
        public static bool Validate(string? outputDir = null)
        {
            outputDir ??= Path.Combine(Paths.BepInExRootPath, "Debug");
            Directory.CreateDirectory(outputDir);
            var reportPath = Path.Combine(outputDir, "sdk-contract-report.txt");

            var lines = new List<string>();
            lines.Add($"=== SDK CONTRACT REPORT — {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
            lines.Add($"Checking {_checks.Count} string-based bindings...");
            lines.Add("");

            var passed = 0;
            var failed = 0;

            foreach (var check in _checks)
            {
                var type = ReflectionHelpers.FindType(check.TypeName);
                if (type == null)
                {
                    lines.Add($"[FAIL] {check.TypeName}.{check.MemberName}  ← type not found  ({check.CallerFile})");
                    _log.Warning($"[CONTRACT] FAIL: type {check.TypeName} not found");
                    failed++;
                    continue;
                }

                var ok = check.IsMethod
                    ? ReflectionHelpers.HasMethod(type, check.MemberName)
                    : ReflectionHelpers.HasMember(type, check.MemberName);

                if (ok)
                {
                    lines.Add($"[OK  ] {check.TypeName}.{check.MemberName}  ({check.CallerFile})");
                    passed++;
                }
                else
                {
                    lines.Add($"[FAIL] {check.TypeName}.{check.MemberName}  ← member not found  ({check.CallerFile})");
                    _log.Warning($"[CONTRACT] FAIL: {check.TypeName}.{check.MemberName} not found");
                    failed++;
                }
            }

            lines.Add("");
            lines.Add($"=== RESULT: {passed} OK / {failed} FAILED ===");

            File.WriteAllLines(reportPath, lines);

            if (failed == 0)
                _log.Info($"[CONTRACT] All {passed} bindings OK → {reportPath}");
            else
                _log.Error($"[CONTRACT] {failed} binding(s) FAILED — see {reportPath}");

            return failed == 0;
        }
    }
}
#pragma warning restore CS1591

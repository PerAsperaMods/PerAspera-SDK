using System;
using System.Collections.Generic;
using PerAspera.Core.IL2CPP;
using BepInEx.Logging;

namespace PerAspera.Core.IL2CPP
{
    /// <summary>
    /// Testing utilities for IL2CPP collection conversion
    /// </summary>
    public static class CollectionConversionTests
    {
        private static readonly ManualLogSource _log = Logger.CreateLogSource("CollectionTests");

        /// <summary>
        /// Test the ConvertIl2CppList method with various collection types
        /// </summary>
        /// <param name="testCollection">Collection to test</param>
        /// <returns>Test success result</returns>
        public static bool TestCollectionConversion(object? testCollection)
        {
            if (testCollection == null)
            {
                _log.LogInfo("Testing with null collection - should return null");
                var nullResult = testCollection.ConvertIl2CppList<string>();
                return nullResult == null;
            }

            try
            {
                _log.LogInfo($"Testing collection of type: {testCollection.GetType().Name}");
                
                // Test string list conversion
                var stringResult = testCollection.ConvertIl2CppList<string>();
                _log.LogInfo($"String conversion result: {stringResult?.Count ?? 0} items");
                
                // Test object list conversion
                var objectResult = testCollection.ConvertIl2CppList<object>();
                _log.LogInfo($"Object conversion result: {objectResult?.Count ?? 0} items");
                
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError($"Collection conversion test failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test conversion with a managed List for reference
        /// </summary>
        public static void TestManagedListConversion()
        {
            _log.LogInfo("Testing managed list conversion...");
            
            var managedList = new List<string> { "test1", "test2", "test3" };
            var converted = managedList.ConvertIl2CppList<string>();
            
            _log.LogInfo($"Managed list: {managedList.Count} items, Converted: {converted?.Count ?? 0} items");
            
            if (converted != null && converted.Count == managedList.Count)
            {
                _log.LogInfo("✅ Managed list conversion successful");
            }
            else
            {
                _log.LogWarning("❌ Managed list conversion failed");
            }
        }
    }
}
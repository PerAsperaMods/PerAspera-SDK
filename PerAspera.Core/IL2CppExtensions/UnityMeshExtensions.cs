using System.Collections.Generic;
using UnityEngine;

namespace PerAspera.Core.IL2CPP
{
    /// <summary>
    /// Safe wrappers for UnityEngine.Mesh methods that are stripped in IL2CPP.
    ///
    /// Per Aspera's IL2CPP binary only preserves the List&lt;T&gt; overloads of Mesh APIs.
    /// The array property setters (mesh.vertices = array, mesh.uv = array) throw
    /// MissingMethodException at runtime — these helpers route managed collections
    /// through the IL2CPP overloads that survive stripping.
    ///
    /// Depuis 2026-06 (références interop seules, plus de unity-libs), la signature
    /// compile-time correspond au runtime : conversion managée → IL2CPP explicite ici.
    ///
    /// <example>
    /// // Instead of: mesh.vertices = new[] { ... }   ← MissingMethodException at runtime
    /// // Use:
    /// mesh.SetVerticesSafe(new[] { new Vector3(-r, 0f, -r), ... });
    /// mesh.SetTrianglesSafe(new[] { 0, 1, 2, 0, 2, 3 });
    /// mesh.SetUVsSafe(0, new[] { new Vector2(0f, 0f), ... });
    /// </example>
    /// </summary>
    public static class UnityMeshExtensions
    {
        /// <summary>
        /// Sets mesh vertices using the IL2CPP List overload preserved at runtime.
        /// Replaces mesh.vertices = array and SetVertices(Vector3[]), both stripped.
        /// </summary>
        public static void SetVerticesSafe(this Mesh mesh, IEnumerable<Vector3> vertices)
        {
            var list = new Il2CppSystem.Collections.Generic.List<Vector3>();
            foreach (var v in vertices) list.Add(v);
            mesh.SetVertices(list);
        }

        /// <summary>
        /// Sets mesh triangles using the IL2CPP array overload preserved at runtime.
        /// Replaces mesh.triangles = array, stripped at runtime.
        /// </summary>
        public static void SetTrianglesSafe(this Mesh mesh, IEnumerable<int> triangles, int submesh = 0)
        {
            // int[] managé → Il2CppStructArray<int> (conversion implicite Il2CppInterop)
            var array = triangles is int[] a ? a : new List<int>(triangles).ToArray();
            mesh.SetTriangles(array, submesh);
        }

        /// <summary>
        /// Sets mesh UVs using the IL2CPP List overload preserved at runtime.
        /// Replaces mesh.uv = array and SetUVs(int, Vector2[]), both stripped.
        /// </summary>
        public static void SetUVsSafe(this Mesh mesh, int channel, IEnumerable<Vector2> uvs)
        {
            var list = new Il2CppSystem.Collections.Generic.List<Vector2>();
            foreach (var uv in uvs) list.Add(uv);
            mesh.SetUVs(channel, list);
        }
    }
}

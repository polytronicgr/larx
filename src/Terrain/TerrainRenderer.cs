using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Larx.Terrain
{
    public class TerrainRenderer : TerrainPicker
    {
        private const int mapSize = 128;
        private int indexCount = 0;
        private int vertexBuffer;
        private int colorBuffer;
        private int indexBuffer;
        private int normalBuffer;
        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector3> colors = new List<Vector3>();
        private List<Vector3> normals = new List<Vector3>();
        private List<int> indices = new List<int>();
        private int triangleArray;

        public TerrainShader Shader { get; }

        public TerrainRenderer()
        {
            Shader = new TerrainShader();
            build();
        }

        public void Render(Camera camera)
        {
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            GL.UseProgram(Shader.Program);

            GL.Uniform3(Shader.Ambient, 0.5f, 0.5f, 0.5f);
            GL.Uniform3(Shader.Diffuse, 0.9f, 0.9f, 0.9f);
            GL.Uniform3(Shader.Specular, 1.0f, 1.0f, 1.0f);
            GL.Uniform1(Shader.Shininess, 50f);
            camera.ApplyCamera(Shader);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, colorBuffer);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, normalBuffer);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        public void ChangeElevation(float offset, float radius, float hardness, MousePicker picker)
        {
            var position = GetPosition(picker);
            var toUpdate = getTilesInArea(position, radius);

            foreach (var i in toUpdate)
            {
                if (i >= indices.Count) continue;

                var vertex = vertices[indices[(int)i]];
                var color = colors[indices[(int)i]];

                Func<float, float> calcP = (float t) => MathF.Pow(1f - t, 2) * MathF.Pow(1f + t, 2);

                var amount = Vector3.Distance(position, vertex);
                var elev = vertex.Y + calcP(MathF.Min(1f, MathF.Sqrt((amount / radius > hardness ? amount : 0.0f) / radius))) * offset;

                vertices[indices[(int)i]] = new Vector3(vertex.X, elev, vertex.Z);
                colors[indices[(int)i]] = new Vector3(0.27f, 0.34f, 0.05f);
            }

            updateNormals(position, radius);
            updateBuffers();
        }

        private void updateNormals(Vector3 center, float radius)
        {
            var toUpdate = getTilesInArea(center, radius);

            for (var i = 0; i < toUpdate.Count; i++)
            {
                var ci = toUpdate[i];
                if (ci < 0 || ci >= indices.Count) continue;
                updateNormal(ci);
            }
        }

        private void calculateNormals()
        {
            for (var i = 0; i < indices.Count; i += 3)
                updateNormal(i);

            updateBuffers();
        }

        private List<int> getTilesInArea(Vector3 center, float radius)
        {
            var included = new List<int>();

            for (var z = center.Z - radius; z <= center.Z + radius; z++)
                for (var x = center.X - radius; x <= center.X + radius; x++)
                {
                    var index = getTileIndex(new Vector3(x, 0, z));
                    if (index == null) continue;
                    included.Add((int)index);
                }

            return included;
        }

        private void updateNormal(int i)
        {
            var v1 = vertices[indices[i]];
            var v2 = vertices[indices[i + 1]];
            var v3 = vertices[indices[i + 2]];

            normals[indices[i]] += Vector3.Cross(v2 - v1, v3 - v1);
            normals[indices[i + 1]] += Vector3.Cross(v2 - v1, v3 - v1);
            normals[indices[i + 2]] += Vector3.Cross(v2 - v1, v3 - v1);

            normals[indices[i]].Normalize();
            normals[indices[i + 1]].Normalize();
            normals[indices[i + 2]].Normalize();
        }

        private void build()
        {
            triangleArray = GL.GenVertexArray();
            GL.BindVertexArray(triangleArray);

            var halfMapSize = (float)(mapSize / 2);
            var rnd = new Random();
            var i = 0;

            for (var z = -halfMapSize; z <= halfMapSize; z++)
            {
                for (var x = -halfMapSize; x <= halfMapSize; x++)
                {
                    vertices.Add(new Vector3(x, 0f, z));
                    colors.Add(new Vector3(0.27f, 0.34f, 0.05f));
                    normals.Add(new Vector3(0f, 1f, 0f));

                    if (x < halfMapSize && z < halfMapSize)
                    {
                        indices.AddRange(new int[] {
                            (i), (i + mapSize + 1), (i + 1),
                            (i + 1), (i + mapSize + 1), (i + mapSize + 2)
                        });
                    }

                    i++;
                }
            }

            indexCount = indices.Count;

            vertexBuffer = GL.GenBuffer();
            colorBuffer = GL.GenBuffer();
            indexBuffer = GL.GenBuffer();
            normalBuffer = GL.GenBuffer();

            calculateNormals();
            updateBuffers();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(int), indices.ToArray(), BufferUsageHint.StaticDraw);
        }

        private int? getTileIndex(Vector3 position)
        {
            var x = (int)Math.Round(position.X + (mapSize / 2));
            if (x < 0 || x > mapSize) return null;

            var z = (int)Math.Round(position.Z + (mapSize / 2));
            if (z < 0 || z > mapSize) return null;

            return ((z * mapSize) + x) * 6;
        }

        private void updateBuffers()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, vertices.Count * Vector3.SizeInBytes, vertices.ToArray(), BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, colorBuffer);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, colors.Count * Vector3.SizeInBytes, colors.ToArray(), BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, normalBuffer);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, normals.Count * Vector3.SizeInBytes, normals.ToArray(), BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
        }
    }
}
using OpenTK.Graphics.OpenGL;

namespace Larx.Assets
{
    public class AssetShader : Shader
    {
        public AssetShader() : base("asset") { }

        public int Position { get; private set; }
        public int Rotation { get; private set; }
        public int BaseColorTexture { get; private set; }
        public int NormalTexture { get; private set; }
        public int Roughness { get; private set; }
        public int RoughnessTexture { get; private set; }
        public int ClipPlane { get; private set; }
        public int FogColor { get; private set; }
        public int FarPlane { get; private set; }

        protected override void SetUniformsLocations()
        {
            base.SetDefaultUniformLocations();
            base.SetShadowUniformLocations();

            Position = GL.GetUniformLocation(Program, "uPosition");
            Rotation = GL.GetUniformLocation(Program, "uRotation");
            BaseColorTexture = GL.GetUniformLocation(Program, "uBaseColorTexture");
            NormalTexture = GL.GetUniformLocation(Program, "uNormalTexture");
            Roughness = GL.GetUniformLocation(Program, "uRoughness");
            RoughnessTexture = GL.GetUniformLocation(Program, "uRoughnessTexture");
            ClipPlane = GL.GetUniformLocation(Program, "uClipPlane");
            FogColor = GL.GetUniformLocation(Program, "uFogColor");
            FarPlane = GL.GetUniformLocation(Program, "uFarPlane");
        }
    }
}
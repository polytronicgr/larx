using OpenTK.Graphics.OpenGL;

namespace Larx
{
    public class Multisampling
    {
        private int samples;

        private int texture;
        private int framebuffer;
        private int depthBuffer;
        private int colorBuffer;

        public Multisampling(int samples)
        {
            this.samples = samples;

            GL.Enable(EnableCap.Multisample);

            framebuffer = GL.GenFramebuffer();
            texture = GL.GenTexture();
            colorBuffer = GL.GenRenderbuffer();
            depthBuffer = GL.GenRenderbuffer();
        }

        private void createFramebuffer()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
        }

        private void createMultisampleTexture()
        {
            GL.BindTexture(TextureTarget.Texture2DMultisample, texture);
            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, samples, PixelInternalFormat.Rgb, State.Window.Size.Width, State.Window.Size.Height, true);
            GL.BindTexture(TextureTarget.Texture2DMultisample, 0);
        }

        private void createColorBuffer()
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, colorBuffer);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, samples, RenderbufferStorage.Rgba8, State.Window.Size.Width, State.Window.Size.Height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, colorBuffer);
        }

        private void createDepthBuffer()
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuffer);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, samples, RenderbufferStorage.DepthComponent, State.Window.Size.Width, State.Window.Size.Height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);
        }

        public void RefreshBuffers()
        {
            createMultisampleTexture();
            createFramebuffer();
            createColorBuffer();
            createDepthBuffer();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
        }

        public void Draw()
        {
            GL.Disable(EnableCap.DepthTest);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, framebuffer);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.BlitFramebuffer(0, 0, State.Window.Size.Width, State.Window.Size.Height, 0, 0, State.Window.Size.Width, State.Window.Size.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            GL.Enable(EnableCap.DepthTest);
        }
    }
}
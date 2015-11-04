using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace MonoGameTraining
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private bool useCustomShader = true;

        private GraphicsDeviceManager graphics;

        Matrix projectionMatrix;
        Matrix worldMatrix;

        //Camera
        public CameraController Camera;
        //Lantern model
        Model model;
        //Terrain
        Effect effect;
        MeshGrid terrain;
        VertexBuffer vertexBuffer;
        VertexPositionColorNormal[] vertexArray;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            terrain = new MeshGrid(40, 40, 1);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();

            //Setup Camera
            Camera = new CameraController(new Vector3(0f, 7f, -15f));
            Camera.ProcessInput(0f);

            //Setup other matrices
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), graphics.GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000f);
            worldMatrix = Matrix.Identity;

            //Fill buffer with terrain vertices
            terrain.RecalculateNormals();
            vertexArray = terrain.TriangleVerticesList;
            vertexBuffer = new VertexBuffer(graphics.GraphicsDevice, typeof(VertexPositionColorNormal), vertexArray.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertexArray);
            
        }
        
        protected override void LoadContent()
        {
            if (useCustomShader)
                effect = Content.Load<Effect>("NewPointLight");
            else
                effect = Content.Load<Effect>("effects");
            model = Content.Load<Model>("Lantern");
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();
        }

        protected override void UnloadContent() { }

        protected override void Update(GameTime gameTime)
        {
            //Update Camera position and angle
            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            Camera.ProcessInput(timeDifference);
            terrain.RecalculateNormals();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Terrain
            graphics.GraphicsDevice.SetVertexBuffer(vertexBuffer);
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            //Shader settings
            if (useCustomShader)
                SetupCustomShader(effect);
            else
            {
                effect.CurrentTechnique = effect.Techniques["Colored"];
                effect.Parameters["xEnableLighting"].SetValue(true);
                effect.Parameters["xLightDirection"].SetValue(new Vector3(0,1,0));
            }
            effect.Parameters["xWorld"].SetValue(worldMatrix);
            effect.Parameters["xView"].SetValue(Camera.ViewMatrix);
            effect.Parameters["xProjection"].SetValue(projectionMatrix);

            //Rendering
            //  terrain
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                //GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, terrain.SizeX * terrain.SizeZ * 2);
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, terrain.TriangleVerticesList, 0, terrain.SizeX * terrain.SizeZ * 2, VertexPositionColorNormal.VertexDeclaration);
            }

            //  Lantern1
            DrawModel(model, new Vector3(0, 0, 0), "SinglePointLight");
            //  Lantern2
            DrawModel(model, new Vector3(9, 0, 5), "SinglePointLight");

            base.Draw(gameTime);
        }
        private void SetupCustomShader(Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques["SinglePointLight"];
            effect.Parameters["xCameraPosition"].SetValue(new Vector4(Camera.CameraPosition, 1));
            effect.Parameters["AmbientColor"].SetValue(new Vector4(0.2f, 0.2f, 0.2f,1f));
            effect.Parameters["L1Position"].SetValue(new Vector4(0, 15, 0, 1));
            effect.Parameters["L1Range"].SetValue(100.0f);
            effect.Parameters["L1DColor"].SetValue(new Vector3(1f));
            effect.Parameters["L1SColor"].SetValue(new Vector4(1,1,1,200));
        }
        private void DrawModel(Model model, Vector3 modelPosition, string technique )
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques[technique];
                    effect.Parameters["xWorld"].SetValue(Matrix.CreateTranslation(modelPosition));
                    effect.Parameters["xView"].SetValue(Camera.ViewMatrix);
                    effect.Parameters["xProjection"].SetValue(projectionMatrix);
                    SetupCustomShader(effect);
                }
                mesh.Draw();
            }
        }
    }
}

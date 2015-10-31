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
        private GraphicsDeviceManager graphics;
        //private SpriteBatch spriteBatch;

        //Camera
        public CameraController Camera;

        Matrix projectionMatrix;
        Matrix worldMatrix;

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
            terrain = new MeshGrid(4, 4, 15);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            //Setup Camera
            Camera = new CameraController(new Vector3(0f, 0f, -15f));
            Camera.ProcessInput(0f);

            //Setup other matrices
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), graphics.GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000f);
            worldMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);

            //Fill buffer with terrain vertices
            vertexArray = terrain.triangleVerticesList();
            vertexBuffer = new VertexBuffer(graphics.GraphicsDevice, typeof(VertexPositionColorNormal), vertexArray.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertexArray);
            
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            effect = Content.Load<Effect>("effects");
            model = Content.Load<Model>("Lantern");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //Update Camera position and angle
            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            Camera.ProcessInput(timeDifference);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
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
            effect.CurrentTechnique = effect.Techniques["ColoredNoShading"];
            effect.Parameters["xView"].SetValue(Camera.ViewMatrix);
            effect.Parameters["xProjection"].SetValue(projectionMatrix);
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);

            //Rendering
            //  terrain
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                //GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, terrain.SizeX * terrain.SizeZ * 2);
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, terrain.triangleVerticesList(), 0, terrain.SizeX * terrain.SizeZ * 2, VertexPositionColorNormal.VertexDeclaration);
            }

            //  Lantern1
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();

                    effect.AmbientLightColor = new Vector3(1f, 0, 0);
                    effect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
                    effect.View = Camera.ViewMatrix;
                    effect.World = worldMatrix;
                    effect.Projection = projectionMatrix;
                }
                mesh.Draw();
            }

            //  Lantern2
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.AmbientLightColor = new Vector3(1f, 0, 0);
                    effect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
                    effect.View = Camera.ViewMatrix;
                    effect.World = worldMatrix * Matrix.CreateTranslation(9, 0, 5);
                    effect.Projection = projectionMatrix;
                }
                mesh.Draw();
            }

            base.Draw(gameTime);
        }
    }
}

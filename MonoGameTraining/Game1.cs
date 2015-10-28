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
        Vector3 camTarget;
        Vector3 camPosition;
        Matrix projectionMatrix;
        Matrix viewMatrix;
        Matrix worldMatrix;

        //Lantern model
        Model model;

        //Terrain
        BasicEffect basicEffect;
        MeshGrid terrain;
        VertexBuffer vertexBuffer;
        VertexPositionColor[] vertexArray;
        

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
            camTarget = new Vector3(0f, 0f, 0f);
            camPosition = new Vector3(0f, 0f, -15f);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), graphics.GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000f);
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget, new Vector3(0f, 1f, 0f));
            worldMatrix = Matrix.CreateWorld(camTarget, Vector3.Forward, Vector3.Up);

            //Terrain setup
            vertexArray = terrain.triangleVerticesList().Select(v => new VertexPositionColor(v.Position,v.Color)).ToArray();
            vertexBuffer = new VertexBuffer(graphics.GraphicsDevice, typeof(VertexPositionColor), vertexArray.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertexArray);

            //BasicEffect
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.Alpha = 1f;
            basicEffect.VertexColorEnabled = true;
            basicEffect.LightingEnabled = true;
            basicEffect.EnableDefaultLighting();

            basicEffect.AmbientLightColor = new Vector3(0.1f, 0.1f, 0f);
            basicEffect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);

            basicEffect.DirectionalLight0.Enabled = true;
            basicEffect.DirectionalLight0.DiffuseColor = new Vector3(0.5f, 1f, 0.5f); // a red light
            basicEffect.DirectionalLight0.Direction = new Vector3(0, -1, 0);  // coming along the _axis
            basicEffect.DirectionalLight0.SpecularColor = new Vector3(1, 0, 0); // with green highlights


        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back ==
               ButtonState.Pressed || Keyboard.GetState().IsKeyDown(
               Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                camPosition.X -= 0.1f;
                camTarget.X -= 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                camPosition.X += 0.1f;
                camTarget.X += 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                camPosition.Y -= 0.1f;
                camTarget.Y -= 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                camPosition.Y += 0.1f;
                camTarget.Y += 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                camPosition.Z += 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            {
                camPosition.Z -= 0.1f;
            }
            
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
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

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                //GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, terrain.SizeX * terrain.SizeZ * 2);
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, terrain.triangleVerticesList(), 0, terrain.SizeX * terrain.SizeZ * 2);
            }

            ////Lantern1
            //foreach (ModelMesh mesh in model.Meshes)
            //{
            //    foreach (BasicEffect effect in mesh.Effects)
            //    {
            //        effect.EnableDefaultLighting();
            //        effect.AmbientLightColor = new Vector3(1f, 0, 0);
            //        effect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
            //        effect.View = viewMatrix;
            //        effect.World = worldMatrix;
            //        effect.Projection = projectionMatrix;
            //    }
            //    mesh.Draw();
            //}

            ////Lantern2
            //foreach (ModelMesh mesh in model.Meshes)
            //{
            //    foreach (BasicEffect effect in mesh.Effects)
            //    {
            //        effect.EnableDefaultLighting();
            //        effect.AmbientLightColor = new Vector3(1f, 0, 0);
            //        effect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
            //        effect.View = viewMatrix;
            //        effect.World = worldMatrix * Matrix.CreateTranslation(9, 0, 5);
            //        effect.Projection = projectionMatrix;
            //    }
            //    mesh.Draw();
            //}

            base.Draw(gameTime);
        }
    }
}

﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace MonoGameTraining
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;

        Matrix projectionMatrix;
        Matrix worldMatrix;

        //Camera
        public CameraController Camera;
        //Lantern model
        Vector3 lantern1Position = new Vector3(5, 0, 10);
        Vector3 lantern2Position = new Vector3(20, 0, 25);
        Model lanternModel;
        Model monkeyModel;
        //Terrain
        Effect effect;
        MeshGrid terrain;
        TexturedCube cube;
        Skybox skybox;
        VertexBuffer vertexBuffer;
        VertexPositionNormalTexture[] vertexArray;
        //Lights
        public PointLight Light1, Light2;
        //Textures
        public int GrassTextureIndex = 0;
        private Texture2D[] grassTextures = new Texture2D[2];
        private Texture2D sidewalkTexture, gridSkybox, symbolTexture;
        public TextureFilter TexFilter = TextureFilter.Linear;
        public float MipMapLevelBias = 0;
        //Fog
        private float fogIntensity = 0.7f;
        public float FogIntensity { get { return fogIntensity; } set { fogIntensity = value; } }
        private float fogStart = 10;
        public float FogStart { get { return fogStart; } set { fogStart = value; } }
        private float fogEnd = 100;
        public float FogEnd { get { return fogEnd; } set { fogEnd = value; } }




        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            cube = new TexturedCube(1000);
            terrain = new MeshGrid(200, 200, 1);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
            IsMouseVisible = true;
            //Setup Camera
            Camera = new CameraController(new Vector3(0f, 7f, -15f));
            Camera.ProcessInput(0f);

            //Setup other matrices
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), graphics.GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000f);
            worldMatrix = Matrix.Identity;

            //Fill buffer with terrain vertices
            terrain.RecalculateNormals();

            var tmpList = new List<VertexPositionNormalTexture>();
            tmpList.AddRange(terrain.TriangleVerticesList);
            //tmpList.AddRange(cube.TriangleVerticesList);

            vertexArray = tmpList.ToArray();
            vertexBuffer = new VertexBuffer(graphics.GraphicsDevice, typeof(VertexPositionNormalTexture), vertexArray.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertexArray);

            Light1 = new PointLight()
            {
                IsOn = false,
                Position = lantern1Position + new Vector3(0, 10, 0),
                Range = 35,
                DiffuseColor = Color.LightYellow,
                SpecularColor = Color.YellowGreen,
                SpecularPower = 200
            };

            Light2 = new PointLight()
            {
                IsOn = true,
                Position = lantern2Position + new Vector3(0, 10, 0),
                Range = 35,
                DiffuseColor = Color.Yellow,
                SpecularColor = Color.MediumVioletRed,
                SpecularPower = 200
            };

        }
                
        protected override void LoadContent()
        {
            //skybox = new Skybox("SkyBox", Content);
            effect = Content.Load<Effect>("Shaders");
            lanternModel = Content.Load<Model>("Lantern");
            monkeyModel = Content.Load<Model>("monkey");

            grassTextures[0] = Content.Load<Texture2D>("grass");
            grassTextures[1] = Content.Load<Texture2D>("lava");
            sidewalkTexture = Content.Load<Texture2D>("road");
            gridSkybox = Content.Load<Texture2D>("gridSkybox");
            symbolTexture = Content.Load<Texture2D>("batman-logo");

            foreach (ModelMesh mesh in lanternModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();
            foreach (ModelMesh mesh in monkeyModel.Meshes)
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
            SetupCustomShader(effect, new Vector3(0, 0, 0));
            SetupTextureShader(effect, "DoubleTextured", grassTextures[GrassTextureIndex], sidewalkTexture);
            SetupPointLightShader(effect, "TextureLightFog");
            //Rendering
            //  terrain
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, terrain.SizeX * terrain.SizeZ * 2);
                //graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, terrain.TriangleVerticesList, 0, terrain.SizeX * terrain.SizeZ * 2, VertexPositionNormalTexture.VertexDeclaration);
            }

            SetupCustomShader(effect, new Vector3(cube.Size/(-2)));
            SetupTextureShader(effect, "TextureFog", gridSkybox);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                //GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 14);
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, cube.TriangleVerticesList, 0, 12, VertexPositionNormalTexture.VertexDeclaration);
            }


            //  Lantern1
            DrawModel(lanternModel, lantern1Position, "PointLightsOnly");
            //  Lantern2
            DrawModel(lanternModel, lantern2Position, "PointLightsOnly");
            // 
            DrawModel(monkeyModel, new Vector3(30, 10, 2), "PointLightsOnly");

            base.Draw(gameTime);
        }

        private void SetupCustomShader(Effect effect, Vector3 translation)
        {
            effect.Parameters["xWorld"].SetValue(Matrix.CreateTranslation(translation));
            effect.Parameters["xView"].SetValue(Camera.ViewMatrix);
            effect.Parameters["xProjection"].SetValue(projectionMatrix);
            effect.Parameters["xCameraPosition"].SetValue(new Vector4(Camera.CameraPosition, 1));
            effect.Parameters["fogColor"].SetValue(new Vector4(1f,1f,1f,FogIntensity));
            effect.Parameters["fogStart"].SetValue(this.FogStart);
            effect.Parameters["fogEnd"].SetValue(this.FogEnd);
        }

        private void SetupTextureShader(Effect effect, string techniqueName, Texture2D tex1, Texture2D tex2 = null)
        {
            effect.CurrentTechnique = effect.Techniques[techniqueName];
            effect.Parameters["tex1"].SetValue(tex1);
            effect.Parameters["Light3Tex"].SetValue(symbolTexture);
            effect.GraphicsDevice.SamplerStates[1] = new SamplerState() { AddressU = TextureAddressMode.Mirror, AddressV = TextureAddressMode.Mirror, Filter = TexFilter, MipMapLevelOfDetailBias = MipMapLevelBias };
            effect.GraphicsDevice.SamplerStates[2] = new SamplerState() { AddressU = TextureAddressMode.Mirror, AddressV = TextureAddressMode.Mirror, Filter = TextureFilter.Linear };
            if (techniqueName == "DoubleTextured")
                effect.Parameters["tex2"].SetValue(tex2);
        }

        private void SetupPointLightShader(Effect effect, string techniqueName)
        {
            effect.CurrentTechnique = effect.Techniques[techniqueName];
            effect.Parameters["AmbientColor"].SetValue(new Vector4(0.5f, 0.5f, 0.5f,1f));
            Light1.SetEffectParameters(effect, 1);
            Light2.SetEffectParameters(effect, 2);
        }
        private void DrawModel(Model model, Vector3 modelPosition, string technique )
        {
            if (model == null)
                return;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques[technique];
                    SetupCustomShader(effect, modelPosition);
                    SetupPointLightShader(effect, technique);
                }
                mesh.Draw();
            }
        }
    }
}

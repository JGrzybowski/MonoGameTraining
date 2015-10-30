using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTraining
{
    public class MeshGrid
    {
        public VertexPositionColorNormal[,] Vertices;
        public Tuple<VertexPositionColorNormal, VertexPositionColorNormal, VertexPositionColorNormal>[] Triangles;
        public BasicEffect basicEffect;
        //private GraphicsDevice device;

        private int sizeX;
        public int SizeX {get {return sizeX;} }
        private int sizeZ;
        public int SizeZ {get {return sizeZ;} }
        
        public MeshGrid(int sizeX, int sizeZ, float squareSideSize)
        {
            //this.device = device;
            //basicEffect = new BasicEffect(device);
            this.sizeX = sizeX;
            this.sizeZ = sizeZ;
            this.Vertices = new VertexPositionColorNormal[sizeX + 1, sizeZ + 1];
            this.Triangles = new Tuple<VertexPositionColorNormal, VertexPositionColorNormal, VertexPositionColorNormal>[sizeX*sizeZ*2];
            var rand = new Random();

            for (int x = 0; x < sizeX+1; x++)
                for (int z = 0; z < sizeZ+1; z++)
                    this.Vertices[x, z] = new VertexPositionColorNormal(new Vector3(squareSideSize*x, 0, squareSideSize*z), new Color(0, 0.5f, 0.2f), Vector3.Up);

            //for(int i=0; i<20; i++) { Vertices[rand.Next(SizeX), rand.Next(SizeZ)].Position.Y += 1; }

            for (int x = 0; x < sizeX; x++)
                for (int z = 0; z < sizeZ; z++)
                {
                    Triangles[x*2*sizeZ + z] = new Tuple<VertexPositionColorNormal, VertexPositionColorNormal, VertexPositionColorNormal>(Vertices[x,z], Vertices[x,z+1], Vertices[x+1,z]);
                    Triangles[x*2*sizeZ + sizeZ + z] = new Tuple<VertexPositionColorNormal, VertexPositionColorNormal, VertexPositionColorNormal>(Vertices[x,z+1], Vertices[x+1,z], Vertices[x+1,z+1]);
                } 
        }

        public VertexPositionColorNormal this[int x, int z]
        {
            get { return Vertices[x, z]; }
            set { Vertices[x, z] = value; }
        }

        public VertexPositionColorNormal[] triangleVerticesList()
        {
            List<VertexPositionColorNormal> list = new List<VertexPositionColorNormal>();
            foreach(var triangle in Triangles)
            {
                list.Add(triangle.Item1);
                list.Add(triangle.Item2);
                list.Add(triangle.Item3);
            }
            return list.ToArray();
        }

        public void Draw(VertexBuffer buffer)
        {
            
        }
    }
}

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
        public class Triangle
        {
            public Point Index1 { get; set; }
            public Point Index2 { get; set; }
            public Point Index3 { get; set; }

            public Triangle(int x1, int y1, int x2, int y2, int x3, int y3)
            {
                Index1 = new Point(x1, y1);
                Index2 = new Point(x2, y2);
                Index3 = new Point(x3, y3);
            }
            public override string ToString()
            {
                return string.Format("{0} {1} {2}", Index1, Index2, Index3);
            }
        }

        public VertexPositionNormalTexture[,] Vertices;
        public Triangle[] Triangles;
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
            this.Vertices = new VertexPositionNormalTexture[sizeX + 1, sizeZ + 1];
            this.Triangles = new Triangle[sizeX*sizeZ*2];
            var rand = new Random();

            int texParam = 2;

            for (int x = 0; x < sizeX+1; x++)
                for (int z = 0; z < sizeZ+1; z++)
                    this.Vertices[x, z] = new VertexPositionNormalTexture(new Vector3(squareSideSize*x, 0, squareSideSize*z), Vector3.Up, new Vector2(texParam * (float)x/(float)SizeX,texParam * (float)z/(float)SizeZ));

            //for(int i=0; i<20; i++) { Vertices[rand.Next(SizeX), rand.Next(SizeZ)].Position.Y += 1; }

            for (int x = 0; x < sizeX; x++)
                for (int z = 0; z < sizeZ; z++)
                {
                    Triangles[x * 2 * sizeZ + z] = new Triangle(x, z, x, z + 1, x + 1, z);
                    Triangles[x * 2 * sizeZ + sizeZ + z] = new Triangle(x, z + 1, x + 1, z, x + 1, z + 1);
                } 
        }

        public VertexPositionNormalTexture this[int x, int z]
        {
            get { return Vertices[x, z]; }
            set { Vertices[x, z] = value; }
        }

        public void RecalculateNormals()
        {
            for (int i = 0; i < SizeX; i++)
                for (int j = 0; j < SizeZ; j++)
                    Vertices[i, j].Normal = Vector3.Up;
                        //Vertices[i,j].Normal = Vector3.Zero;

           // foreach (var triangle in Triangles)
           // {
           //     RecalculateTriangleNormals(triangle);
           // }

           //for (int i = 0; i < SizeX; i++)
           //     for (int j = 0; j < SizeZ; j++)
           //         Vertices[i,j].Normal.Normalize();
        }

        private void RecalculateTriangleNormals(Triangle triangle)
        {
            Vector3 side1 = Vertices[triangle.Index1.X, triangle.Index1.Y].Position - Vertices[triangle.Index3.X, triangle.Index3.Y].Position;
            Vector3 side2 = Vertices[triangle.Index1.X, triangle.Index1.Y].Position - Vertices[triangle.Index2.X, triangle.Index2.Y].Position;
            Vector3 normal = Vector3.Cross(side1, side2);

            Vertices[triangle.Index1.X, triangle.Index1.Y].Normal += normal;
            Vertices[triangle.Index2.X, triangle.Index2.Y].Normal += normal;
            Vertices[triangle.Index3.X, triangle.Index3.Y].Normal += normal;
        }


        public VertexPositionNormalTexture[] TriangleVerticesList
        {
            get
            {
                List<VertexPositionNormalTexture> list = new List<VertexPositionNormalTexture>();
                foreach (var triangle in Triangles)
                {
                    list.Add(Vertices[triangle.Index1.X, triangle.Index1.Y]);
                    list.Add(Vertices[triangle.Index2.X, triangle.Index2.Y]);
                    list.Add(Vertices[triangle.Index3.X, triangle.Index3.Y]);
                }
                return list.ToArray();
            }
        }

    }
}

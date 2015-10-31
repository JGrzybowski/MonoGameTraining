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
        }

        public VertexPositionColorNormal[,] Vertices;
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
            this.Vertices = new VertexPositionColorNormal[sizeX + 1, sizeZ + 1];
            this.Triangles = new Triangle[sizeX*sizeZ*2];
            var rand = new Random();

            for (int x = 0; x < sizeX+1; x++)
                for (int z = 0; z < sizeZ+1; z++)
                    this.Vertices[x, z] = new VertexPositionColorNormal(new Vector3(squareSideSize*x, 0, squareSideSize*z), new Color(0, 0.5f, 0.2f), Vector3.Up);

            //for(int i=0; i<20; i++) { Vertices[rand.Next(SizeX), rand.Next(SizeZ)].Position.Y += 1; }

            for (int x = 0; x < sizeX; x++)
                for (int z = 0; z < sizeZ; z++)
                {
                    Triangles[x * 2 * sizeZ + z] = new Triangle(x, z, x, z + 1, x + 1, z);
                    Triangles[x * 2 * sizeZ + sizeZ + z] = new Triangle(x, z + 1, x + 1, z, x + 1, z + 1);
                } 
        }

        public VertexPositionColorNormal this[int x, int z]
        {
            get { return Vertices[x, z]; }
            set { Vertices[x, z] = value; }
        }

        //private VertexPositionColorNormal[] CalculateNormals(VertexPositionColorNormal[] vertices, int[] indices)
        //{
        //    for (int i = 0; i < vertices.Length; i++)
        //        vertices[i].Normal = Vector3.Zero;

        //    for (int i = 0; i < Triangles.Length; i++)
        //    {
        //        var triangle = Triangles[i];

        //        Vector3 side1 = triangle.Vertex1.Position - triangle.Vertex3.Position;
        //        Vector3 side2 = triangle.Vertex1.Position - triangle.Vertex2.Position;
        //        Vector3 normal = Vector3.Cross(side1, side2);

        //        triangle.Vertex1.Normal += normal;
        //        triangle.Vertex2.Normal += normal;
        //        triangle.Vertex3.Normal += normal;
        //    }

        //    for (int i = 0; i < indices.Length / 3; i++)
        //    {
        //        int index1 = indices[i * 3];
        //        int index2 = indices[i * 3 + 1];
        //        int index3 = indices[i * 3 + 2];

        //        Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
        //        Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
        //        Vector3 normal = Vector3.Cross(side1, side2);

        //        vertices[index1].Normal += normal;
        //        vertices[index2].Normal += normal;
        //        vertices[index3].Normal += normal;
        //    }

        //    for (int i = 0; i < vertices.Length; i++)
        //        vertices[i].Normal.Normalize();

        //    return vertices;
        //}

        public VertexPositionColorNormal[] triangleVerticesList()
        {
            List<VertexPositionColorNormal> list = new List<VertexPositionColorNormal>();
            foreach(var triangle in Triangles)
            {
                list.Add(Vertices[triangle.Index1.X,triangle.Index1.Y]);
                list.Add(Vertices[triangle.Index2.X,triangle.Index2.Y]);
                list.Add(Vertices[triangle.Index3.X,triangle.Index3.Y]);
            }
            return list.ToArray();
        }

    }
}

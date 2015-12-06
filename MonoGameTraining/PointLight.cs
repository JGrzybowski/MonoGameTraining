using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTraining
{
    public class PointLight
    {
        public Vector3 Position{ get; set; }
        public float Range { get; set; }
        public Color DiffuseColor { get; set; }
        public Color SpecularColor { get; set; }
        public int SpecularPower { get; set; }
        public bool IsOn { get; set; }

        public void SetEffectParameters(Effect effect, int index)
        {
            effect.Parameters["L" + index + "On"].SetValue(IsOn);
            effect.Parameters["L" + index + "Position"].SetValue(new Vector4(Position, 1));
            effect.Parameters["L" + index + "Range"].SetValue(Range);
            effect.Parameters["L" + index + "DColor"].SetValue(DiffuseColor.ToVector3());
            effect.Parameters["L" + index + "SColor"].SetValue(new Vector4(SpecularColor.ToVector3(), 200));
        }
    }
}

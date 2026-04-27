using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Files.Utils;
using UnityEngine;

namespace FSO.Vitaboy
{
    /// <summary>
    /// Compressed Floating Point
    /// 
    /// Used to contain the animation and translation vectors for animations
    /// separately from their "head" information. This makes it easier for the
    /// TS1 content system to scan them without loading all of the animation data.
    /// </summary>
    public class CFP
    {
        public static float[] Delta = Enumerable.Range(0, 256).Select(x => (float)(3.9676e-10 * (Math.Pow((double)x - 126, 3) * Math.Abs(x - 126)))).ToArray();

        public byte[] Data;

        public void Read(Stream stream)
        {
            using (var dest = new MemoryStream())
            {
                stream.CopyTo(dest);
                Data = dest.ToArray();
            }
        }

        public void EnrichAnim(Animation anim)
        {
            using (var stream = new MemoryStream(Data))
            {
                using (var io = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN))
                {

                    anim.Translations = new Vector3[anim.TranslationCount];
                    anim.Rotations = new Quaternion[anim.RotationCount];
                    //read the floating point values and give them to the anim.
                    //the anim supplies us with the count for the translation and rotation vector arrays.
                    ReadNFloats(io, anim.Translations.Count(), (i, x) => anim.Translations[i].x = -x);
                    ReadNFloats(io, anim.Translations.Count(), (i, y) => anim.Translations[i].y = y);
                    ReadNFloats(io, anim.Translations.Count(), (i, z) => anim.Translations[i].z = z);

                    ReadNFloats(io, anim.Rotations.Count(), (i, x) => anim.Rotations[i].x = x);
                    ReadNFloats(io, anim.Rotations.Count(), (i, y) => anim.Rotations[i].y = -y);
                    ReadNFloats(io, anim.Rotations.Count(), (i, z) => anim.Rotations[i].z = -z);
                    ReadNFloats(io, anim.Rotations.Count(), (i, w) => anim.Rotations[i].w = -w);
                }
            }
            //Data = null;
        }

        public static void ReadNFloats(IoBuffer io, int floats, Action<int, float> output)
        {
            float lastValue = 0;
            for (int i=0; i<floats; i++)
            {
                var code = io.ReadByte();
                switch (code)
                {
                    case 0xFF:
                        lastValue = io.ReadFloat();
                        break;
                    case 0xFE:
                        //repeat count
                        var repeats = io.ReadUInt16();
                        for (int j=0; j<repeats; j++)
                        {
                            output(i++, lastValue);
                        }
                        break;
                    default:
                        lastValue += Delta[code];
                        break;
                }

                output(i, lastValue);
            }
        }
    }
}

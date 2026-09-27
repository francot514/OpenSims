using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;	
using System.IO;
using FSO.Files.XA;
using UnityEngine;

namespace TSO.HIT
{
    public class AmbiencePlayer
    {
        private bool fscMode;
        private FSCPlayer fsc;
        private AudioClip sfx;
        private AudioSource inst;

        public AmbiencePlayer(Ambience amb)
        {
            if (amb.Loop)
            {
                byte[] data = new XAFile(FSO.Content.Content.Get().GetPath(amb.Path)).DecompressedData;
                var stream = new MemoryStream(data);
                sfx = AudioClip.Create("", 32, 32, 128, false);
                stream.Close();

                inst = new AudioSource();
                inst.loop = true;
                inst.Play();
                fscMode = false;
            }
            else
            {
                fsc = HITVM.Get().PlayFSC(FSO.Content.Content.Get().GetPath(amb.Path));
                fsc.SetVolume(0.33f); //may need tweaking
                fscMode = true;
            }
        }

        public void Kill()
        {
            if (fscMode) HITVM.Get().StopFSC(fsc);
            else
            {
                inst.Stop();
                
            }
        }
    }

    public struct Ambience
    {
        public string Path;
        public bool Loop; //certain ambiences are simple xa loops instead of fscs.

        public Ambience(string path, bool loop)
        {
            Path = path;
            Loop = loop;
        }
    }
}

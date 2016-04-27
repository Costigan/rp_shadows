using System;
using System.Collections.Generic;
using Shadow.spice;
using OpenTK;

namespace Shadow.viz
{
    public class World
    {
        public readonly List<Shape> FarShapes = new List<Shape>();
        public readonly List<Shape> NearShapes = new List<Shape>();
        public readonly List<OpenGLControlWrapper> Wrappers = new List<OpenGLControlWrapper>();
        private long _time;

        public Ball Sun;
        public Ball CentralBall;
        public TerrainPatch Patch;

        public long Time
        {
            get { return _time; }
            set { Update(value); }
        }

        public void Tick()
        {
            foreach (var w in Wrappers)
            {
                w.CameraMode.Tick();
                w.Invalidate();
            }
        }

        public void Update(long t)
        {
            _time = t;
            Tick();
        }
    }
}
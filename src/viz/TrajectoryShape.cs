using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using MBLib.Spice;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using ITOSLIB;

namespace LadeeViz.Viz
{
    public class TrajectoryShape : Shape
    {
        public enum TrajectoryTimeMode
        {
            StartStop,
            LeadTrail
        }

        public const long TrajectoryStep = 65536L*60*1; // 1 minute

        /*
        public static DateTime StartDate = new DateTime(2013, 9, 7, 4, 10, 0); // add 10 minutes buffer for clock error
        public static long StartTimestamp = TimeUtilities.DateTimeToTime42(StartDate);
        public static DateTime StopDate = new DateTime(2014, 4, 18, 4, 32, 55); // 2014 APR 18 04:32:54.211
        public static long StopTimestamp = TimeUtilities.DateTimeToTime42(StopDate);
        */

        public static long StartTimestamp = MBLib.MBLibData.FirstTimestamp;
        public static long StopTimestamp = MBLib.MBLibData.LastTimestamp;
        public static DateTime StartDate = TimeUtilities.Time42ToDateTime(StartTimestamp);
        public static DateTime StopDate = TimeUtilities.Time42ToDateTime(StopTimestamp);

        private readonly List<TrajectoryWrapper> _wrappers = new List<TrajectoryWrapper>();
        public long DrawLead = 65536L*3600;

        public long DrawStart = StartTimestamp;
        public long DrawStop = StopTimestamp;
        public long DrawTrail = 65536L*3600;
        public World TheWorld;
        public TrajectoryTimeMode TimeMode = TrajectoryTimeMode.StartStop;
        private TrajectoryWrapper _currentWrapper;
        private LadeeStateFetcher.StateFrame _frame = LadeeStateFetcher.StateFrame.EarthFixed;

        public TrajectoryShape(World world)
        {
            TheWorld = world;
            Visible = false;
        }

        public LadeeStateFetcher.StateFrame Frame
        {
            get { return _frame; }
            set
            {
                LadeeStateFetcher.StateFrame oldFrame = _frame;
                _frame = value;
                if (oldFrame != _frame)
                    LoadMode(Frame);
            }
        }

        public static int TrajectoryCount
        {
            get { return (int) ((StopTimestamp - StartTimestamp)/TrajectoryStep); }
        }

        private void LoadMode(LadeeStateFetcher.StateFrame mode)
        {
            TrajectoryWrapper w = _wrappers.FirstOrDefault(s => s.Mode == mode);
            if (w != null)
            {
                MakeCurrent(w);
                return;
            }
            w = new TrajectoryWrapper(mode, TheWorld);
            _wrappers.Add(w);
            MakeCurrent(w);
        }

        private void MakeCurrent(TrajectoryWrapper w)
        {
            if (Buffer.VboID == 0)
                GL.GenBuffers(1, out Buffer.VboID);
            int size;
            Vector3[] vertices = w.Vectors;
            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffer.VboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (vertices.Length*BlittableValueType.StrideOf(vertices)),
                          vertices, BufferUsageHint.DynamicDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length*BlittableValueType.StrideOf(vertices) != size)
                throw new ApplicationException("Vertex data not uploaded correctly");
            _currentWrapper = w;
        }

        public long GetDrawStart(long t)
        {
            return TimeMode == TrajectoryTimeMode.StartStop ? DrawStart : t - DrawLead;
        }

        public long GetDrawStop(long t)
        {
            return TimeMode == TrajectoryTimeMode.StartStop ? DrawStop : t + DrawTrail;
        }

        private int ClampIndex(long index, int size)
        {
            if (index < 0L) return 0;
            if (index >= size) return size;
            return (int) index;
        }

        public override void Transform(bool near, Vector3d eye)
        {
            if (near)
                TranslateNear(Position, eye);
            else
                TranslateFar(Position, eye);

            // Note, no rotation.  This wasn't here.  It probably should be defaulted, but then the attitude will be identity
        }

        public override void Paint()
        {
            if (_currentWrapper == null) return;

            long start = GetDrawStart(TheWorld.Time);
            long stop = GetDrawStop(TheWorld.Time);
            int length = _currentWrapper.Length;
            int startIndex = ClampIndex((start - StartTimestamp)/TrajectoryStep, length);
            int stopIndex = ClampIndex((stop - StartTimestamp)/TrajectoryStep+1, length);
            int drawCount = stopIndex - startIndex;

            GL.Disable(EnableCap.Lighting);
            GL.Color3(Color.LightBlue);
            GL.LineWidth(2f);

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffer.VboID);
            GL.InterleavedArrays(InterleavedArrayFormat.V3f, 0, IntPtr.Zero);
            GL.DrawArrays(BeginMode.LineStrip, startIndex, drawCount);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.Enable(EnableCap.Lighting);
        }
    }

    public class TrajectoryWrapper
    {
        public string Filename;
        public int Length;
        public LadeeStateFetcher.StateFrame Mode;
        public long Step;
        public long Timestamp;
        public Vector3[] Vectors;

        public TrajectoryWrapper(LadeeStateFetcher.StateFrame mode, World world)
        {
            Mode = mode;
            Filename = MBLib.MBLibData.Combine(@"VBO_Cache\" + mode + ".vbo");

            if (File.Exists(Filename))
            {
                Read();
                Length = TrajectoryShape.TrajectoryCount;

/*                if (false)
                {
                    // debugging
                    LadeeStateFetcher fetcher1 = world.Fetcher;
                    LadeeStateFetcher.StateFrame oldFrame1 = fetcher1.Frame;
                    fetcher1.Frame = mode;
                    var p1 = new Vector3d();
                    Length = TrajectoryShape.TrajectoryCount;
                    var Vectors1 = new Vector3[Length];
                    long start1 = TrajectoryShape.StartTimestamp;
                    for (int i = 0; i < Length; i++)
                    {
                        fetcher1.FetchPosition(LadeeStateFetcher.LadeeId, start1 + i*TrajectoryShape.TrajectoryStep,
                                               ref p1);

                        Vectors1[i].X = (float) p1.X/OrbitOverviewWindow.FarUnit;
                        Vectors1[i].Y = (float) p1.Y/OrbitOverviewWindow.FarUnit;
                        Vectors1[i].Z = (float) p1.Z/OrbitOverviewWindow.FarUnit;

                        if (Vectors[i].X != Vectors1[i].X)
                            Console.WriteLine("x");
                        if (Vectors[i].Y != Vectors1[i].Y)
                            Console.WriteLine("y");
                        if (Vectors[i].Z != Vectors1[i].Z)
                            Console.WriteLine("z");
                    }
                    fetcher1.Frame = oldFrame1;
                }*/
                return;
            }

            LadeeStateFetcher fetcher = world.Fetcher;
            LadeeStateFetcher.StateFrame oldFrame = fetcher.Frame;
            fetcher.Frame = mode;
            var p = new Vector3d();
            Length = TrajectoryShape.TrajectoryCount;
            Vectors = new Vector3[Length];
            long start = TrajectoryShape.StartTimestamp;
            for (int i = 0; i < Length; i++)
            {
                fetcher.FetchPosition(LadeeStateFetcher.LadeeId, start + i*TrajectoryShape.TrajectoryStep, ref p);
                Vectors[i].X = (float) p.X/OrbitOverviewWindow.FarUnit;
                Vectors[i].Y = (float) p.Y/OrbitOverviewWindow.FarUnit;
                Vectors[i].Z = (float) p.Z/OrbitOverviewWindow.FarUnit;
            }
            fetcher.Frame = oldFrame;
            Write();
        }

        public void Write()
        {
            using (var fs = new FileStream(Filename, FileMode.Create, FileAccess.Write))
            using (var bw = new BinaryWriter(fs))
            {
                foreach (Vector3 v in Vectors)
                {
                    bw.Write(v.X);
                    bw.Write(v.Y);
                    bw.Write(v.Z);
                }
            }
        }

        public unsafe void Read()
        {
            //var bytes = File.ReadAllBytes(Filename);
            //Vectors = (Vector3[])GetObjectFromBytes(bytes, typeof(Vector3[]));
            //return;

            byte[] bytes = File.ReadAllBytes(Filename);
            int len = bytes.Length/sizeof (Vector3);
            var v = new Vector3[len];
            int step = sizeof (Vector3);

            fixed (byte* ptr = bytes)
                for (int i = 0; i < len; i++)
                {
                    v[i].X = *((float*) (ptr + i*step));
                    v[i].Y = *((float*) (ptr + i*step + sizeof (float)));
                    v[i].Z = *((float*) (ptr + i*step + sizeof (float) + sizeof (float)));
                }
            Vectors = v;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TestVector
    {
        public float X;
        public float Y;
        public float Z;
    }
}
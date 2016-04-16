using OpenTK;

namespace Shadow.viz
{
    public class Presentation
    {
        public void UpdateCamera(OpenGLControlWrapper w)
        {
        }

        public void PaintFarScene(OpenGLControlWrapper w, Vector3d eye)
        {
            for (int i = 0; i < w.TheWorld.FarShapes.Count; i++)
                w.TheWorld.FarShapes[i].Draw(false, eye); // modified
        }

        public void PaintNearScene(OpenGLControlWrapper w, Vector3d eye)
        {
            for (int i = 0; i < w.TheWorld.NearShapes.Count; i++)
                w.TheWorld.NearShapes[i].Draw(true, eye);
        }

        public void PaintSensors(OpenGLControlWrapper w, Vector3d eye)
        {
            for (int i = 0; i < w.TheWorld.NearShapes.Count; i++)
                w.TheWorld.NearShapes[i].DrawSensors(eye);
        }
    }

    public class PhasingLoopsOverview : Presentation
    {
    }

    public class Science : Presentation
    {
    }
}
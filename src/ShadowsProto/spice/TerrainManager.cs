using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace LadeeViz.Spice
{
    public class TerrainManager
    {
        //private const string TerrainImageFile = @"C:\UVS\svn\src\TestData\Terrain\ldem_64.img";
        private const string TerrainImageFile = @"Terrain\ldem_64.img";
        MemoryMappedFile _mmf;
        MemoryMappedViewAccessor _accessor;

        public TerrainManager()
        {
            Open(TerrainImageFile);
        }

        public TerrainManager(string filename)
        {
            Open(filename);
        }

        public void Open(string filename)
        {
            _mmf = MemoryMappedFile.CreateFromFile(filename, FileMode.Open, "MissionManager", 0L, MemoryMappedFileAccess.Read);

            // This maps the whole file.  There is an accessor creation method
            // that maps only a section of the file, which could be used for larger
            // files, but it would need to know what the lat/lon are first.
            //_accessor = _mmf.CreateViewAccessor();
            _accessor = _mmf.CreateViewAccessor(0, 0L, MemoryMappedFileAccess.Read);
        }

        public void Close()
        {
            Dispose(true);
        }

        public void Dispose(bool ignore)
        {
            if (_mmf == null) return;
            _mmf.Dispose();
            _mmf = null;
            _accessor = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat">Latitude in Degrees</param>
        /// <param name="lon">Longitude in Degrees</param>
        /// <returns>Terrain Height in Meters</returns>
        public float ReadTerrainHeight(double lat, double lon)
        {
            var row = (long)Math.Round(5760 - 64 * lat);
            var col = (long)Math.Round(64 * lon);
            var pos = 46080 * row + 2 * col;

            var h = _accessor.ReadInt16(pos);

            var height = 0.5f * h;
            return height;
        }

        public void TerrainMinMax(ref float min, ref float max)
        {
            min = float.MaxValue;
            max = float.MinValue;

            if (true)
            {
                for (var lat = -89d; lat < 89d; lat = lat + 1d)
                    for (var lon = 1d; lon < 359d; lon = lon + 1d)
                    {
                        var height = ReadTerrainHeight(lat, lon);
                        if (height < min) min = height;
                        if (height > max) max = height;
                    }
            }
            else
            {
                for (var i = 0; i < 265420800; i = i + 2)
                {
                    var height = 0.5f * _accessor.ReadInt16(i);
                    if (height < min) min = height;
                    if (height > max) max = height;
                }       
            }

        }
    }
}

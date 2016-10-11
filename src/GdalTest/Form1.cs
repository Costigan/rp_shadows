using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gdal = OSGeo.GDAL.Gdal;
using Ogr = OSGeo.OGR.Ogr;

namespace GdalTest
{
    public partial class Form1 : Form
    {
        //string sample_diff = @"C:\git\rp_shadows\data\synthetic-lunar-patch.tif";
        string sample_diff = @"C:\RP\maps\Nobile\Andy_Nobile-C3_2020_DSN\ldem_80s_20m.img.DSN.2020-02-14T00-00-00.050.tex.tif";
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GdalConfiguration.ConfigureGdal();
            var dataset = Gdal.Open(sample_diff, OSGeo.GDAL.Access.GA_ReadOnly);
            if (dataset == null)
            {
                Console.WriteLine(@"couldn't open file");
                return;
            }
            Console.WriteLine(@"Size=[{0},{1}]", dataset.RasterXSize, dataset.RasterYSize);
            var geoTransform = new double[6];
            dataset.GetGeoTransform(geoTransform);
            Console.WriteLine(@"Origin = [{0},{1}]", geoTransform[0], geoTransform[3]);
            Console.WriteLine(@"Pixel size=[{0},{1}]", geoTransform[1], geoTransform[5]);


            foreach (var v in geoTransform)
                Console.WriteLine(v);

            if (dataset.RasterCount < 1)
                throw new Exception(@"No bands");

            var band = dataset.GetRasterBand(1);
            int blockX, blockY;
            band.GetBlockSize(out blockX, out blockY);
            Console.WriteLine(@"Block size=[{0},{1}]", blockX, blockY);
            Console.WriteLine(Gdal.GetDataTypeName(band.DataType));
        }
    }
}

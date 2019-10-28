using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AE_GraphDemo
{
    //定义一个Operation枚举
    enum Operation
    {
        ConstructionPoint,//绘制点
        ConstructionMultPoint,//绘制多点
        ConstructionPolyLine,//绘制折线
        ConstructionPolygon,//绘制画
        Nothing
    }

    //声明为密封类, 则不能被其他类继承
    public sealed partial class Form1 : Form
    {
        //声明类的私有成员
        private IMapControl3 m_mapControl = null;
        private string m_mapDocumentName = string.Empty;

        //声明Operation枚举类型的变量
        Operation oprFlag = Operation.Nothing;
        //定义IGeometryCollection对象, 提供对成员的访问, 可用于访问、添加和删除单个几何多部分的几何(多点、折线、多边形)
        IGeometryCollection geoCollection;
        object missing;
        IGeometry pGeometry;
        /// <summary>
        /// 主函数
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //获得MapControl
            m_mapControl = (IMapControl3)axMapControl1.Object;
            OpenMxd();
        }
        /// <summary>
        /// 打开Mxd文件
        /// </summary>
        private void OpenMxd()
        {
            IMapDocument mapDocument = new MapDocumentClass();
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "打开地图文档";
                ofd.Filter = "map documents(*.mxd)|*.mxd";
                ofd.InitialDirectory = Application.StartupPath + @"\data";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string pFileName = ofd.FileName;
                    //filePath——地图文档的路径, ""——赋予默认密码
                    mapDocument.Open(pFileName, "");
                    for (int i = 0; i < mapDocument.MapCount; i++)
                    {
                        //通过get_Map(i)方法逐个加载
                        axMapControl1.Map = mapDocument.get_Map(i);
                    }
                    axMapControl1.Refresh();
                }
                else
                {
                    mapDocument = null;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        /// <summary>
        /// 鼠标移动的函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            try
            {
                toolStripStatusLabel1.Text = string.Format("{0},{1}  {2}", e.mapX.ToString("#######.##"), e.mapY.ToString("#######.##"), axMapControl1.MapUnits.ToString().Substring(4));
            }
            catch
            { }

        }

        private void 点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }
    }
}

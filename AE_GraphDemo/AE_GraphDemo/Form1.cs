using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AE_GraphDemo
{
    //定义一个Operation枚举
    enum Operation
    {
        ConstructionPoint,//绘制点
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
        //定义IPointCollection接口对象, 该接口对象(变量)提供对多点、环、折线、多边形等几何图形的操作方法
        IPointCollection ptCollection;
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
        #region 添加图形绘制的单击事件
        private void 点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            oprFlag = Operation.ConstructionPoint;
        }


        private void 折线ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            oprFlag = Operation.ConstructionPolyLine;
            geoCollection = new PolylineClass();
            ptCollection = new PolylineClass();
        }

        private void 面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            oprFlag = Operation.ConstructionPolygon;
        }


        #endregion


        /// <summary>
        /// axMapContol控件的鼠标单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            //表示 System.Type 信息中的缺少值。 此字段为只读。
            missing = Type.Missing;
            //若为添加点的事件
            if (oprFlag == Operation.ConstructionPoint)
            {
                //axMapControl1控件的当前地图工具为空
                axMapControl1.CurrentTool = null;
                //通过AddPointByStore函数, 获取绘制点的图层——Cities
                //从GetPoint函数获取点的坐标
                AddPointByStore("Cities", GetPoint(e.mapX, e.mapY) as IPoint);
                //点添加完之后结束编辑状态
                oprFlag = Operation.Nothing;
            }
            //若为添加折线的事件
            if (oprFlag == Operation.ConstructionPolyLine)
            {
                //axMapControl1控件的当前地图工具为空
                axMapControl1.CurrentTool = null;
                //获取鼠标单击的坐标
                //ref参数能够将一个变量带入一个方法中进行改变, 改变完成后, 再将改变后的值带出方法
                //ref参数要求在方法外必须为其赋值, 而方法内可以不赋值
                ptCollection.AddPoint(GetPoint(e.mapX, e.mapY), ref missing, ref missing);
                //定义集合类型绘制折线的方法
                pGeometry = axMapControl1.TrackLine();

                //通过addFeature函数的两个参数, Highways——绘制折线的图层; Geometry——绘制的几何折线
                AddFeature("Highways", pGeometry);

                //折线添加完之后结束编辑状态
                oprFlag = Operation.Nothing;
            }
            //若为添加面的事件
            if (oprFlag == Operation.ConstructionPolygon)
            {
                //axMapControl1控件的当前地图工具为空
                axMapControl1.CurrentTool = null;
                //
                CreateDrawPolygon(axMapControl1.ActiveView, "Counties");
                //面添加完之后结束编辑状态
                oprFlag = Operation.Nothing;
            }
        }
        /// <summary>
        /// 添加面事件
        /// </summary>
        /// <param name="activeView"></param>
        /// <param name="v"></param>
        private void CreateDrawPolygon(IActiveView activeView, string sLayer)
        {
            //绘制多边形事件
            pGeometry = axMapControl1.TrackPolygon();
            //通过AddFeature函数的两个参数, sLayer——绘制折线的图层; pGeometry——绘制几何的图层
            AddFeature(sLayer, pGeometry);
        }

        /// <summary>
        /// 添加实体对象到地图图层(添加线、面要素)
        /// </summary>
        /// <param name="layerName">图层名称</param>
        /// <param name="pGeometry">绘制形状(线、面)</param>
        private void AddFeature(string layerName, IGeometry pGeometry)
        {
            //得到要添加地物的图层
            IFeatureLayer pFeatureLayer = GetLayerByName(layerName) as IFeatureLayer;
            if (pFeatureLayer != null)
            {
                //定义一个地物类, 把要编辑的图层转化为定义的地物类
                IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                //先定义一个编辑的工作空间, 然后将其转化为数据集, 最后转化为编辑工作空间
                IWorkspaceEdit w = (pFeatureClass as IDataset).Workspace as IWorkspaceEdit;
                IFeature pFeature;

                //开始事务操作
                w.StartEditing(true);
                //开始编辑
                w.StartEditOperation();

                //在内存创建一个用于暂时存放编辑数据的要素(FeatureBuffer)
                IFeatureBuffer pFeatureBuffer = pFeatureClass.CreateFeatureBuffer();
                //定义游标
                IFeatureCursor pFtCursor;
                //查找到最后一条记录, 游标指向该记录后再进行插入操作
                pFtCursor = pFeatureClass.Search(null, true);
                pFeature = pFtCursor.NextFeature();
                //开始插入新的实体对象(插入对象要使用Insert游标)
                pFtCursor = pFeatureClass.Insert(true);
                try
                {
                    //向缓存游标的Shape属性赋值
                    pFeatureBuffer.Shape = pGeometry;
                }
                catch (COMException ex)
                {
                    MessageBox.Show("绘制的几何图形超出了边界！");
                    return;
                }
                //判断:几何图形是否为多边形
                if (pGeometry.GeometryType.ToString() == "ESRIGeometryPolygon")
                {
                    int index = pFeatureBuffer.Fields.FindField("STATE_NAME");
                    pFeatureBuffer.set_Value(index, "California");
                }
                object featureOID = pFtCursor.InsertFeature(pFeatureBuffer);
                //保存实体
                pFtCursor.Flush();

                //结束编辑
                w.StopEditOperation();
                //结束事务操作
                w.StopEditing(true);
            }
            else
            {
                MessageBox.Show("未发现" + layerName + "图层");
            }
        }

        /// <summary>
        /// 获取鼠标单击时的坐标位置信息
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private IPoint GetPoint(double x, double y)
        {
            IPoint pt = new PointClass();
            pt.PutCoords(x, y);
            return pt;
        }
        /// <summary>
        /// 获取绘制点的图层——Cities, 保存点绘制的函数
        /// </summary>
        /// <param name="pointLayerName"></param>
        /// <param name="point"></param>
        private void AddPointByStore(string pointLayerName, IPoint pt)
        {
            //得到要添加地物的图层
            IFeatureLayer pFeatureLayer = GetLayerByName(pointLayerName) as IFeatureLayer;
            if (pFeatureLayer != null)
            {
                //定义一个地物类, 把要编辑的图层转化为定义的地物类
                IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                //先定义一个编辑的工作空间, 然后将其转化为数据集, 最后转化为编辑工作空间
                IWorkspaceEdit w = (pFeatureClass as IDataset).Workspace as IWorkspaceEdit;
                IFeature pFeature;
                //开始事务操作
                w.StartEditing(false);
                //开始编辑
                w.StartEditOperation();
                //创建一个(点)要素
                pFeature = pFeatureClass.CreateFeature();
                //赋值该要素的Shape属性
                pFeature.Shape = pt;

                //保存要素, 完成点要素生成
                //此时生成的点要素只要集合特征(shape/Geometry), 无普通属性
                pFeature.Store();

                //结束编辑
                w.StopEditOperation();
                //结束事务操作
                w.StopEditing(true);

            }
            //屏幕刷新
            this.axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, pFeatureLayer, null);

        }
        /// <summary>
        /// 通过名字获取图层函数
        /// </summary>
        /// <param name="pointLayerName">图层名</param>
        /// <returns>返回图层</returns>
        private ILayer GetLayerByName(string layerName)
        {
            //声明一个图层为空
            ILayer layer = null;
            //遍历axMapControl1图层对象
            for (int i = 0; i < axMapControl1.LayerCount; i++)
            {
                //获取单个图层
                layer = axMapControl1.Map.get_Layer(i);
                //当遍历的图层名等同于传来的名字则结束循环
                if (layer.Name == layerName)
                {
                    break;
                }
                else
                {
                    layer = null;//重新赋值为null
                }
            }
            return layer;//返回图层
        }
    }
}

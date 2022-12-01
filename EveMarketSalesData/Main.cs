using DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EveMarketSalesData
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }
        Dictionary<string,long> regionPairs = new Dictionary<string,long>();
        private void Main_Load(object sender, EventArgs e)
        {
            MarketDataDB db = new MarketDataDB();

            var regions  = from table in db.EveRegions select table;
            foreach (var region in regions)
            {
                regionPairs.Add(region.Name, region.Id);
                comboBox1.Items.Add(region.Name);
            }
            comboBox1.SelectedIndex = 0;
            var result = from table in db.EveMarketGroups.Where(item => item.ParentGroupId == 0) select table;
            foreach (var item in result)
            {
                TreeNode topNode = new TreeNode(item.NameZh);
                topNode.Tag = item.Id;
                treeView_type.Nodes.Add(topNode);
                FillChildren(topNode.Nodes, item.Id);
            }
        }

        private void FillChildren(TreeNodeCollection node,long? id) 
        {
            MarketDataDB db = new MarketDataDB();
            var result = from table in db.EveMarketGroups.Where(item => item.ParentGroupId == id)
                         select table;
            foreach (var item in result)
            {
                TreeNode topNode = new TreeNode(item.NameZh);
                topNode.Tag = item.Id;
                node.Add(topNode);
                FillChildren(topNode.Nodes, item.Id);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (treeView_type.SelectedNode == null) 
            {
                MessageBox.Show("请先选择分类", "警告",MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 开始获取所选分类下的所有分类

            List<long?> groupIds = new List<long?>();
            MarketDataDB db = new MarketDataDB();
            var result = from table in db.EveMarketGroups.Where(item => item.ParentGroupId == Convert.ToInt64(treeView_type.SelectedNode.Tag)) select table;
            while (result.Count() >0) 
            {
                List<long?> ids = new List<long?>();
                foreach (var item in result) 
                {
                    ids.Add(item.Id);
                    groupIds.Add(item.Id);
                }
                MarketDataDB db1 = new MarketDataDB();
                result = from table1 in db1.EveMarketGroups.Where(item => ids.Contains(item.ParentGroupId)) select table1;
            }

            var typeGroups = from table in db.EveTypeGroups.Where(item => groupIds.Contains(item.GroupId)) select table;
            List<long?> typeIds =   new List<long?>();
            foreach (var typeGroup in typeGroups) 
            {
               typeIds.Add(typeGroup.TypeId);
            }

            Dictionary<long?,string> idNamePairs = new Dictionary<long?,string>();
            var idName = from table in db.EveItemNames.Where(item=>item.Type == 8) select table;
            foreach (var idNamePair in idName) 
            {
                if (!idNamePairs.ContainsKey(idNamePair.ItemId)) 
                {
                    idNamePairs.Add(idNamePair.ItemId, idNamePair.ZhName);
                }
                
            }
            lab_pro.ForeColor = Color.LightSeaGreen;
            foreach (var typeId in typeIds) 
            {
                if (idNamePairs.ContainsKey(typeId))
                {
                    lab_pro.Text = idNamePairs[typeId];
                }
                else 
                {
                    lab_pro.Text = "未知名称";
                }
                Thread.Sleep(200);

            }


        }
    }
}

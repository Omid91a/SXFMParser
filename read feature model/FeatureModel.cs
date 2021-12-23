using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace read_feature_model
{
    public class FeatureTreeNode
    {
        public string ID { get; set; }
        public string Name { get; set; }
        private readonly List<FeatureTreeNode> Children = new List<FeatureTreeNode> { };
        public FeatureTreeNode(string name = "", string id = "")
        {
            this.ID = id;
            this.Name = name;
        }
        public void AddChild(FeatureTreeNode child)
        {
            Children.Add(child);
        }

        public int ChildCount()
        {
            return Children.Count;
        }
        public FeatureTreeNode GetChildAt(int index)
        {
            return Children[index];
        }
        public List<FeatureTreeNode> GetChildren()
        {
            return Children;
        }
    }
    public class RootNode : FeatureTreeNode
    {
        public RootNode(string name, string id) : base(name, id)
        {

        }
    }
    public class FeatureGroup : FeatureTreeNode
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public FeatureGroup() : base("", "")
        {

        }
    }
    public class SolitaireFeature : FeatureTreeNode
    {
        public bool IsOptional { get; set; }
        public SolitaireFeature(string name, string id) : base(name, id)
        {

        }
    }
    public class FeatureModel
    {
        public RootNode Root { get; set; }
        protected List<PropositionalFormula> constraints = new List<PropositionalFormula> { };

        public FeatureModel(string fileName)
        {

        }

        public List<PropositionalFormula> GetConstraints()
        {
            return constraints;
        }

        public void SetConstraints(List<PropositionalFormula> value)
        {
            constraints = value;
        }

        public void AddConstraints(PropositionalFormula value)
        {
            constraints.Add(value);
        }
        public void LoadModel()
        {

        }
    }

    public class PropositionalFormula
    {
        public string Name { get; set; }
        public string FirstOperator { get; set; }
        public string SecondOperator { get; set; }
        public FeatureTreeNode FirstItem { get; set; }
        public FeatureTreeNode SecondItem { get; set; }
        override public string ToString()
        {
            return FirstOperator + FirstItem.ID + " Or " + SecondOperator + SecondItem.ID;
        }
    }
    public class XMLFeatureModel : FeatureModel
    {
        private List<KeyValuePair<FeatureTreeNode, int>> nodeList;
        public Dictionary<string, string> Attributes { get; set; }
        public int MyProperty { get; set; }
        /// <summary>
        /// Load header attributes for more detailes
        /// </summary>
        /// <param name="xmlData">The XML part of file</param>
        private void LoadAttributes(XmlNodeList xmlData)
        {
            Attributes = new Dictionary<string, string> { };
            foreach (XmlNode node in xmlData)
            {
                Attributes.Add(node.Attributes.GetNamedItem("name").Value, node.InnerText);
            }
        }
        /// <summary>
        /// Load all feature tree and add to root.
        /// </summary>
        /// <param name="xmlData">The XML part of file</param>
        private void LoadFeatureTree(XmlNodeList xmlData)
        {
            if (xmlData.Count == 0)
            {
                Debug.WriteLine("Feature tree is empty");
                return;
            }
            List<string> treeBody = xmlData.Item(0).InnerText.ToString().Trim().Split('\n').ToList();
            nodeList = new List<KeyValuePair<FeatureTreeNode, int>> { };
            int lastLevel = 0;
            foreach (string row in treeBody)
            {
                string name = "";
                string id = "";
                int currentLevel = row.Where(c => c == '\t').Count();
                string rowTemp = row.Replace("\t", "");
                FeatureTreeNode node = null;
                // root feature
                if (rowTemp.Contains(":r "))
                {
                    rowTemp = rowTemp.Replace(":r ", "");
                    int parantesIndex = rowTemp.IndexOf("(");
                    int parantesEndIndex = rowTemp.IndexOf(")");
                    name = rowTemp.Substring(0, parantesIndex);
                    id = rowTemp.Substring(parantesIndex + 1, parantesEndIndex - parantesIndex - 1);
                    node = new RootNode(name, id);
                }
                // optional feature
                else if (rowTemp.Contains(":o "))
                {
                    rowTemp = rowTemp.Replace(":o ", "");
                    int parantesIndex = rowTemp.IndexOf("(");
                    int parantesEndIndex = rowTemp.IndexOf(")");
                    name = rowTemp.Substring(0, parantesIndex);
                    id = rowTemp.Substring(parantesIndex + 1, parantesEndIndex - parantesIndex - 1);
                    node = new SolitaireFeature(name, id);
                    ((SolitaireFeature)node).IsOptional = true;
                }
                // mandatory feature
                else if (rowTemp.Contains(":m "))
                {
                    rowTemp = rowTemp.Replace(":m ", "");
                    int parantesIndex = rowTemp.IndexOf("(");
                    int parantesEndIndex = rowTemp.IndexOf(")");
                    name = rowTemp.Substring(0, parantesIndex);
                    id = rowTemp.Substring(parantesIndex + 1, parantesEndIndex - parantesIndex - 1);
                    node = new SolitaireFeature(name, id);
                    ((SolitaireFeature)node).IsOptional = false;
                }
                // inclusive-OR feature group with cardinality [1..*] ([1..3] also allowed)
                else if (rowTemp.Contains(":g "))
                {
                    rowTemp = rowTemp.Replace(":g ", "");
                    int parantesIndex = rowTemp.IndexOf("[");
                    int parantesEndIndex = rowTemp.IndexOf("]");
                    var range = rowTemp.Substring(parantesIndex + 1, parantesEndIndex - parantesIndex - 1).Split(',');
                    node = new FeatureGroup();
                    ((FeatureGroup)node).Min = Convert.ToInt32(range[0] != "*" ? range[0] : "25000");
                    ((FeatureGroup)node).Max = Convert.ToInt32(range[1] != "*" ? range[1] : "25000");
                }
                // grouped feature
                else if (rowTemp.Contains(": "))
                {
                    rowTemp = rowTemp.Replace(": ", "");
                    int parantesIndex = rowTemp.IndexOf("(");
                    int parantesEndIndex = rowTemp.IndexOf(")");
                    name = rowTemp.Substring(0, parantesIndex);
                    id = rowTemp.Substring(parantesIndex + 1, parantesEndIndex - parantesIndex - 1);
                    node = new FeatureTreeNode(name, id);
                }
                nodeList.Add(new KeyValuePair<FeatureTreeNode, int>(node, currentLevel));
                lastLevel = currentLevel;
            }
            // connect nodes
            while (nodeList.Count > 1)
            {
                int maxLevel = nodeList.Select(x => x.Value).Max();
                for (int i = 0; i < nodeList.Count; i++)
                {
                    if (nodeList[i + 1].Value > nodeList[i].Value && nodeList[i + 1].Value == maxLevel)
                    {
                        nodeList[i].Key.AddChild(nodeList[i + 1].Key);
                        nodeList.RemoveAt(i + 1);
                        break;
                    }
                }
            }
            // set single remaining node as Root
            this.Root = (RootNode)nodeList[0].Key;
        }

        private FeatureTreeNode GetNodeByID(FeatureTreeNode root, string id)
        {
            if (root.ID == id)
                return root;
            else if (root.ChildCount() > 0)
            {
                foreach (FeatureTreeNode node in root.GetChildren())
                {
                    var tempNode = GetNodeByID(node, id);
                    if (tempNode != null)
                        return tempNode;
                }
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlData">The XML part of file</param>
        private void LoadConstraints(XmlNodeList xmlData)
        {
            constraints = new List<PropositionalFormula> { };
            List<string> lines = xmlData.Item(0).InnerText.Trim().Split('\n').ToList();
            foreach (string line in lines)
            {
                string constraintName = line.Split(':')[0];
                string constraintFormula = line.Split(':')[1];
                string firstItemId = constraintFormula.Replace(" or ", ",").Split(',')[0];
                string secondItemId = constraintFormula.Replace(" or ", ",").Split(',')[1];
                bool firstOper = firstItemId.Contains("~");
                bool secondOper = secondItemId.Contains("~");
                firstItemId = firstOper ? firstItemId.Replace("~", "") : firstItemId;
                secondItemId = secondOper ? secondItemId.Replace("~", "") : secondItemId;
                firstItemId = firstItemId.Replace(" ", "");
                secondItemId = secondItemId.Replace(" ", "");
                PropositionalFormula formula = new PropositionalFormula();
                formula.Name = constraintName;
                formula.FirstOperator = firstOper ? "~" : "";
                formula.SecondOperator = secondOper ? "~" : "";
                var item = GetNodeByID(this.Root, firstItemId);
                formula.FirstItem = item;
                item = GetNodeByID(this.Root, secondItemId);
                formula.SecondItem = item;
                constraints.Add(formula);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="value"></param>
        public XMLFeatureModel(string fileName) : base(fileName)
        {
            string fileData = File.ReadAllText(fileName);
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(fileData);
            XmlNodeList xmlAttributeList = xdoc.SelectNodes("//data");
            XmlNodeList xmlFeatureTree = xdoc.SelectNodes("//feature_tree");
            XmlNodeList xmlConstraints = xdoc.SelectNodes("//constraints");
            //
            LoadAttributes(xmlAttributeList);
            LoadFeatureTree(xmlFeatureTree);
            LoadConstraints(xmlConstraints);
        }
    }
}

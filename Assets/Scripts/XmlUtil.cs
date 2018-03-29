using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using UnityEngine;

namespace Assets.UI
{
    public class XmlUtil : System.Object
    {
        protected string strXmlFile;
        protected XmlDocument objXmlDoc = new XmlDocument();



        /// <summary>
        /// 根据isFile区别XmlFile为xml文件路径还是xml字符串，并获取XmlDocument对象
        /// </summary>
        /// <param name="XmlFile"></param>
        /// <param name="isFile"></param>
        public XmlUtil(string XmlFile, Boolean isFile)
        {
            strXmlFile = XmlFile;
            if (isFile)
            {
                objXmlDoc = GetXmlDocByFilePath(XmlFile);
            }
            else
            {
                objXmlDoc = GetXmlDocByXmlContent(XmlFile);
            }
        }
        /// <summary>
        /// 创建xml文件，并获取XmlDocument对象
        /// </summary>
        /// <param name="XmlFile"></param>
        /// <param name="bOverWrite"></param>
        /// <param name="sRoot"></param>
        public XmlUtil(string XmlFile, Boolean bOverWrite, string sRoot)
        {
            try
            {
                createDirectory(XmlFile);
                //如果覆盖模式，则强行创建一个xml文档
                if (bOverWrite)
                {
                    objXmlDoc.AppendChild(objXmlDoc.CreateXmlDeclaration("1.0", "utf-8", null));//设置xml的版本，格式信息
                    objXmlDoc.AppendChild(objXmlDoc.CreateElement("", sRoot, ""));//创建根元素
                    objXmlDoc.Save(XmlFile);//保存
                }
                else //否则，检查文件是否存在，不存在则创建
                {
                    if (!(File.Exists(XmlFile)))
                    {
                        objXmlDoc.AppendChild(objXmlDoc.CreateXmlDeclaration("1.0", "utf-8", null));
                        objXmlDoc.AppendChild(objXmlDoc.CreateElement("", sRoot, ""));
                        objXmlDoc.Save(XmlFile);
                    }
                }
                objXmlDoc.Load(XmlFile);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            strXmlFile = XmlFile;
        }


        public XmlDocument getXmlDocument()
        {
            return objXmlDoc;
        }
        public void createDirectory(string XmlFile)
        {
            string path = XmlFile.Substring(0, XmlFile.LastIndexOf('/'));
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// 更新节点内容
        /// </summary>
        /// <param name="xmlPathNode"></param>
        /// <param name="Content"></param>
        public void UpdateNode(string xmlPathNode, string content)
        {
            objXmlDoc.SelectSingleNode(xmlPathNode).InnerText = content;
        }

        /// <summary>
        /// 更新节点内容
        /// </summary>
        /// <param name="xmlPathNode"></param>
        /// <param name="Content"></param>
        public void UpdateNode(XmlNode Node, string content)
        {
            Node.InnerText = content;
        }

        /// <summary>
        /// 更新节点的某个属性
        /// </summary>
        /// <param name="xmlPathNode">要操作的节点</param>
        /// <param name="AttribName">属性名</param>
        /// <param name="AttribValue">属性值</param>
        public void UpdateNode(string xmlPathNode, string AttribName, string AttribValue)
        {

            ((XmlElement)(objXmlDoc.SelectSingleNode(xmlPathNode))).SetAttribute(AttribName, AttribValue);
        }


        /// <summary>
        /// 修改节点(同步更新内容和属性)
        /// </summary>
        /// <param name="xmlPathNode">要操作节点的xpath语句</param>
        /// <param name="arrAttribName">属性名称字符串数组</param>
        /// <param name="arrAttribContent">属性内容字符串数组</param>
        /// <param name="Content">节点内容</param>
        public void UpdateNode(string xmlPathNode, string[] arrAttribName, string[] arrAttribContent, string content)
        {

            XmlNode xn = objXmlDoc.SelectSingleNode(xmlPathNode);
            if (xn != null)
            {
                xn.InnerText = content;
                xn.Attributes.RemoveAll();
                for (int i = 0; i <= arrAttribName.GetUpperBound(0); i++)
                {
                    ((XmlElement)(xn)).SetAttribute(arrAttribName[i], arrAttribContent[i]);
                }

            }
        }

        /// <summary>
        /// 移除选定节点集的所有属性
        /// </summary>
        /// <param name="xmlPathNode"></param>
        public void RemoveAllAttribute(string xmlPathNode)
        {
            XmlNodeList xnl = objXmlDoc.SelectNodes(xmlPathNode);
            foreach (XmlNode xn in xnl)
            {
                xn.Attributes.RemoveAll();
            }
        }


        public void DeleteNode(string Node)
        {
            //刪除一个节点。
            try
            {
                string mainNode = Node.Substring(0, Node.LastIndexOf("/"));
                objXmlDoc.SelectSingleNode(mainNode).RemoveChild(objXmlDoc.SelectSingleNode(Node));
            }
            catch
            {
                //throw;   
                return;
            }
        }


        public void InsertNodeWithChild(string mainNode, string ChildNode, string Element, string Content)
        {
            //插入一节点和此节点的一子节点。
            XmlNode objRootNode = objXmlDoc.SelectSingleNode(mainNode);
            XmlElement objChildNode = objXmlDoc.CreateElement(ChildNode);
            objRootNode.AppendChild(objChildNode);//插入节点
            XmlElement objElement = objXmlDoc.CreateElement(Element);
            objElement.InnerText = Content;
            objChildNode.AppendChild(objElement);//插入子节点
        }

        /// <summary>
        /// 插入一个节点，带一个Attribute和innerText
        /// </summary>
        /// <param name="mainNode"></param>
        /// <param name="Element">节点名称</param>
        /// <param name="Attrib">Attribute名称</param>
        /// <param name="AttribContent">Attribute值</param>
        /// <param name="Content">innerText值</param>
        public void InsertNode(string mainNode, string Element, string Attrib, string AttribContent, string Content)
        {
            XmlNode objNode = objXmlDoc.SelectSingleNode(mainNode);
            XmlElement objElement = objXmlDoc.CreateElement(Element);
            objElement.SetAttribute(Attrib, AttribContent);
            objElement.InnerText = Content;
            objNode.AppendChild(objElement);
        }

        /// <summary>
        /// 插入一个节点，带一个Attribute
        /// </summary>
        /// <param name="mainNode"></param>
        /// <param name="Element">节点名称</param>
        /// <param name="Attrib">Attribute名称</param>
        /// <param name="AttribContent">Attribute值</param>   
        public void InsertNode(string mainNode, string Element, string Attrib, string AttribContent)
        {
            XmlNode objNode = objXmlDoc.SelectSingleNode(mainNode);
            XmlElement objElement = objXmlDoc.CreateElement(Element);
            objElement.SetAttribute(Attrib, AttribContent);
            objNode.AppendChild(objElement);
        }


        /// <summary>
        /// 插入一个节点
        /// </summary>
        /// <param name="mainNode"></param>
        /// <param name="Element">节点名称</param>       
        public void InsertNode(string mainNode, string Element)
        {
            XmlNode objNode = objXmlDoc.SelectSingleNode(mainNode);
            XmlElement objElement = objXmlDoc.CreateElement(Element);
            objNode.AppendChild(objElement);
        }


        //<summary>
        //插入一个节点，带多个属性和一个inner text
        //</summary>
        public void InsertNode(string mainNode, string elementName, string[] arrAttributeName, string[] arrAttributeContent, string elementContent)
        {
            try
            {
                XmlNode objNode = objXmlDoc.SelectSingleNode(mainNode);
                XmlElement objElement = objXmlDoc.CreateElement(elementName);
                for (int i = 0; i <= arrAttributeName.GetUpperBound(0); i++)
                {
                    objElement.SetAttribute(arrAttributeName[i], arrAttributeContent[i]);
                }
                objElement.InnerText = elementContent;
                objNode.AppendChild(objElement);
            }
            catch
            {
                throw;
                //string t = mainNode;
                //;
            }
        }

        ///<summary>
        ///插入一个节点，带多个属性
        ///</summary>
        public void InsertNode(string mainNode, string elementName, string[] arrAttributeName, string[] arrAttributeContent)
        {
            try
            {
                XmlNode objNode = objXmlDoc.SelectSingleNode(mainNode);
                XmlElement objElement = objXmlDoc.CreateElement(elementName);
                for (int i = 0; i <= arrAttributeName.GetUpperBound(0); i++)
                {
                    objElement.SetAttribute(arrAttributeName[i], arrAttributeContent[i]);
                }
                //objElement.InnerText = elementContent;
                objNode.AppendChild(objElement);
            }
            catch
            {
                throw;
                //string t = mainNode;
                //;
            }
        }

        /// <summary>
        /// 插入子节点(带多个属性)
        /// </summary>
        /// <param name="parentNode">要插入的父节点</param>
        /// <param name="elementName">插入的节点名称</param>
        /// <param name="arrAttributeName">属性名称[数组]</param>
        /// <param name="arrAttributeContent">属性内容[数组]</param>
        /// <param name="elementContent">节点内容</param>
        public void AddChildNode(string parentNodePath, string elementName, string[] arrAttributeName, string[] arrAttributeContent, string elementContent)
        {
            try
            {
                XmlNode parentNode = objXmlDoc.SelectSingleNode(parentNodePath);
                XmlElement objChildElement = objXmlDoc.CreateElement(elementName);
                for (int i = 0; i <= arrAttributeName.GetUpperBound(0); i++)
                {
                    objChildElement.SetAttribute(arrAttributeName[i], arrAttributeContent[i]);
                }
                objChildElement.InnerText = elementContent;
                parentNode.AppendChild(objChildElement);
            }
            catch
            {
                return;
            }

        }

        /// <summary>
        /// 插入子节点(将内容以CData形式写入)
        /// </summary>
        /// <param name="parentNode">要插入的父节点</param>
        /// <param name="elementName">插入的节点名称</param>
        /// <param name="elementContent">节点内容</param>
        public void AddChildNodeCData(string parentNodePath, string elementName, string elementContent)
        {
            try
            {
                XmlNode parentNode = objXmlDoc.SelectSingleNode(parentNodePath);
                XmlElement objChildElement = objXmlDoc.CreateElement(elementName);

                //写入cData数据
                XmlCDataSection xcds = objXmlDoc.CreateCDataSection(elementContent);

                objChildElement.AppendChild(xcds);
                parentNode.AppendChild(objChildElement);
            }
            catch
            {
                return;
            }

        }


        /// <summary>
        /// 插入子节点(仅内容，不带属性)
        /// </summary>
        /// <param name="parentNode">要插入的父节点</param>
        /// <param name="elementName">插入的节点名称</param>
        /// <param name="elementContent">节点内容</param>
        public void AddChildNode(string parentNodePath, string elementName, string elementContent)
        {
            try
            {
                XmlNode parentNode = objXmlDoc.SelectSingleNode(parentNodePath);
                XmlElement objChildElement = objXmlDoc.CreateElement(elementName);

                objChildElement.InnerText = elementContent;
                parentNode.AppendChild(objChildElement);
            }
            catch
            {
                return;
            }

        }

        /// <summary>
        /// 根据xpath值查找节点
        /// </summary>
        /// <param name="NodePath">要查找节点的xpath值</param>
        /// <returns>找到返回true,否则返回true</returns>
        public bool FindNode(string NodePath)
        {
            try
            {
                if (objXmlDoc.SelectSingleNode(NodePath) != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        ///保存文档
        /// </summary>
        public void Save()
        {
            //保存文档。
            try
            {
                objXmlDoc.Save(strXmlFile);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            objXmlDoc = null;
        }





        #region 获取XmlDocument对象

        /// <summary>
        /// 根据XML文件内容获取XmlDocument对象
        /// </summary>
        /// <param name="xmlFileContent"></param>
        /// <returns></returns>
        private XmlDocument GetXmlDocByXmlContent(string xmlFileContent)
        {
            if (string.IsNullOrEmpty(xmlFileContent))
            {
                return null;
            }

            var xDoc = new XmlDocument();
            try
            {
                xDoc.LoadXml(xmlFileContent);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.ToString());
                xDoc = null;
            }
            return xDoc;
        }

        /// <summary>
        /// 根据XML文件路径获取XmlDocument对象
        /// </summary>
        /// <param name="xmlFilePath"></param>
        /// <returns></returns>
        private XmlDocument GetXmlDocByFilePath(string xmlFilePath)
        {
            if (string.IsNullOrEmpty(xmlFilePath))
            {
                return null;
            }

            var xDoc = new XmlDocument();
            try
            {
                xmlFilePath = xmlFilePath.Replace(".xml", "");
                TextAsset textAsset = (TextAsset)Resources.Load(xmlFilePath.ToString(), typeof(TextAsset));
                if (textAsset == null)
                    Debug.Log(xmlFilePath);
                xDoc.LoadXml(textAsset.text);

                //xDoc.Load(xmlFilePath);
            }
            catch
            {
                throw new Exception(string.Format("请确认该XML文件格式正确，路径为：{0}", xmlFilePath));
            }
            return xDoc;
            return null;
        }

        #endregion

        //========================================================= //


        //========================================================= //

        #region 获取XML节点（或节点列表）

        /// <summary>
        /// 获取父节点下指定节点名称的第一个子节点
        /// </summary>
        /// <param name="parentXmlNode"></param>
        /// <param name="childNodeName"></param>
        /// <returns></returns>
        public XmlNode GetFirstChildNodeByName(XmlNode parentXmlNode, string childNodeName)
        {
            var childXmlNodes = GetChildNodesByName(parentXmlNode, childNodeName);
            if (childXmlNodes != null && childXmlNodes.Count > 0)
            {
                return childXmlNodes[0];
            }

            return null;
        }

        /// <summary>
        /// 获取父节点下指定节点名称和指定属性的属性值的节点列表
        /// </summary>
        /// <param name="parentXmlNode">父节点</param>
        /// <param name="childNodeName">节点名</param>
        /// <param name="attrName">属性名</param>
        /// <param name="attrValue">属性值</param>
        /// <returns></returns>
        public List<XmlNode> GetChildNodeByNameAttrNameValue(XmlNode parentXmlNode, string childNodeName, string attrName, string attrValue)
        {
            if (parentXmlNode == null || string.IsNullOrEmpty(childNodeName) ||
                string.IsNullOrEmpty(attrName) || string.IsNullOrEmpty(attrValue))
            {
                return null;
            }
            List<XmlNode> xmlNodes = new List<XmlNode>();
            foreach (XmlNode nd in parentXmlNode.ChildNodes)
            {

                XmlElement element = (XmlElement)nd;
                if (element.GetAttribute(attrName).Equals(attrValue))
                {
                    xmlNodes.Add(nd);
                }
            }
            return xmlNodes;
        }

        /// <summary>
        /// 获取父节点下指定节点名称和指定属性的属性值的第一个子节点
        /// </summary>
        /// <param name="parentXmlNode">父节点</param>
        /// <param name="childNodeName">节点名</param>
        /// <param name="attrName">属性名</param>
        /// <param name="attrValue">属性值</param>
        /// <returns></returns>
        public XmlNode GetFirstChildNodeByNameAttrNameValue(XmlNode parentXmlNode, string childNodeName, string attrName, string attrValue)
        {
            if (parentXmlNode == null || string.IsNullOrEmpty(childNodeName) ||
                string.IsNullOrEmpty(attrName) || string.IsNullOrEmpty(attrValue))
            {
                return null;
            }
            foreach (XmlNode nd in parentXmlNode.ChildNodes)
            {
                XmlElement element = (XmlElement)nd;
                if (element.GetAttribute(attrName).Equals(attrValue))
                {
                    return nd;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据xpath值获取节点列表
        /// </summary>
        /// <param name="xpathNode"></param>
        /// <returns></returns>
        public XmlNodeList GetChildXPathNode(string xpathNode)
        {
            return objXmlDoc.SelectNodes(xpathNode);
        }

        /// <summary>
        /// 获取父节点下指定节点名称的子节点列表
        /// </summary>
        /// <param name="parentXmlNode">父节点</param>
        /// <param name="nodeName">节点名称</param>
        /// <returns></returns>
        public XmlNodeList GetChildNodesByName(XmlNode parentXmlNode, string nodeName)
        {
            if (parentXmlNode == null || string.IsNullOrEmpty(nodeName))
            {
                return null;
            }

            return GetChildNodesByXPathExpr(parentXmlNode, string.Format(".//{0}", nodeName));
        }


        /// <summary>
        /// 获取父节点下满足xpathExpr表达式的XML子节点列表
        /// </summary>
        /// <param name="parentXmlNode">父节点</param>
        /// <param name="xpathExpr"></param>
        /// <returns></returns>   
        public XmlNodeList GetChildNodesByXPathExpr(XmlNode parentXmlNode, string xpathExpr)
        {
            if (parentXmlNode == null || string.IsNullOrEmpty(xpathExpr))
            {
                return null;
            }

            return parentXmlNode.SelectNodes(xpathExpr);
        }

        /// <summary>
        /// 获取父节点下的第一个子节点
        /// </summary>
        /// <param name="parentXmlNode"></param>
        /// <returns></returns>
        public XmlNode GetFirstChildNode(XmlNode parentXmlNode)
        {
            var childXmlNodes = GetChildNodes(parentXmlNode);
            if (childXmlNodes != null && childXmlNodes.Count > 0)
            {
                return childXmlNodes[0];
            }

            return null;
        }

        /// <summary>
        /// 获取父节点的子节点列表
        /// </summary>
        /// <param name="parentXmlNode">父节点</param>
        /// <returns></returns>
        public XmlNodeList GetChildNodes(XmlNode parentXmlNode)
        {
            return parentXmlNode == null ? null : parentXmlNode.ChildNodes;
        }

        #endregion
        //========================================================= //


        //========================================================= //

        #region 读取节点属性值

        /// <summary>
        /// 读取某个XML节点的属性值（根据属性名）
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <param name="attrName"></param>
        /// <returns></returns>
        public string ReadAttrValue(XmlNode xmlNode, string attrName)
        {
            var xmlElement = xmlNode as XmlElement;

            return xmlElement == null ? null : xmlElement.GetAttribute(attrName);
        }

        /// <summary>
        /// 读取父节点下指定节点名和属性名的第一个子节点的属性值
        /// </summary>
        /// <param name="parentXmlNode">XML父节点</param>
        /// <param name="childNodeName">节点名称</param>
        /// <param name="attrName">属性名</param>
        /// <returns></returns>
        public string ReadFirstAttrValue(XmlNode parentXmlNode, string childNodeName, string attrName)
        {
            var attrVals = ReadAttrValues(parentXmlNode, childNodeName, attrName);
            return (attrVals == null || attrVals.Length == 0) ? null : attrVals[0];
        }

        /// <summary>
        /// 读取父节点下指定节点名和属性名的所有子节点的该属性值的数组
        /// </summary>
        /// <param name="parentXmlNode">XML文档</param>
        /// <param name="childNodeName">节点名称</param>
        /// <param name="attrName">属性名</param>
        /// <returns></returns>
        public string[] ReadAttrValues(XmlNode parentXmlNode, string childNodeName, string attrName)
        {
            if (parentXmlNode == null || string.IsNullOrEmpty(childNodeName) || string.IsNullOrEmpty(attrName))
            {
                return null;
            }

            var xpathExpr = string.Format("//{0}[@{1}]", childNodeName, attrName);
            var nodes = GetChildNodesByXPathExpr(parentXmlNode, xpathExpr);
            if (nodes != null && nodes.Count > 0)
            {
                var nodeCount = nodes.Count;
                var attrVals = new string[nodeCount];
                for (var i = 0; i < nodeCount; i++)
                {
                    attrVals[i] = ((XmlElement)nodes[i]).GetAttribute(attrName);
                }

                return attrVals;
            }

            return null;
        }

        #endregion

        //========================================================= //


        //========================================================= //

        #region 读取父节点下的子节点的文本内容

        /// <summary>
        /// 读取父节点下指定节点名的第一个子节点的文本
        /// </summary>
        /// <param name="parentXmlNode"></param>
        /// <param name="childNodeName"></param>
        /// <returns></returns>
        public string ReadFirstChildNodeTextByName(XmlNode parentXmlNode, string childNodeName)
        {
            var childNodeTexts = ReadChildNodeTextsByName(parentXmlNode, childNodeName);
            if (childNodeTexts != null && childNodeTexts.Length > 0)
            {
                return childNodeTexts[0];
            }

            return null;
        }

        /// <summary>
        /// 读取父节点下指定节点名的所有子节点的文本数组
        /// </summary>
        /// <param name="parentXmlNode"></param>
        /// <param name="childNodeName"></param>
        /// <returns></returns>
        public string[] ReadChildNodeTextsByName(XmlNode parentXmlNode, string childNodeName)
        {
            if (parentXmlNode == null || string.IsNullOrEmpty(childNodeName))
            {
                return null;
            }

            var xpathExpr = string.Format(".//{0}", childNodeName);
            var childNodes = GetChildNodesByXPathExpr(parentXmlNode, xpathExpr);
            if (childNodes != null && childNodes.Count > 0)
            {
                var nodeCount = childNodes.Count;
                var nodeTexts = new string[nodeCount];
                for (var i = 0; i < nodeCount; i++)
                {
                    nodeTexts[i] = childNodes[i].InnerText;
                }

                return nodeTexts;
            }

            return null;
        }

        /// <summary>
        /// 读取父节点下的第一个子节点的文本
        /// </summary>
        /// <param name="parentXmlNode"></param>
        /// <returns></returns>
        public string ReadFirstChildNodeText(XmlNode parentXmlNode)
        {
            var childNodeTexts = ReadChildNodeTexts(parentXmlNode);
            if (childNodeTexts != null && childNodeTexts.Length > 0)
            {
                return childNodeTexts[0];
            }

            return null;
        }

        /// <summary>
        /// 读取父节点下的所有子节点的文本数组
        /// </summary>
        /// <param name="parentXmlNode"></param>
        /// <returns></returns>
        public string[] ReadChildNodeTexts(XmlNode parentXmlNode)
        {
            if (parentXmlNode == null)
            {
                return null;
            }

            var childNodes = GetChildNodes(parentXmlNode);
            if (childNodes != null && childNodes.Count > 0)
            {
                var nodeCount = childNodes.Count;
                var nodeTexts = new string[nodeCount];
                for (var i = 0; i < nodeCount; i++)
                {
                    nodeTexts[i] = childNodes[i].InnerText;
                }

                return nodeTexts;
            }

            return null;
        }

        /// <summary>
        /// 读取XML节点文本
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public string ReadNodeText(XmlNode xmlNode)
        {
            if (xmlNode == null)
            {
                return null;
            }

            return xmlNode.InnerText;
        }

        #endregion

    }
}

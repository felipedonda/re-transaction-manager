using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using RETrasactionManager.Lib;

namespace RETrasactionManager.Lib.KmlFunctions
{
    public class KmlFile
    {
        public Kml Kml {get; set;}
        public async Task ILoad (Stream stream)
        {
            //XNamespace ns = "http://www.opengis.net/kml/2.2";
            XmlDocument XmlDoc = new XmlDocument();
            Console.WriteLine(" KML -> Loading stream...");
            await Task.Run( () => {XmlDoc.Load(stream);});
            Console.WriteLine(" KML -> Filling KML...");
            this.Kml = new Kml(XmlDoc);
            this.Kml.Fill();
        }
    }
    public class Kml
    {
        public Kml()
        {
            this.KmlDocument = new KmlDocument();
        }
        public Kml(KmlDocument Document)
        {
            this.KmlDocument = Document;
        }
        public Kml(XmlDocument xml)
        {
            this.XmlDocument = xml;
        }
        public XmlDocument XmlDocument {get; set;}
        public KmlDocument KmlDocument {get;set;}
        public void Fill ()
        {
            this.KmlDocument = new KmlDocument((this.XmlDocument.GetElementsByTagName("kml")[0] as XmlElement).GetElementsByTagName("Document")[0] as XmlElement);
        }
        public XmlDocument ToXml()
        {
            string ns = "http://www.opengis.net/kml/2.2";
            XmlDocument XmlDocument = new XmlDocument();
            XmlElement KmlNode = XmlDocument.CreateElement("kml",ns);
            XmlDocument.AppendChild(KmlNode);
            this.KmlDocument.ToXml(ref KmlNode,ref XmlDocument,ns);
            return XmlDocument;
        }
    }

     public class KmlContainer
    {
        public string Name {get; set;}
        public string Description {get; set;}
        public XmlElement Element {get; set;}
        public string elementName {get; set;}
        public  List<KmlFolder> Folders = new List<KmlFolder>();
        public  List<KmlDocument> Documents = new List<KmlDocument>();
        public  List<KmlPlacemark> Placemarks = new List<KmlPlacemark>();
        public KmlContainer()
        {
        }
        public KmlContainer(XmlElement node)
        {
            this.Element = node;
            this.Fill(node);
        }
        public void Fill(XmlElement node)
        {

            foreach(XmlElement child in node.ChildNodes)
            {
                switch(child.LocalName)
                {
                    case "name":
                        this.Name = child.InnerText;
                    break;
                    case "description":
                        this.Description = child.InnerText;
                    break;
                    case "Placemark":
                        KmlPlacemark placemark = new KmlPlacemark(child);
                        this.Placemarks.Add(placemark);
                    break;
                    case "Folder":
                        KmlFolder folder = new KmlFolder(child);
                        this.Folders.Add(folder);
                    break;
                    case "Document":
                        KmlDocument document = new KmlDocument(child);
                        this.Documents.Add(document);
                    break;
                }
            }
        }

        public void ToXml(ref XmlElement node,ref XmlDocument xmlDocument,string ns)
        {
            XmlElement ContainerElement = xmlDocument.CreateElement(this.elementName,ns);
            node.AppendChild(ContainerElement);
            if(this.Name != null)
            {
                XmlElement Name = xmlDocument.CreateElement("name",ns);
                Name.InnerText = this.Name;
                ContainerElement.AppendChild(Name);
            }

            if(this.Description != null)
            {
                XmlElement Description = xmlDocument.CreateElement("description",ns);
                Description.InnerText = this.Description;
                ContainerElement.AppendChild(Description);
            }

            foreach(KmlContainer Container in Folders)
            {
                Container.ToXml(ref ContainerElement,ref xmlDocument,ns);
            }

            foreach(KmlContainer Container in Documents)
            {
                Container.ToXml(ref ContainerElement,ref xmlDocument,ns);
            }

            foreach(KmlPlacemark Container in Placemarks)
            {
                Container.ToXml(ref ContainerElement,ref xmlDocument,ns);
            }
        }
        public string OutputStructure(string prefix)
        {
            string Structure = "";
            foreach(var element in Documents)
            {
                Structure += String.Format("{0}Document: {1}\n",prefix,element.Name);
                Structure += element.OutputStructure(prefix + "__");
            }
            foreach(var element in Folders)
            {
                Structure += String.Format("{0}Folder: {1}\n",prefix,element.Name);
                Structure += element.OutputStructure(prefix + "__");
            }
            foreach(var element in Placemarks)
            {
                Structure += String.Format("{0}Placemark({2}): {1}\n",prefix,element.Name,element.PlacemarkType);
            }
            return Structure;
        }

        public int GetPlacemarksCount()
        {
            int count = 0;

            count += Placemarks.Count;

            foreach(KmlContainer Container in Folders)
            {
                Container.GetPlacemarksCount();
            }

            foreach(KmlContainer Container in Documents)
            {
                Container.GetPlacemarksCount();
            }

            return count;
        }

        public int GetFoldersCount()
        {
            int count = 0;

            count += Folders.Count;

            foreach(KmlContainer Container in Folders)
            {
                Container.GetFoldersCount();
            }

            foreach(KmlContainer Container in Documents)
            {
                Container.GetFoldersCount();
            }

            return count;
        }

        public int GetKmlsCount()
        {
            int count = 0;

            count += Documents.Count;

            foreach(KmlContainer Container in Folders)
            {
                Container.GetKmlsCount();
            }

            foreach(KmlContainer Container in Documents)
            {
                Container.GetKmlsCount();
            }

            return count;
        }
    }

    public class KmlFolder : KmlContainer {
        public KmlFolder() : base()
        {
            this.elementName = "Folder";
        }
        public KmlFolder(XmlElement node) : base(node)
        {
            this.elementName = "Folder";
        }
        public KmlDocument ToKmlDocument()
        {
            KmlDocument KmlDocument = new KmlDocument();
            KmlDocument.Description = this.Description;
            KmlDocument.Name = this.Name;
            KmlDocument.Documents = this.Documents;
            KmlDocument.Folders = this.Folders;
            KmlDocument.Placemarks = this.Placemarks;
            return KmlDocument;
        }
    }
    public class KmlDocument : KmlContainer {
        public KmlDocument() : base()
        {
            this.elementName = "Document";
        }
        public KmlDocument(XmlElement node) : base(node)
        {
            this.elementName = "Document";
        }
    }

    public class KmlPlacemark
    {
        public KmlPlacemark(){}
        public KmlPlacemark(KmlPolygon polygon)
        {
            this.Polygon = polygon;
        }
        public KmlPlacemark(KmlPoint point)
        {
            this.Point = point;
        }
        public KmlPlacemark(KmlWay way)
        {
            this.Way = way;
        }
        public void ToXml(ref XmlElement node,ref XmlDocument xmlDocument,string ns)
        {
            XmlElement ContainerElement = xmlDocument.CreateElement("Placemark",ns);
            node.AppendChild(ContainerElement);

            if(this.Name != null)
            {
                XmlElement Name = xmlDocument.CreateElement("name",ns);
                Name.InnerText = this.Name;
                ContainerElement.AppendChild(Name);
            }

            if(this.Description != null)
            {
                XmlElement Description = xmlDocument.CreateElement("description",ns);
                Description.InnerText = this.Description;
                ContainerElement.AppendChild(Description);
            }

            if(this.Point != null)
            {
                this.Point.ToXml(ref ContainerElement,ref xmlDocument,ns);
            }

            if(this.Polygon != null)
            {
                this.Polygon.ToXml(ref ContainerElement,ref xmlDocument,ns);
            }

            if(this.Way != null)
            {
                this.Way.ToXml(ref ContainerElement,ref xmlDocument,ns);
            }
        }
        public KmlPlacemark(XmlElement node)
        {
            foreach(XmlElement child in node.ChildNodes)
            {
                switch(child.LocalName)
                {
                    case "name":
                        this.Name = child.InnerText;
                    break;
                    case "description":
                        this.Description = child.InnerText;
                    break;
                    case "Polygon":
                        this.Polygon = new KmlPolygon(child);
                    break;
                    case "LineString":
                        this.Way = new KmlWay(child);
                    break;
                    case "Point":
                        this.Point = new KmlPoint(child);
                    break;
                }
            }
        }
        public string Name {get; set;}
        public string Description {get; set;}
        public string PlacemarkType
        {
            get
            {
                string PlacemarkType = "";

                if(this.Point != null)
                {
                    PlacemarkType = "Point";
                }
                if(this.Way != null)
                {
                    PlacemarkType = "Way";
                }
                if(this.Polygon != null)
                {
                    PlacemarkType = "Polygon";
                }

                return PlacemarkType;
            }
        }
        public XmlElement Element {get; set;}
        public KmlPoint Point {get; set;}
        public KmlWay Way {get; set;}
        public KmlPolygon Polygon {get; set;}
    }

    public class KmlPolygon
    {
        public KmlPolygon()
        {
            this.Tessellate = 1;
        }
        public KmlPolygon(XmlElement node)
        {
            foreach(XmlElement Child in node.ChildNodes)
            {
                switch(Child.LocalName)
                {
                    case "tessellate":
                        this.Tessellate = Int32.Parse(Child.InnerText);
                    break;
                    case "outerBoundaryIs":
                        this.OuterBoundary = Coordinate.GetCoordinatesFromString(Child.FirstChild.InnerText);
                    break;
                    case "innerBoundaryIs":
                        this.InnerBoundary = Coordinate.GetCoordinatesFromString(Child.FirstChild.InnerText);
                    break;
                }
            }
        }
        public void ToXml(ref XmlElement node,ref XmlDocument xmlDocument,string ns)
        {
            XmlElement ContainerElement = xmlDocument.CreateElement("Polygon",ns);
            node.AppendChild(ContainerElement);

            XmlElement TessellateElement = xmlDocument.CreateElement("tessellate",ns);
            TessellateElement.InnerText = this.Tessellate.ToString();
            ContainerElement.AppendChild(TessellateElement);
            
            XmlElement OuterBoundary = xmlDocument.CreateElement("outerBoundaryIs",ns);
            ContainerElement.AppendChild(OuterBoundary);
            
            XmlElement OuterLinearRing = xmlDocument.CreateElement("LinearRing",ns);
            OuterBoundary.AppendChild(OuterLinearRing);

            XmlElement OuterCoordinates = xmlDocument.CreateElement("coordinates",ns);
            OuterCoordinates.InnerText = Coordinate.GetStringFromCoordinates(this.OuterBoundary);
            OuterLinearRing.AppendChild(OuterCoordinates);

            if(InnerBoundary != null)
            {
                XmlElement InnerBoundary = xmlDocument.CreateElement("innerBoundaryIs",ns);
                ContainerElement.AppendChild(InnerBoundary);
                
                XmlElement InnerLinearRing = xmlDocument.CreateElement("LinearRing",ns);
                InnerBoundary.AppendChild(InnerLinearRing);

                XmlElement InnerCoordinates = xmlDocument.CreateElement("coordinates",ns);
                InnerCoordinates.InnerText = Coordinate.GetStringFromCoordinates(this.InnerBoundary);
                InnerLinearRing.AppendChild(InnerCoordinates);
            }

        }
        public int Tessellate {get; set;}
        public List<Coordinate> OuterBoundary {get; set;}
        public List<Coordinate> InnerBoundary {get; set;}
    }

    public class KmlPoint
    {
        public KmlPoint() {}
        public KmlPoint(XmlElement node)
        {
            foreach(XmlElement Child in node.ChildNodes)
            {
                switch(Child.LocalName)
                {
                    case "coordinates":
                        this.Coordinate = new Coordinate(Child.InnerText);
                    break;
                }
            }
        }
        public Coordinate Coordinate {get; set;}

        public void ToXml(ref XmlElement node,ref XmlDocument xmlDocument,string ns)
        {
            XmlElement ContainerElement = xmlDocument.CreateElement("Point",ns);
            node.AppendChild(ContainerElement);

            XmlElement CoordinatesElement = xmlDocument.CreateElement("coordinates",ns);
            CoordinatesElement.InnerText = this.Coordinate.ToString();
            ContainerElement.AppendChild(CoordinatesElement);

        }
    }
    public class KmlWay
    {
        public KmlWay()
        {
            this.Tessellate = 1;
        }
        public KmlWay(XmlElement node)
        {
            foreach(XmlElement Child in node.ChildNodes)
            {
                switch(Child.LocalName)
                {
                    case "tessellate":
                        this.Tessellate = Int32.Parse(Child.InnerText);
                    break;
                    case "coordinates":
                        this.ListofCoordinates = Coordinate.GetCoordinatesFromString(Child.InnerText);
                    break;
                }
            }
        }
        public void ToXml(ref XmlElement node,ref XmlDocument xmlDocument,string ns)
        {
            XmlElement ContainerElement = xmlDocument.CreateElement("LineString",ns);
            node.AppendChild(ContainerElement);

            XmlElement TessellateElement = xmlDocument.CreateElement("tessellate",ns);
            TessellateElement.InnerText = this.Tessellate.ToString();
            ContainerElement.AppendChild(TessellateElement);

            XmlElement CoordinatesElement = xmlDocument.CreateElement("coordinates",ns);
            CoordinatesElement.InnerText = Coordinate.GetStringFromCoordinates(this.ListofCoordinates);
            ContainerElement.AppendChild(CoordinatesElement);
        }
        public int Tessellate {get; set;}
        public List<Coordinate> ListofCoordinates {get; set;}
    }
}
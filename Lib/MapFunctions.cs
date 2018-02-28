using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using RETrasactionManager.Lib.KmlFunctions;
using RETrasactionManager.Lib.GeojsonFunctions;

namespace RETrasactionManager.Lib
{
    public class Coordinate
    {
        public Coordinate()
        {
        }
        public Coordinate(string coordinateString)
        {
            NumberStyles style = NumberStyles.Any | NumberStyles.AllowExponent;

            //new System.Globalization.CultureInfo("en-US");??
            this.Longitude = decimal.Parse(coordinateString.Split(',')[0],style,CultureInfo.InvariantCulture);
            this.Latitude = decimal.Parse(coordinateString.Split(',')[1],style,CultureInfo.InvariantCulture);
            if((coordinateString.Length - coordinateString.Replace(",","").Length ) > 2)
            {
                this.Altitude = decimal.Parse(coordinateString.Split(',')[2],style,CultureInfo.InvariantCulture);
            }
            else
            {
                this.Altitude = 0;
            }
        }
        public Coordinate(string coordinateString,string delimiter)
        {
            NumberStyles style = NumberStyles.Any | NumberStyles.AllowExponent;

            this.Latitude = decimal.Parse(coordinateString.Split(delimiter)[0],style,CultureInfo.InvariantCulture);
            this.Longitude = decimal.Parse(coordinateString.Split(delimiter)[1],style,CultureInfo.InvariantCulture);
            if((coordinateString.Length - coordinateString.Replace(delimiter,"").Length ) > 2)
            {
                this.Altitude = decimal.Parse(coordinateString.Split(delimiter)[2],style,CultureInfo.InvariantCulture);
            }
            else
            {
                this.Altitude = 0;
            }
            
        }

        static public List<Coordinate> GetCoordinatesFromString(string lineString)
        {
            //Console.WriteLine("@ {0}",lineString);
            List<Coordinate> ListofCoordinates = new List<Coordinate>();
            foreach(string CoordinateString in lineString.Replace("\t","").Replace("\n","").Split(" "))
            {
                if(CoordinateString.Length > 0)
                {
                    //Console.WriteLine("@ {0}",System.Text.RegularExpressions.Regex.Escape(CoordinateString));
                    //Console.WriteLine("@ {0}",CoordinateString.Length);
                    ListofCoordinates.Add(new Coordinate(CoordinateString));
                }
            }
            return ListofCoordinates;
        }

        static public List<Coordinate> GetCoordinatesFromString(string lineString,string dataDelimiter,string coordinatesDelimiter)
        {
            //Console.WriteLine("@ {0}",lineString);
            List<Coordinate> ListofCoordinates = new List<Coordinate>();
            foreach(string CoordinateString in lineString.Replace("\t","").Replace("\n","").Split(dataDelimiter))
            {
                if(CoordinateString.Length > 0)
                {
                    //Console.WriteLine("@ {0}",System.Text.RegularExpressions.Regex.Escape(CoordinateString));
                    //Console.WriteLine("@ {0}",CoordinateString.Length);
                    ListofCoordinates.Add(new Coordinate(CoordinateString,coordinatesDelimiter));
                }
            }
            return ListofCoordinates;
        }

        static public string GetStringFromCoordinates(List<Coordinate> coordinates)
        {
            string CoordinatesString = "";
            foreach(Coordinate Coordinate in coordinates)
            {
                CoordinatesString += String.Format("{0} ",Coordinate.ToString());
            }
            return CoordinatesString.Substring(0,CoordinatesString.Length -1);
        }
        
        override public string ToString()
        {
            return String.Format("{0},{1},{2}",this.Longitude.ToString(CultureInfo.InvariantCulture),
                this.Latitude.ToString(CultureInfo.InvariantCulture),
                this.Altitude.ToString(CultureInfo.InvariantCulture));
            
        }
        public string ToString(string delimiter)
        {
            return String.Format("{0}{3}{1}{3}{2}",this.Latitude,this.Longitude,this.Altitude,delimiter);
        }
        public decimal Latitude {get; set;}
        public decimal Longitude {get; set;}
        public decimal Altitude {get; set;}
    }
    public class Placemark
    {
        public Placemark(){}
        public Placemark(Polygon polygon)
        {
            this.Polygon = polygon;
        }
        public Placemark(Point point)
        {
            this.Point = point;
        }
        public Placemark(Way way)
        {
            this.Way = way;
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
        public Point Point {get; set;}
        public Way Way {get; set;}
        public Polygon Polygon {get; set;}
        public KmlPlacemark ToKmlPlacemark()
        {
            KmlPlacemark KmlPlacemark = new KmlPlacemark();
            switch(this.PlacemarkType)
            {
                case "Point":
                    KmlPlacemark.Point = this.Point.ToKmlPoint();
                break;
                case "Way":
                    KmlPlacemark.Way = this.Way.ToKmlWay();
                break;
                case "Polygon":
                    KmlPlacemark.Polygon = this.Polygon.ToKmlPolygon();
                break;
            }
            return KmlPlacemark;
        }
    }

    public class Polygon
    {
        public string Name {get; set;}
        public string Description {get; set;}
        public List<Coordinate> OuterBoundary {get; set;}
        public List<Coordinate> InnerBoundary {get; set;}

        public KmlPolygon ToKmlPolygon()
        {
            KmlPolygon KmlPolygon = new KmlPolygon();
            KmlPolygon.InnerBoundary = this.InnerBoundary;
            KmlPolygon.OuterBoundary = this.OuterBoundary;
            KmlPolygon.Tessellate = 1;
            return KmlPolygon;
        }
    }

    public class Point
    {
        public string Name {get; set;}
        public string Description {get; set;}
        public Coordinate Coordinate {get; set;}

        public KmlPoint ToKmlPoint()
        {
            KmlPoint KmlPoint = new KmlPoint();
            KmlPoint.Coordinate = this.Coordinate;
            return KmlPoint;
        }
    }
    public class Way
    {
        public string Name {get; set;}
        public string Description {get; set;}
        public List<Coordinate> ListofCoordinates {get; set;}

        public KmlWay ToKmlWay()
        {
            KmlWay KmlWay = new KmlWay();
            KmlWay.ListofCoordinates = this.ListofCoordinates;
            return KmlWay;
        }
    }
    public class Layer
    {
        public string Name {get; set;}
        public string Description {get; set;}
        public List<Point> Points {get; set;}
        public List<Way> Ways {get; set;}   
        public List<Polygon> Polygons {get; set;}   
        public List<Layer> Layers {get; set;}   
        public KmlFolder ToKmlFolder()
        {
            KmlFolder KmlFolder = new KmlFolder();
            KmlFolder.Name = this.Name;
            KmlFolder.Description = this.Description;

            if(Points != null)
            {
                foreach(Point obj in Points)
                {
                    KmlPlacemark KmlPlacemark = new KmlPlacemark(new KmlPoint());
                    KmlPlacemark.Point.Coordinate = obj.Coordinate;
                    KmlPlacemark.Name = obj.Name;
                    KmlPlacemark.Description = obj.Description;
                    KmlFolder.Placemarks.Add(KmlPlacemark);
                }
            }

            if(Ways != null)
            {
                foreach(Way obj in Ways)
                {
                    KmlPlacemark KmlPlacemark = new KmlPlacemark(new KmlWay());
                    KmlPlacemark.Way.ListofCoordinates = obj.ListofCoordinates;
                    KmlPlacemark.Way.Tessellate = 1;
                    KmlPlacemark.Name = obj.Name;
                    KmlPlacemark.Description = obj.Description;
                    KmlPlacemark.Name = obj.Name;
                    KmlPlacemark.Description = obj.Description;
                    KmlFolder.Placemarks.Add(KmlPlacemark);
                }
            }
            if(Polygons != null)
            {
                foreach(Polygon obj in Polygons)
                {
                    KmlPlacemark KmlPlacemark = new KmlPlacemark(new KmlPolygon());
                    KmlPlacemark.Polygon.InnerBoundary = obj.InnerBoundary;
                    KmlPlacemark.Polygon.OuterBoundary = obj.OuterBoundary;
                    KmlPlacemark.Polygon.Tessellate = 1;
                    KmlPlacemark.Name = obj.Name;
                    KmlPlacemark.Description = obj.Description;
                    KmlFolder.Placemarks.Add(KmlPlacemark);
                }
            }

            if(Layers != null)
            {
                foreach(Layer obj in Layers)
                {
                    KmlFolder.Folders.Add(obj.ToKmlFolder());
                } 
            }

            return KmlFolder;
        }
    }
    public class Map
    {
        public Layer Root {get; set;}
        public List<Layer> Layers {get; set;}
        public string Name {get; set;}
        public string Description {get; set;}
        public Kml ToKml()
        {
            KmlDocument KmlDocument =  new KmlDocument();

            if(Root != null)
            {
                KmlDocument = Root.ToKmlFolder().ToKmlDocument();
            }

            if(!String.IsNullOrEmpty(this.Name))
            {
                KmlDocument.Name = this.Name;
            }

            if(!String.IsNullOrEmpty(this.Description))
            {
                KmlDocument.Description = this.Description;
            }

            foreach(Layer Layer in Layers)
            {
                KmlDocument.Folders.Add(Layer.ToKmlFolder());
            }
            
            return new Kml(KmlDocument);
        }
    }
}
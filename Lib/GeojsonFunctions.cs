using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using RETrasactionManager.Lib.KmlFunctions;
using RETrasactionManager.Lib;
using Newtonsoft.Json;

namespace RETrasactionManager.Lib.GeojsonFunctions
{
    public class ConvertRelations
    {
        static public Kml ConvertRelationsToKml(Stream stream)
        {
            Kml Output = new Kml();
            
            string RawData = "";

            using (StreamReader reader = new StreamReader(stream))
            {
                RawData = reader.ReadToEnd();
            }

            dynamic Json = JsonConvert.DeserializeObject(RawData);

            int relationid = 1;

            foreach(var relation in Json)
            {
                KmlFolder relationfolder = new KmlFolder();
                relationfolder.Name = String.Format("Relation{0}",relationid);
                
                KmlFolder lines = new KmlFolder();
                lines.Name = "lines";
                KmlFolder substations = new KmlFolder();
                substations.Name = "substations";

                //Console.WriteLine("\nrelation{0}: {1} member(s).",relationid,relation.members.Count);

                foreach(var member in relation.members)
                {
                    string CoordinatesString = "";
                    switch(member.type.ToString())
                    {
                        case "substation":
                            //Console.WriteLine("--substation: {0}",member.id);
                            CoordinatesString = member.geom.ToString().Split("(")[2].Split(")")[0].Replace(", ",",");
                            //Console.WriteLine("--coordinate string: {0}",CoordinatesString);
                            KmlPolygon substationpoly = new KmlPolygon();
                            substationpoly.OuterBoundary = Coordinate.GetCoordinatesFromString(CoordinatesString,","," ");

                            KmlPlacemark substation = new KmlPlacemark(substationpoly);
                            substation.Name = member.id;
                            
                            substations.Placemarks.Add(substation);
                        break;
                        case "line":
                            //Console.WriteLine("--line: {0}",member.id);
                            CoordinatesString = member.geom.ToString().Split("(")[1].Split(")")[0].Replace(", ",",");
                            //Console.WriteLine("--coordinate string: {0}",CoordinatesString);

                            KmlWay lineway = new KmlWay();
                            lineway.ListofCoordinates = Coordinate.GetCoordinatesFromString(CoordinatesString,","," ");

                            KmlPlacemark line = new KmlPlacemark(lineway);
                            line.Name = member.id;

                            line.Description = "";

                            if(!String.Equals(member.voltage.ToString(),"None"))
                            {
                                line.Description = String.Concat(line.Description, String.Format("Voltage: {0}<br/>",member.voltage) );
                            }
                            if(!String.Equals(member.cables.ToString(),"None"))
                            {
                                line.Description = String.Concat(line.Description, String.Format("Cables: {0}<br/>",member.cables) );
                            }
                            if(!String.Equals(member.length.ToString(),"None"))
                            {
                                line.Description = String.Concat(line.Description, String.Format("Length: {0:0.##}km<br/>",Decimal.Parse(member.length.ToString())/1000 ) );
                            }
                            
                            if(line.Description.Length > 5)
                            {
                                line.Description = line.Description.Substring(0,line.Description.Length - 5);
                            }

                            lines.Placemarks.Add(line);
                        break;
                    }
                }

                relationfolder.Folders.Add(lines);
                relationfolder.Folders.Add(substations);

                Output.KmlDocument.Folders.Add(relationfolder);
                relationid ++;
            }
            return Output;
        }
    } 
}
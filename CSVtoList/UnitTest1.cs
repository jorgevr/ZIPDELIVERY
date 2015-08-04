using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gnarum.Wise.ZipDeliveryDispatcher.Model;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace CSVtoList
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [DeploymentItem(@"../../TestData/FC_T_STILPU01_1H_20150805000000_20150804144001.csv", "TestData")]
        [DeploymentItem(@"../../TestData/prevdia_20150805_STILPU01.csv", "TestData")]
        public void TestMethod1()
        {

            IList<EximProdForecast> result = new List<EximProdForecast>();

            string[] filenamePath = new string[] { @"TestData\FC_T_STILPU01_1H_20150805000000_20150804144001.csv", @"TestData\prevdia_20150805_STILPU01.csv" };
            foreach (string filePath in filenamePath)
            {
                
                string line;
                int linecounter = 0;

                string fileId = Path.GetFileNameWithoutExtension(filePath).Substring(0, 4);
                int valueId = GetLocationDataValue(fileId);

                string datavariable = GetDatavariableFromFileName(fileId);
                string unit = GetUnitsFromFileName(fileId);

                StreamReader file = new System.IO.StreamReader(filePath);
                
                while ((line = file.ReadLine()) != null)
                {
                    linecounter++;
                    if (!line.Equals("*"))
                    {
                        if (fileId != "prev" && linecounter == 1 ) continue;
                        string[] lineSplitted = line.Split(';');
                        int year = int.Parse(lineSplitted[0]);
                        int month = int.Parse(lineSplitted[1]);
                        int day = int.Parse(lineSplitted[2]);
                        int hour = int.Parse(lineSplitted[3]);
                        DateTime initDateTime = new DateTime(year, month, day);
                        DateTime utcDateTime = initDateTime;
                        if (hour != 0)
                        {
                            utcDateTime = initDateTime.AddHours(hour);
                        } else
                        {
                            utcDateTime = initDateTime.AddDays(-1).AddHours(24);
                        }
                        string value = lineSplitted[valueId];
                        string finalValue = valueParsedforDataVariable(fileId, value);
                        
                        EximProdForecast forecast = new EximProdForecast();
                        forecast.UtcDateTime = utcDateTime;
                        forecast.Datavariable = datavariable;
                        forecast.DatavariableUnit = unit;
                        forecast.Value = finalValue;
                        result.Add(forecast);
                    }
                    
                }
            }

            StringBuilder sb = new StringBuilder();
            var utcDatesList = result.Select(x => new { x.UtcDateTime, x.Datavariable, x.DatavariableUnit, x.Value }).OrderBy(x => x.UtcDateTime).ToList();
            var DatesList = result.Select(x => new { x.UtcDateTime }).OrderBy(x => x.UtcDateTime).Distinct().ToList();

            foreach (var eacDate in DatesList)
            {
                var value = utcDatesList.Select(x => new { x.Value }).Equals(x => x.UtcDateTime)
                    
            }



            sb.AppendLine("DATE/TIME (UTC); DATAVARIABLE; DATAVARIABLE (UNIT); VALUE");
            for (int i =0 ; i < utcDatesList.Count; i++)
            {                

                string line = utcDatesList[i].UtcDateTime.ToString("dd/MM/yyyy HH:mm");
                string dataVariable = utcDatesList[i].Datavariable;
                string dataVariableUnits = utcDatesList[i].DatavariableUnit;
                string value = utcDatesList[i].Value;
                line += ";" + dataVariable + ";" + dataVariableUnits + ";" + value;

                sb.AppendLine(line);
            }

            
            //result.OrderBy(x => x.UtcDateTime).ToList();
        }
        

        private string valueParsedforDataVariable(string fileId, string value)
        {
            if (fileId == "FC_T")
            {
                if (value.Length < 5) value = value + "0";
                double result = Convert.ToDouble(value);
                return result.ToString().Replace(".", ",");
            }
            else
            {
                return value.Replace(".", ",");
            }
        }

        private int GetLocationDataValue(string fileId)
        {
            if (fileId == "prev")
                return 4;
            else
                return 6;
        }

        private string GetDatavariableFromFileName(string fileId)
        {
            switch(fileId)
            {
                case "FC_R":
                    return "Direct_Radiation";
                case "FC_T":
                    return "Temperature";
                case "prev":
                    return "Production";
                default:
                    return "";
            }
        }

        private string GetUnitsFromFileName(string fileId)
        {
            
            switch (fileId)
            {
                case "FC_R":
                    return "kW/m2";
                case "FC_T":
                    return "\x00B0 C";
                case "prev":
                    return "MWh";
                default:
                    return "";
            }

        }



    }
}

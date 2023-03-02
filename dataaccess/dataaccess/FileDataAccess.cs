﻿using DataAccess.Model;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SaveList = System.Collections.Generic.List<DataAccess.Model.Sale>;

namespace DataAccess.DataAccess;

public class FileDataAccess : IDataAccess
{
    public FileDataAccess(string fileName)
    {
        SaveFileName = fileName;
    }

    public Task WriteSale(Sale sale)
    {
        return Task.Run(() =>
        {
            SaveList entries = ReadFromXmlFile();
            entries.Add(sale);
            WriteToXmlFile(entries);
        });
    }

    public Task RemoveSale(Sale sale)
    {
        return Task.Run(() =>
        {
            SaveList entries = ReadFromXmlFile();
            entries.Remove(sale);
            WriteToXmlFile(entries);
        });

    }


    public static void WriteToXmlFile(SaveList entries)
    {
        using var filestream = new FileStream(SaveFileName, FileMode.Truncate);
        var xmlwriter = new XmlSerializer(typeof(SaveList));
        xmlwriter.Serialize(filestream, (SaveList)entries);
    }

    public static SaveList ReadFromXmlFile()
    {
        var entries = new SaveList();
        using (var filestream = new FileStream(SaveFileName, FileMode.OpenOrCreate))
        {
            if (filestream.Length > 0)
            {
                var xmlreader = new XmlSerializer(typeof(SaveList));
                try
                {
                    entries = (SaveList)xmlreader.Deserialize(filestream);
                }
                catch (System.InvalidOperationException)
                {
                    //TODO: Error bar at the top
                    CopyFileToErrorBackup();
                }
            }
        }

        return entries;
    }

    private static void CopyFileToErrorBackup()
    {
        int i = NextAvailableErrorFileNumber();
        File.Copy(SaveFileName, GetErrorFileName(i));
    }

    private static int NextAvailableErrorFileNumber()
    {
        int i = 0;
        while (File.Exists(path: GetErrorFileName(++i)))
        {
            if (i > 100)
            {
                // Defensive
                // Should never happen
                throw new IOException("Too many error files!");
            }
        }

        return i;
    }

    // Adds "_error<num> to cSaveFileName
    private static string GetErrorFileName(int i)
    {
        string dir = Path.GetDirectoryName(SaveFileName);
        string fileName = Path.GetFileNameWithoutExtension(SaveFileName);
        string ext = Path.GetExtension(SaveFileName);

        return Path.Combine(dir, $"{fileName}_error{i}{ext}");
    }

    public static string SaveFileName { get; set; }
}

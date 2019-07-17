using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Config
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public float ambientVolume;
    public float voicesVolume;
    public string userName;
    public string ipAddress;
    
    public Config()
    {
        masterVolume = 1.0f;
        musicVolume = 0.3f;
        sfxVolume = 1.0f;
        ambientVolume = 0.6f;
        voicesVolume = 1.0f;
        userName = "Username";
        ipAddress = "127.0.0.1";
    }


    static string endl = "\r\n";


    static public bool ReadConfig(string fileName, out Config conf)
    {
        bool success = false;
        conf = new Config();

        FileStream file = null;
        StreamReader sr = null;

        try
        {
            if(File.Exists(fileName))
            {
                file = new FileStream(fileName, FileMode.Open);
                sr = new StreamReader(file);

                string line;
                ConfigAttribute attr;
                while ((line = sr.ReadLine()) != null)
                {
                    if (ReadFloat(line, out attr))
                    {
                        if (attr.name == "master") { conf.masterVolume = attr.floatValue; }
                        if (attr.name == "music") { conf.musicVolume = attr.floatValue; }
                        if (attr.name == "soundfx") { conf.sfxVolume = attr.floatValue; }
                        if (attr.name == "ambient") { conf.ambientVolume = attr.floatValue; }
                        if (attr.name == "voices") { conf.voicesVolume = attr.floatValue; }
                    }
                    else if(ReadString(line, out attr))
                    {
                        if (attr.name == "username") { conf.userName = attr.stringValue; }
                        if (attr.name == "ip") { conf.ipAddress = attr.stringValue; }
                    }
                }
            }

            else
            {
                file = new FileStream(fileName, FileMode.Create);
                file.Dispose();
                WriteConfig(fileName, conf);
            }
            
            success = true;
        }
        catch(Exception e)
        {
            Debug.LogWarning("Error : " + e.Message);
            success = false;
        }
        finally
        {
            if (sr != null)
            {
                sr.Dispose();
                sr.Close();
            }
            if (file != null)
            {
                file.Dispose();
                file.Close();
            }
        }

        return success;
    }


    static public bool WriteConfig(string fileName, Config conf)
    {
        bool success = false;

        FileStream file = null;
        StreamWriter sr = null;

        try
        {
            file = new FileStream(fileName, FileMode.Truncate);
            sr = new StreamWriter(file);

            string str = WriteFloat("master", conf.masterVolume) + endl;
            str += WriteFloat("music", conf.musicVolume) + endl;
            str += WriteFloat("soundfx", conf.sfxVolume) + endl;
            str += WriteFloat("ambient", conf.ambientVolume) + endl;
            str += WriteFloat("voices", conf.voicesVolume) + endl;
            str += WriteString("username", conf.userName) + endl;
            str += WriteString("ip", conf.ipAddress);

            sr.Write(str);
            success = true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error  : " + e.Message);
            success = false;
        }
        finally
        {
            if (sr != null)
            {
                sr.Dispose();
                sr.Close();
            }
            if (file != null)
            {
                file.Dispose();
                file.Close();
            }
        }
       

        return success;
    }

    private static bool ReadFloat(string line, out ConfigAttribute attr)
    {
        bool success = true;
        string[] splitted = line.Split(':');
        
        float f = 0.0f;
        if (!float.TryParse(splitted[1], out f))
        {
            //Debug.LogWarning("Unable to parse float value of field \"" + splitted[0] + "\".");
            success = false; 
        }
        
        attr = new ConfigAttribute();
        attr.name = splitted[0];
        attr.floatValue = f;

        return success;
    }
    
    private static string WriteFloat(string attrName, float value)
    {
        return attrName + ":" + value.ToString();
    }


    private static bool ReadString(string line, out ConfigAttribute attr)
    {
        bool success = true;
        string[] splitted = line.Split(':');

        string s = "";
        try
        {
            s = splitted[1];
        }
        catch
        {
            success = false;
        }

        attr = new ConfigAttribute();
        attr.name = splitted[0];
        attr.stringValue = s;

        return success;
    }

    private static string WriteString(string attrName, string value)
    {
        return attrName + ":" + value.ToString();
    }
}


struct ConfigAttribute
{
    public string name;
    public string stringValue;
    public int intValue;
    public float floatValue;
    public bool boolValue;
}

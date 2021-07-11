using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace WindowsFormsApp3
{
    [Serializable]
    public class Account
    {
        [XmlElement("Name")]
        public string Name;
        [XmlElement("Login")]
        public string Login;
        [XmlElement("Password")]
        public string Password;
    }

    public class Data
    {
        BindingList<Account> acc = new BindingList<Account>();
        public int Count()
        {
            return acc.Count;
        }

        public void Add(Account data)
        {
            acc.Add(data);
        }

        public Account Return(int i)
        {
            return acc.ElementAt(i);
        }

        public void SaveData(string filename)
        {
            FileStream filestream = new FileStream(filename, FileMode.Create, access: FileAccess.Write, share: FileShare.Write);
            XmlSerializer xmls = new XmlSerializer(typeof(BindingList<Account>));
            xmls.Serialize(filestream, acc);
            filestream.Close();
        }

        public void LoadData(string filename)
        {
            FileStream filestream = new FileStream(filename, FileMode.Open, access: FileAccess.Read, share: FileShare.Read);
            XmlSerializer xmls = new XmlSerializer(typeof(BindingList<Account>));
            acc = (BindingList<Account>)xmls.Deserialize(filestream);
            filestream.Close();
        }

        public Account FindData(string name)
        {
            for (int i = 0; i < acc.Count; i++)
            {
                if (acc.ElementAt(i).Name == name)
                {
                    return acc.ElementAt(i);
                }

                if (acc.ElementAt(i).Login == name)
                {
                    return acc.ElementAt(i);
                }
                if (acc.ElementAt(i).Password == name)
                {
                    return acc.ElementAt(i);
                }
            }
            return null;
        }
    }
}

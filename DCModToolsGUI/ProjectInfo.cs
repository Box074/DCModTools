using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCModToolsGUI
{
    public class ProjectInfo
    {
        public const string ModInfoName = "__DCTGUI_modInfo__";
        public const string ModifiedAtlasName = "__MODIFIED_ATLAS__";
        public PAKWriter pak;
        public FileData infoFile;
        public DirectoryData modifiedAtlas;
        public (string ModName, int ModType) ModInfo { get; set; }
        public ProjectInfo()
        {
            pak = new();
            infoFile = new(ModInfoName, Encoding.UTF8.GetBytes("Test Mod\n0"));
            pak.root.AddEntry(infoFile);
            modifiedAtlas = new(ModifiedAtlasName);
            pak.root.AddEntry(modifiedAtlas);

            OnLoad();
        }
        public ProjectInfo(PAKReader reader)
        {
            pak = new(reader);
            infoFile = pak.root.files.First(x => x.name == ModInfoName);
            modifiedAtlas = pak.root.directories.First(x => x.name == ModifiedAtlasName);

            OnLoad();
        }
        public void OnSave()
        {
            infoFile.data = Encoding.UTF8.GetBytes(ModInfo.ModName.Trim() + "\n" + ModInfo.ModType);
        }
        public void OnLoad()
        {
            var info = Encoding.UTF8.GetString(infoFile.data).Split('\n');
            ModInfo = (info[0].Trim(), info.Length > 1 ? (int.TryParse(info[1].Trim(), out var categoryIndex) ? categoryIndex : 0) : 0);
        }
        public void RemoveAtlasDir(string name)
        {
            modifiedAtlas.directories.RemoveAll(x => x.name == name);
        }
        public DirectoryData GetAtlasDir(string name)
        {
            var result = modifiedAtlas.directories.FirstOrDefault(x => x.name == name);
            if (result == null)
            {
                result = new(name);
                modifiedAtlas.AddEntry(result);
            }
            return result;
        }
    }
}

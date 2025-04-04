using System.Text;
using UnityEngine;

namespace Moths.Internal.Packages
{
    [System.Serializable]
    public struct PackageData
    {
        public string name;
        public string displayName;
        public Version version;
        public UnityVersion unity;
        [TextArea(2, 3)]
        public string description;
        public Author author;
        public Dependency[] dependencies;
        public Change[] changelog;
        public Sample[] samples;
        public License license;

    }

    [System.Serializable]
    public struct Author
    {
        public string name;
        public string email;
        public string url;
    }

    [System.Serializable]
    public struct Dependency
    {
        public string name;
        public Version version;

        public override string ToString()
        {
            return $"\"{name}\": \"{version}\"";
        }
    }

    [System.Serializable]
    public struct Version
    {
        public int v0;
        public int v1;
        public int v2;

        public override string ToString()
        {
            return $"{v0}.{v1}.{v2}";
        }
    }

    [System.Serializable]
    public enum UnityVersion
    {
        v2021,
        v2022,
        v2023,
        v6000,
    };

    [System.Serializable]
    public struct Change
    {
        public Version version;
        public string description;

        public override string ToString()
        {
            return $"\"{version}\": \"{description}\"";
        }
    }

    [System.Serializable]
    public struct Sample
    {
        public string displayName;
        public string description;
        public string path;
    }

    public enum License
    {
        MIT,
        Apache2,
        GPL3,
        LGPL3,
        BSD2,
        BSD3,
        MPL2,
        ISC,
        CC0,
        EULA
    }

    public static class Extensions
    {
        public static string UnityVersionString(this UnityVersion unityVer)
        {
            switch(unityVer)
            {
                case UnityVersion.v2021:
                    return "2021.3";
                case UnityVersion.v2022:
                    return "2022.3";
                case UnityVersion.v2023:
                    return "2023.3";
                case UnityVersion.v6000:
                    return "6000.0";
            }
            return string.Empty;
        }
    }

    [CreateAssetMenu(menuName = "Moths/Internal/Packages/Package")]
    public class Package : ScriptableObject
    {
        [SerializeField] PackageData _package;


        public override string ToString()
        {

            StringBuilder builder = new StringBuilder();


            builder.AppendLine("{");

            builder.AppendLine($"\t\"name\": \"{_package.name}\",");

            builder.AppendLine($"\t\"displayName\": \"{_package.displayName}\",");

            builder.AppendLine($"\t\"version\": \"{_package.version}\",");

            builder.AppendLine($"\t\"unity\": \"{_package.unity.UnityVersionString()}\",");

            builder.AppendLine($"\t\"description\": \"{_package.description}\",");

            builder.AppendLine($"\t\"author\": {{");
            builder.AppendLine($"\t\t\"name\": \"{_package.author.name}\",");
            builder.AppendLine($"\t\t\"email\": \"{_package.author.email}\",");
            builder.AppendLine($"\t\t\"url\": \"{_package.author.url}\"");
            builder.AppendLine($"\t}},");

            builder.AppendLine($"\t\"dependencies\": {{");
            for (int i = 0; i < _package.dependencies.Length; i++)
            {
                builder.Append($"\t\t{_package.dependencies[i]}");
                if (i < _package.dependencies.Length - 1) builder.AppendLine(",");
                else builder.AppendLine("");
            }
            builder.AppendLine($"\t}},");

            builder.AppendLine($"\t\"changelog\": {{");
            for (int i = 0; i < _package.changelog.Length; i++)
            {
                builder.Append($"\t\t{_package.changelog[i]}");
                if (i < _package.changelog.Length - 1) builder.AppendLine(",");
                else builder.AppendLine("");
            }
            builder.AppendLine($"\t}},");

            builder.AppendLine($"\t\"samples\": [");
            for (int i = 0; i < _package.samples.Length; i++)
            {
                builder.AppendLine("\t\t{");
                builder.AppendLine($"\t\t\t\"displayName\": \"{_package.samples[i].displayName}\",");
                builder.AppendLine($"\t\t\t\"description\": \"{_package.samples[i].description}\",");
                builder.AppendLine($"\t\t\t\"path\": \"{_package.samples[i].path}\"");
                builder.Append("\t\t}");
                if (i < _package.samples.Length - 1) builder.AppendLine(",");
                else builder.AppendLine("");
            }
            builder.AppendLine($"\t],");

            builder.AppendLine($"\t\"license\": \"{_package.license}\"");

            builder.AppendLine("}");

            return builder.ToString();
        }
    }
}
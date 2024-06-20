using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Tools
{
    public static class AssetMapper
    {
        public static Asset MapV1YamlToAsset(dynamic yamlProj, string screenshotBasePath)
        {
            var asset = new Asset();
            asset.ImportV1 = true;
            asset.Enabled = true;
            asset.Id = yamlProj["id"];
            asset.Name = yamlProj["name"];
            asset.Tagline = yamlProj["tagline"];
            asset.Authors = ExtractAuthors(yamlProj);
            asset.BusinessProblem = yamlProj["business_problem"];
            asset.BusinessValue = yamlProj["business_value"];
            asset.Description = yamlProj["accelerator_description"];
            asset.ModelingApproachAndTraining = yamlProj["modeling_approach_and_training"];
            asset.Data = yamlProj["data"];
            asset.Architecture = yamlProj["architecture"];
            asset.Tags = SafeExtractList(yamlProj["tags"]);
            asset.Industries = SafeExtractList(yamlProj["industries"]);             
            ProcessImages(yamlProj, "images", asset, screenshotBasePath);
            ProcessLinks(yamlProj, "links", asset);
            asset.LastChangedBy = CreateSystemMetaData();
            asset.CreatedBy = CreateSystemMetaData();
            asset.ReleasedBy = CreateSystemMetaData();

            var assetType = yamlProj["assetType"];
            if (assetType != AssetType.Accelerator && assetType != AssetType.Demo)
            {
                throw new Exception($"Invalid asset type {assetType} for asset {asset.Name}");
            }

            asset.AssetType = new List<string> { assetType };
            
            if (!string.IsNullOrEmpty(asset.DemoUrl) && assetType != AssetType.Demo)
                asset.AssetType.Add(AssetType.Demo);
            
            return asset;
        }

        private static UserActionMetadata CreateSystemMetaData() => new UserActionMetadata
        {
            DisplayName = "Asset Importer",
            On = DateTime.Now,
            Mail = "franspa@microsoft.com",
            ObjectId = ""
        };

        private static List<string> SafeExtractList(dynamic element)
        {
            var values = new List<string>();
            if (element.GetType() == typeof(string))
            {
                values.Add(element);
            }
            else
            {
                foreach (var line in element)
                {
                    values.Add(line);
                }
            }
            return values;
        }

        private static List<Author> ExtractAuthors(dynamic yamlProj)
        {
            var authors = new List<Author>();
            foreach (var author in yamlProj["authors"])
            {
                authors.Add(new Author
                {
                    Name = author["name"],
                    GitHubAlias = author["github_alias"],
                    Email = SafeDynamicRead(author, "email")
                });
            }
            return authors;
        }

        private static void ProcessLinks(dynamic yamlProj, string nodeName, Asset asset)
        {
            if (!yamlProj.ContainsKey(nodeName)) return;
            asset.DemoUrl = SafeDynamicRead(yamlProj[nodeName], "demo");
            asset.RepositoryUrl = SafeDynamicRead(yamlProj[nodeName], "source_code");
            asset.ArmTemplate = SafeDynamicRead(yamlProj[nodeName], "arm_template");
        }

        private static void ProcessImages(dynamic yamlProj, string nodeName, Asset asset, string screenshotBasePath)
        {
            if (!yamlProj.ContainsKey(nodeName) || string.IsNullOrEmpty(SafeDynamicRead(yamlProj[nodeName], "screenshot_filename"))) return;
            asset.Screenshot = new Image() { Path = screenshotBasePath + "/" + SafeDynamicRead(yamlProj[nodeName], "screenshot_filename") };
        }

        private static string SafeDynamicRead(dynamic obj, string propName)
        {
            if (obj.ContainsKey(propName))
                return obj[propName];
            return string.Empty;
        }
    }
}
